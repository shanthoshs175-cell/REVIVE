using System.Collections;
using Revive.Game;
using Revive.Reasoning;
using Revive.Scenarios;
using UnityEngine;
namespace Revive.Presentation
{
    /// <summary>Quiet synthesized placeholders. Replace clips later without changing gameplay.</summary>
    public sealed class ScenarioAudio : MonoBehaviour
    {
        [SerializeField] private ScenarioSession session;
        private AudioSource ambient, feedback;
        private AudioClip room, positive, negative, interaction, communication;
        public bool Muted { get; private set; }
        private void Awake()
        {
            ambient = gameObject.AddComponent<AudioSource>(); feedback = gameObject.AddComponent<AudioSource>();
            room = Tone("Concourse ambience", 90, 2, .015f, true);
            positive = Tone("Confirmation", 660, .22f, .09f, false);
            negative = Tone("Pause cue", 220, .3f, .08f, false);
            interaction = Tone("Observation cue", 440, .1f, .045f, false);
            communication = Tone("Request connection", 520, .35f, .055f, false);
            ambient.clip = room; ambient.loop = true; ambient.Play();
        }
        private void OnEnable() => session.DecisionPresented += React;
        private void OnDisable() => session.DecisionPresented -= React;
        public void ToggleMute() { Muted = !Muted; ambient.mute = feedback.mute = Muted; }
        private void React(DecisionResult result) { StopAllCoroutines(); StartCoroutine(Cue(result)); }
        private IEnumerator Cue(DecisionResult result)
        {
            yield return new WaitForSecondsRealtime(result.Consequence.presentationDuration * .5f);
            feedback.PlayOneShot(result.Consequence.worldResponse == WorldResponse.Call ? communication : result.Consequence.kind == ConsequenceKind.Unsafe ? negative : positive);
        }
        private static AudioClip Tone(string name, float hz, float seconds, float volume, bool loop)
        {
            const int rate = 22050;
            var samples = new float[Mathf.RoundToInt(rate * seconds)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate;
                float envelope = loop ? 1 : Mathf.Sin(Mathf.PI * i / samples.Length);
                samples[i] = (Mathf.Sin(2 * Mathf.PI * hz * t) + .2f * Mathf.Sin(2 * Mathf.PI * hz * 2 * t)) * volume * envelope;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private void OnDestroy() { Destroy(room); Destroy(positive); Destroy(negative); Destroy(interaction); Destroy(communication); }
    }
}
