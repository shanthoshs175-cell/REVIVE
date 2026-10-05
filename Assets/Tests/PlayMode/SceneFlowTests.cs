using System.Collections;
using System.Linq;
using NUnit.Framework;
using Revive.Game;
using Revive.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Revive.Tests
{
    public sealed class SceneFlowTests
    {
        private const string Prefix="revive.progress.v1.level01_scene_safety";
        private bool hadBest,hadDone;
        private int best,done;
        [SetUp] public void PreserveProgress()
        {hadBest=PlayerPrefs.HasKey(Prefix+".best");hadDone=PlayerPrefs.HasKey(Prefix+".done");best=PlayerPrefs.GetInt(Prefix+".best");done=PlayerPrefs.GetInt(Prefix+".done");}
        [TearDown] public void RestoreProgress()
        {if(hadBest)PlayerPrefs.SetInt(Prefix+".best",best);else PlayerPrefs.DeleteKey(Prefix+".best");if(hadDone)PlayerPrefs.SetInt(Prefix+".done",done);else PlayerPrefs.DeleteKey(Prefix+".done");PlayerPrefs.Save();}
        [UnityTest] public IEnumerator FullSceneFlowIncludesRetryScoreProgressAndReplay()
        {
            yield return SceneManager.LoadSceneAsync("SceneSafety");yield return null;
            var session=Object.FindFirstObjectByType<ScenarioSession>();Assert.NotNull(session);
            yield return new WaitForSeconds(2.1f);
            Assert.That(Object.FindFirstObjectByType<ScenarioPresentation>().Ready,Is.True);
            var root=Object.FindFirstObjectByType<UIDocument>().rootVisualElement;
            Assert.NotNull(root.Q<Button>("choice-observe_equipment"));
            ScreenCapture.CaptureScreenshot("/tmp/revive-level01-start.png");yield return new WaitForSeconds(.5f);
            Choose(session,"observe_equipment");Assert.That(session.Explanation,Is.Not.Empty);session.Continue();
            Choose(session,"approach");Assert.That(session.Manager.State.Mistakes,Is.EqualTo(1));Assert.That(session.AwaitingContinue,Is.True);
            ScreenCapture.CaptureScreenshot("/tmp/revive-level01-feedback.png");yield return new WaitForSeconds(.5f);
            session.Continue();Choose(session,"observe_area");session.Continue();Choose(session,"observe_people");session.Continue();Choose(session,"request_help");
            Assert.That(session.Manager.State.Complete,Is.True);Assert.That(session.Manager.State.Score,Is.EqualTo(80));
            Assert.That(PlayerPrefs.GetInt(Prefix+".done"),Is.EqualTo(1));Assert.That(session.BestScore,Is.GreaterThanOrEqualTo(80));
            ScreenCapture.CaptureScreenshot("/tmp/revive-level01-complete.png");yield return new WaitForSeconds(.5f);
            session.Restart();Assert.That(session.Manager.State.Score,Is.EqualTo(0));Assert.That(session.Manager.State.Observed.Count,Is.Zero);Assert.That(session.Manager.State.Complete,Is.False);
            LogAssert.NoUnexpectedReceived();
        }
        private static void Choose(ScenarioSession session,string id) => session.Select(session.Narrative.Choices.First(c=>c.ActionId==id).Index);
    }
}
