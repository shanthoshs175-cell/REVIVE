using System;
using System.Collections;
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
        public bool IsPresenting { get; private set; }
        public bool NextLevelUnlocked => progress.IsUnlocked(scenario.UnlocksScenarioId);
        public bool AwaitingContinue { get; private set; }
        public int BestScore => progress.BestScore(scenario.Id);
        public event Action Refreshed;
        public event Action Restarted;
        public event Action<DecisionResult> DecisionPresented;
        private void Awake()
        {
            if (scenario == null)
            {
#if UNITY_EDITOR
                scenario = UnityEditor.AssetDatabase.LoadAssetAtPath<ScenarioData>("Assets/Data/ScenarioData/Level01_SceneSafety.asset");
#else
                scenario = Resources.Load<ScenarioData>("ScenarioData/Level01_SceneSafety");
#endif
            }
            progress = new PlayerPrefsProgressStore();
            Restart();
        }
        public void Restart()
        {
            StopAllCoroutines(); IsPresenting = false;
            Manager = new GameStateManager(scenario); Narrative = new InkNarrative(scenario);
            Manager.OnStateChanged += state => { if (state.Complete) progress.Record(scenario.Id, state.Score, scenario.UnlocksScenarioId); };
            Prompt = Narrative.Present(Manager.State); Explanation = ""; LastDecision = null; AwaitingContinue = false;
            Restarted?.Invoke(); Refreshed?.Invoke();
        }
        public void Select(int choiceIndex)
        {
            if (IsPresenting || AwaitingContinue || Manager.State.Complete) return;
            string id = Narrative.Choose(choiceIndex);
            LastDecision = Manager.Act(id);
            Explanation = Narrative.Explain(LastDecision.Consequence.explanationKnot);
            IsPresenting = true;
            StartCoroutine(ShowFeedbackAfterResponse(LastDecision.Consequence.presentationDuration));
            DecisionPresented?.Invoke(LastDecision); Refreshed?.Invoke();
        }
        private IEnumerator ShowFeedbackAfterResponse(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            IsPresenting = false; AwaitingContinue = true; Refreshed?.Invoke();
        }
        public void Continue()
        {
            if (!AwaitingContinue) return;
            Manager.ResolveDecision();
            AwaitingContinue = false; Prompt = Narrative.Present(Manager.State); Refreshed?.Invoke();
        }
    }
}
