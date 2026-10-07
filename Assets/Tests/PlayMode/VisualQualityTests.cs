using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Revive.Game;
using Revive.Scenarios;
using Revive.Presentation;
using Revive.UI;

namespace Revive.Tests
{
    public sealed class VisualQualityTests
    {
        private string screenshotDir;

        [OneTimeSetUp]
        public void SetUpDirectory()
        {
            screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);
        }

        [UnityTest]
        public IEnumerator PlaySceneAndCaptureScreenshots()
        {
            SceneManager.LoadScene("Assets/Scenarios/Level01/SceneSafety.unity");
            yield return null;
            yield return new WaitForSeconds(0.5f);

            var session = Object.FindFirstObjectByType<ScenarioSession>();
            var presentation = Object.FindFirstObjectByType<ScenarioPresentation>();
            var hud = Object.FindFirstObjectByType<SimulationHUD>();
            var uiDoc = hud.GetComponent<UIDocument>();

            Assert.IsNotNull(session, "ScenarioSession should exist in scene.");
            Assert.IsNotNull(presentation, "ScenarioPresentation should exist.");

            // Wait for arrival animation / incident presentation
            yield return new WaitForSeconds(2.0f);

            // 1. Initial Scene State
            yield return CaptureScreen("game_view_1440x900.png");
            yield return CaptureScreen("game_view_1200x800.png");

            // 2. Play Q1 A (Wrong - Approach casualty directly)
            Assert.AreEqual(0, session.Manager.State.QuestionIndex);
            session.Select(0); // Choice A
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(ConsequenceKind.Unsafe, session.LastDecision.Consequence.kind);
            Assert.AreEqual(WorldResponse.Approach, presentation.Response);
            yield return CaptureScreen("q1_choice_A_wrong_approach.png");

            // Retry Q1
            session.Continue();
            yield return new WaitForSeconds(0.5f);

            // 3. Play Q1 B (Correct - Survey scene for hazards)
            session.Select(1); // Choice B
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(ConsequenceKind.Safe, session.LastDecision.Consequence.kind);
            Assert.AreEqual(WorldResponse.Survey, presentation.Response);
            yield return CaptureScreen("q1_choice_B_correct_survey.png");

            // Continue to Q2
            session.Continue();
            yield return new WaitForSeconds(0.5f);

            // 4. Play Q2 B (Wrong - Reach in to move cable)
            Assert.AreEqual(1, session.Manager.State.QuestionIndex);
            session.Select(1); // Choice B (Reach)
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(ConsequenceKind.Unsafe, session.LastDecision.Consequence.kind);
            Assert.AreEqual(WorldResponse.Reach, presentation.Response);
            yield return CaptureScreen("q2_choice_B_wrong_reach.png");

            // Retry Q2
            session.Continue();
            yield return new WaitForSeconds(0.5f);

            // 5. Play Q2 C (Correct - Call emergency services)
            session.Select(2); // Choice C (Call)
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(ConsequenceKind.Safe, session.LastDecision.Consequence.kind);
            Assert.AreEqual(WorldResponse.Call, presentation.Response);
            yield return CaptureScreen("q2_choice_C_correct_call.png");

            Debug.Log($"REVIVE: All decision paths verified & screenshots captured to {screenshotDir}");
        }

        private IEnumerator CaptureScreen(string fileName)
        {
            yield return new WaitForEndOfFrame();
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            if (tex != null)
            {
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(screenshotDir, fileName), bytes);
                Object.DestroyImmediate(tex);
            }
        }
    }
}
