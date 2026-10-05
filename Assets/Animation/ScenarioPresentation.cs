using Revive.Game;
using Revive.Reasoning;
using Revive.Scenarios;
using UnityEngine;
namespace Revive.Presentation
{
    /// <summary>Animation driver; receives outcomes without deciding their meaning.</summary>
    [RequireComponent(typeof(Animator))]
    public sealed class ScenarioPresentation : MonoBehaviour
    {
        [SerializeField] private ScenarioSession session;
        [SerializeField] private float incidentProgress;
        [SerializeField] private float reactionStrength;
        private Animator animator;
        private float responseStarted, responseDuration;
        public WorldResponse? Response { get; private set; }
        public float ResponseProgress => Response.HasValue ? Mathf.Clamp01((Time.unscaledTime - responseStarted) / responseDuration) : 0;
        public bool AssessmentVisible { get; private set; }
        public bool CommunicationVisible { get; private set; }
        public float IncidentProgress => ReducedMotion ? 1 : incidentProgress;
        public float ReactionStrength => ReducedMotion ? 0 : reactionStrength;
        public bool Ready => incidentProgress >= .99f || ReducedMotion;
        public bool ReducedMotion { get; set; }
        private void Awake() => animator = GetComponent<Animator>();
        private void OnEnable() { session.DecisionPresented += React; session.Restarted += Restart; session.Refreshed += Refresh; }
        private void OnDisable() { session.DecisionPresented -= React; session.Restarted -= Restart; session.Refreshed -= Refresh; }
        private void Restart() { Response = null; AssessmentVisible = CommunicationVisible = false; animator.Play("Arrival", 0, 0); }
        private void Refresh() { if (!session.IsPresenting && !session.AwaitingContinue) Response = null; }
        private void React(DecisionResult result)
        {
            Response = result.Consequence.worldResponse;
            responseStarted = Time.unscaledTime; responseDuration = result.Consequence.presentationDuration;
            if (result.Consequence.advancesQuestion && Response == WorldResponse.Survey) AssessmentVisible = true;
            if (result.Consequence.advancesQuestion && Response == WorldResponse.Call) CommunicationVisible = true;
            animator.SetTrigger(result.Consequence.kind == ConsequenceKind.Unsafe ? "Warn" : "Confirm");
        }
    }
}
