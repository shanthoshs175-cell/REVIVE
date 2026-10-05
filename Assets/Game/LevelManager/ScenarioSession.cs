using System;
using Revive.Scenarios;
using Revive.Reasoning;
using UnityEngine;
namespace Revive.Game
{
    /// <summary>Scene composition root. The same session runs any valid ScenarioData/Ink pair.</summary>
    public sealed class ScenarioSession : MonoBehaviour
    {
        [SerializeField] private ScenarioData scenario;
        private IProgressStore progress;
        public ScenarioData Scenario => scenario;
        public GameStateManager Manager { get; private set; }
        public InkNarrative Narrative { get; private set; }
        public string Prompt { get; private set; }
        public string Explanation { get; private set; }
        public DecisionResult LastDecision { get; private set; }
        public bool AwaitingContinue { get; private set; }
        public int BestScore => progress.BestScore(scenario.Id);
        public event Action Refreshed;
        public event Action Restarted;
        public event Action<DecisionResult> DecisionPresented;
        private void Awake() { progress = new PlayerPrefsProgressStore(); Restart(); }
        public void Restart()
        {
            Manager = new GameStateManager(scenario); Narrative = new InkNarrative(scenario);
            Manager.OnStateChanged += state => { if (state.Complete) progress.Record(scenario.Id, state.Score); };
            Prompt = Narrative.Present(Manager.State); Explanation = ""; LastDecision = null; AwaitingContinue = false;
            Restarted?.Invoke(); Refreshed?.Invoke();
        }
        public void Select(int choiceIndex)
        {
            if (AwaitingContinue || Manager.State.Complete) return;
            string id = Narrative.Choose(choiceIndex);
            LastDecision = Manager.Act(id);
            Explanation = Narrative.Explain(LastDecision.Consequence.explanationKnot);
            AwaitingContinue = true;
            DecisionPresented?.Invoke(LastDecision); Refreshed?.Invoke();
        }
        public void Continue()
        {
            if (!AwaitingContinue) return;
            AwaitingContinue = false; Prompt = Narrative.Present(Manager.State); Refreshed?.Invoke();
        }
    }
}
