using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ink.Runtime;
using Revive.Scenarios;
namespace Revive.Game
{
    public sealed class NarrativeChoice
    {
        public int Index { get; }
        public string Text { get; }
        public string ActionId { get; }
        internal NarrativeChoice(int index, string text, string actionId) { Index = index; Text = text; ActionId = actionId; }
    }
    /// <summary>Bridges validated Ink choice tags to data action IDs, never button text.</summary>
    public sealed class InkNarrative
    {
        private readonly Story story;
        private readonly ScenarioData scenario;
        private IReadOnlyCollection<string> observed = Array.Empty<string>();
        public InkNarrative(ScenarioData scenario)
        {
            this.scenario = scenario;
            story = new Story(scenario.Story.storyJson);
            story.BindExternalFunction<string>("has_observed", id => observed.Contains(id), true);
            story.onError += (message, type) => { if (type == Ink.ErrorType.Error) throw new InvalidOperationException(message); };
            foreach (var consequence in scenario.Consequences)
                if (story.KnotContainerWithName(consequence.explanationKnot) == null)
                    throw new InvalidOperationException("Missing explanation knot: " + consequence.explanationKnot);
        }
        public string Present(GameState state)
        {
            story.variablesState["complete"] = state.Complete;
            observed = state.Observed;
            if (state.Complete) return scenario.CompletionMessage;
            story.ChoosePathString(scenario.Questions[state.QuestionIndex].inkKnot); return Read();
        }
        public IReadOnlyList<NarrativeChoice> Choices => story.currentChoices.Select(c =>
        {
            var ids = (c.tags ?? new List<string>()).Where(t => t.StartsWith("action:", StringComparison.Ordinal)).ToArray();
            if (ids.Length != 1) throw new InvalidOperationException("Each Ink choice requires one action tag.");
            var id = ids[0].Substring(7).Trim(); scenario.Action(id);
            return new NarrativeChoice(c.index, c.text.Trim(), id);
        }).ToArray();
        public string Choose(int index)
        {
            var choice = Choices.Single(c => c.Index == index);
            story.ChooseChoiceIndex(index); Read(); return choice.ActionId;
        }
        public string Explain(string knot) { story.ChoosePathString(knot); return Read(); }
        private string Read()
        {
            var text = new StringBuilder();
            while (story.canContinue) text.Append(story.Continue());
            return text.ToString().Trim();
        }
    }
}
