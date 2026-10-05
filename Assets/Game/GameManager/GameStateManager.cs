using System;
using System.Collections.Generic;
using System.Linq;
using Revive.Reasoning;
using Revive.Scenarios;
namespace Revive.Game
{
    public sealed class GameState
    {
        public int Score { get; internal set; }
        public int Mistakes { get; internal set; }
        public long PenaltyPoints { get; internal set; }
        public bool Complete { get; internal set; }
        public IReadOnlyDictionary<string, bool> Facts { get; internal set; }
        public IReadOnlyCollection<string> Observed => observed.AsReadOnly();
        internal readonly List<string> observed = new List<string>();
    }
    /// <summary>Owns one attempt. UI, audio and progress subscribe without becoming engine dependencies.</summary>
    public sealed class GameStateManager
    {
        private readonly ScenarioData scenario;
        private readonly List<Fact> baseFacts;
        // Keep negative balances so early penalties cannot vanish at the display floor.
        private long scoreBalance;
        private readonly DecisionEngine decisions = new DecisionEngine();
        public GameState State { get; } = new GameState();
        public event Action<string> OnPlayerAction;
        public event Action<IReadOnlyDictionary<string, bool>> OnFactsUpdated;
        public event Action<GameState> OnStateChanged;
        public event Action<DecisionResult> OnConsequenceReady;
        public GameStateManager(ScenarioData scenario)
        {
            scenario.Validate(); this.scenario = scenario;
            baseFacts = scenario.InitialFacts.ToList();
            State.Facts = new ForwardChainer().Infer(baseFacts, scenario.Rules).Facts;
        }
        public DecisionResult Act(string actionId)
        {
            if (State.Complete) throw new InvalidOperationException("This attempt is complete.");
            var action = scenario.Action(actionId);
            if (action.kind == ActionKind.Observe && State.observed.Contains(action.observationId))
                throw new InvalidOperationException("Observation already recorded; no repeat points.");
            var decision = decisions.Evaluate(scenario, baseFacts, actionId);
            var proposed = baseFacts.Concat(decision.PerceivedFacts).Concat(decision.Consequence.effects).ToList();
            var inferred = new ForwardChainer().Infer(proposed, scenario.Rules);
            // Validation above is atomic: failed inference cannot partially update a session.
            OnPlayerAction?.Invoke(actionId);
            baseFacts.Clear(); baseFacts.AddRange(proposed);
            if (action.kind == ActionKind.Observe) State.observed.Add(action.observationId);
            State.Facts = inferred.Facts;
            State.Complete = decision.Consequence.completesScenario;
            if (decision.Consequence.kind == ConsequenceKind.Unsafe) State.Mistakes++;
            scoreBalance += decision.Consequence.scoreDelta + (State.Complete ? scenario.CompletionBonus : 0);
            if (decision.Consequence.scoreDelta < 0) State.PenaltyPoints -= (long)decision.Consequence.scoreDelta;
            State.Score = ScoreCalculator.Apply(scoreBalance, 0, 0, scenario.MaximumScore);
            OnFactsUpdated?.Invoke(State.Facts);
            OnStateChanged?.Invoke(State);
            OnConsequenceReady?.Invoke(decision);
            return decision;
        }
    }
}
