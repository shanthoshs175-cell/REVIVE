using System;
using System.Collections.Generic;
using System.Linq;
using Revive.Scenarios;
namespace Revive.Reasoning
{
    public sealed class DecisionResult
    {
        public ScenarioAction Action { get; }
        public Consequence Consequence { get; }
        public InferenceResult Inference { get; }
        public IReadOnlyList<Fact> PerceivedFacts { get; }
        public DecisionResult(ScenarioAction action, Consequence consequence, InferenceResult inference, Fact[] perceived)
        { Action = action; Consequence = consequence; Inference = inference; PerceivedFacts = Array.AsReadOnly(perceived); }
    }
    /// <summary>Perception reveals only the observation selected by the player.</summary>
    public sealed class PerceptionModule
    {
        public Fact[] Observe(ScenarioData scenario, ScenarioAction action) => action.kind == ActionKind.Observe
            ? scenario.Observations.Single(o => o.id == action.observationId).reveals.ToArray() : Array.Empty<Fact>();
    }
    public sealed class DecisionEngine
    {
        private readonly ForwardChainer chainer = new ForwardChainer();
        private readonly PerceptionModule perception = new PerceptionModule();
        public DecisionResult Evaluate(ScenarioData scenario, IEnumerable<Fact> baseFacts, string actionId)
        {
            var action = scenario.Action(actionId);
            var initial = baseFacts.ToArray();
            if (!RuleEngine.Matches(chainer.Infer(initial, scenario.Rules).Facts, action.requirements))
                throw new InvalidOperationException("Action prerequisites are not known.");
            var revealed = perception.Observe(scenario, action);
            var inferred = chainer.Infer(initial.Concat(revealed).Concat(action.intent), scenario.Rules);
            var outcome = action.outcomes.Select(scenario.Outcome).FirstOrDefault(c => RuleEngine.Matches(inferred.Facts, c.conditions));
            if (outcome == null) throw new InvalidOperationException("No outcome for action: " + actionId);
            return new DecisionResult(action, outcome, inferred, revealed);
        }
    }
}
