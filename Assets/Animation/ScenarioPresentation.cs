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
        public float IncidentProgress => ReducedMotion ? 1 : incidentProgress;
        public float ReactionStrength => ReducedMotion ? 0 : reactionStrength;
        public bool Ready => incidentProgress >= .99f || ReducedMotion;
        public bool ReducedMotion { get; set; }
        private void Awake() => animator = GetComponent<Animator>();
        private void OnEnable() { session.DecisionPresented += React; session.Restarted += Restart; }
        private void OnDisable() { session.DecisionPresented -= React; session.Restarted -= Restart; }
        private void Restart() { animator.Play("Arrival", 0, 0); }
        private void React(DecisionResult result)
        { animator.SetTrigger(result.Consequence.kind == ConsequenceKind.Unsafe ? "Warn" : "Confirm"); }
    }
}
