using System;
using System.Collections.Generic;
using System.Linq;
using Ink.UnityIntegration;
using Revive.Reasoning;
using UnityEngine;
namespace Revive.Scenarios
{
    public enum ActionKind { Observe, Decide }
    public enum ConsequenceKind { Observation, Safe, Unsafe, Incomplete }
    public enum WorldResponse { Approach, Survey, Reach, Leave, DirectBystander, Call, Monitor }
    [Serializable] public sealed class ScenarioQuestion
    {
        public string id;
        public string inkKnot;
        public string[] actionIds = Array.Empty<string>();
    }
    [Serializable] public sealed class Observation
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public Fact[] reveals = Array.Empty<Fact>();
        public Vector2 hotspot;
    }
    [Serializable] public sealed class ScenarioAction
    {
        public string id;
        public ActionKind kind;
        public string observationId;
        public Fact[] requirements = Array.Empty<Fact>();
        public Fact[] intent = Array.Empty<Fact>();
        public string[] outcomes = Array.Empty<string>();
    }
    [Serializable] public sealed class Consequence
    {
        public string id;
        public Fact[] conditions = Array.Empty<Fact>();
        public Fact[] effects = Array.Empty<Fact>();
        public ConsequenceKind kind;
        public string title;
        public string explanationKnot;
        public string sourceId;
        public int scoreDelta;
        public bool completesScenario;
        public bool advancesQuestion;
        public WorldResponse worldResponse;
        public string presentationCaption;
        public float presentationDuration = 3f;
    }
    [Serializable] public sealed class SourceReference
    {
        public string id;
        public string title;
        public string url;
        public string pages;
        public string shortTitle;
    }
    /// <summary>Authoritative scenario content. Medical rules belong here, never in the engine or UI.</summary>
    [CreateAssetMenu(menuName = "REVIVE/Scenario Data")]
    public sealed class ScenarioData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string environment;
        [SerializeField, TextArea] private string incident;
        [SerializeField, TextArea] private string learningObjective;
        [SerializeField] private int difficulty = 1;
        [SerializeField] private string viewpointCaption;
        [SerializeField, TextArea] private string completionSummary;
        [SerializeField] private InkFile story;
        [SerializeField] private Fact[] initialFacts = Array.Empty<Fact>();
        [SerializeField] private Observation[] observations = Array.Empty<Observation>();
        [SerializeField] private ScenarioAction[] availableActions = Array.Empty<ScenarioAction>();
        [SerializeField] private Rule[] rules = Array.Empty<Rule>();
        [SerializeField] private ScenarioQuestion[] questions = Array.Empty<ScenarioQuestion>();
        [SerializeField] private string unlocksScenarioId;
        [SerializeField] private string completionTitle;
        [SerializeField] private string completionMessage;
        [SerializeField] private string[] correctActions = Array.Empty<string>();
        [SerializeField] private Consequence[] consequences = Array.Empty<Consequence>();
        [SerializeField] private SourceReference[] sources = Array.Empty<SourceReference>();
        [SerializeField] private int completionBonus = 20;
        [SerializeField] private int maximumScore = 100;
        public IReadOnlyList<ScenarioQuestion> Questions => questions;
        public string UnlocksScenarioId => unlocksScenarioId;
        public string CompletionTitle => completionTitle;
        public string CompletionMessage => completionMessage;
        public string Id => id;
        public string Title => title;
        public string Environment => environment;
        public string Incident => incident;
        public string LearningObjective => learningObjective;
        public int Difficulty => difficulty;
        public string ViewpointCaption => viewpointCaption;
        public string CompletionSummary => completionSummary;
        public InkFile Story => story;
        public IReadOnlyList<Fact> InitialFacts => initialFacts;
        public IReadOnlyList<Observation> Observations => observations;
        public IReadOnlyList<ScenarioAction> Actions => availableActions;
        public IReadOnlyList<Rule> Rules => rules;
        public IReadOnlyList<Consequence> Consequences => consequences;
        public IReadOnlyList<SourceReference> Sources => sources;
        public int CompletionBonus => completionBonus;
        public int MaximumScore => maximumScore;
        public ScenarioAction Action(string actionId) => availableActions.Single(a => a.id == actionId);
        public Consequence Outcome(string outcomeId) => consequences.Single(c => c.id == outcomeId);
        /// <summary>Reject malformed content before a session starts.</summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id) || story == null || !story.isCompiled || story.hasErrors)
                throw new InvalidOperationException("Scenario needs an ID and a compiled InkFile.");
            Unique(availableActions.Select(a => a.id)); Unique(consequences.Select(c => c.id));
            Unique(observations.Select(o => o.id)); Unique(rules.Select(r => r.Id)); Unique(sources.Select(s => s.id));
            if (maximumScore <= 0 || completionBonus < 0) throw new InvalidOperationException("Invalid score limits.");
            foreach (var action in availableActions)
            {
                if (action.outcomes.Length == 0) throw new InvalidOperationException("Action needs outcomes.");
                foreach (var key in action.outcomes) Outcome(key);
                if (action.kind == ActionKind.Observe && !observations.Any(o => o.id == action.observationId))
                    throw new InvalidOperationException("Unknown observation.");
            }
            Unique(questions.Select(q => q.id));
            if (questions.Length == 0) throw new InvalidOperationException("Scenario needs questions.");
            foreach (var question in questions)
            {
                if (question.actionIds.Length != 4 || question.actionIds.Distinct().Count() != 4 || string.IsNullOrWhiteSpace(question.inkKnot))
                    throw new InvalidOperationException("Each question requires four distinct actions and an Ink knot.");
                foreach (var key in question.actionIds) Action(key);
                if (question.actionIds.Count(key => correctActions.Contains(key)) != 1)
                    throw new InvalidOperationException("Each question requires one correct action.");
            }
            foreach (var outcome in consequences)
                if (outcome.presentationDuration <= 0 || string.IsNullOrWhiteSpace(outcome.presentationCaption))
                    throw new InvalidOperationException("Every outcome needs a world response and caption.");
            foreach (var key in correctActions) Action(key);
            foreach (var rule in rules)
                if (!sources.Any(s => s.id == rule.SourceId)) throw new InvalidOperationException("Rule has no source: " + rule.Id);
            foreach (var outcome in consequences)
                if (string.IsNullOrWhiteSpace(outcome.explanationKnot) || !sources.Any(s => s.id == outcome.sourceId))
                    throw new InvalidOperationException("Outcome needs an explanation and source.");
            new ForwardChainer().Infer(initialFacts, rules);
        }
        private static void Unique(IEnumerable<string> ids)
        {
            var values = ids.ToArray();
            if (values.Any(string.IsNullOrWhiteSpace) || values.Distinct().Count() != values.Length)
                throw new InvalidOperationException("Content IDs must be nonempty and unique.");
        }
    }
}
