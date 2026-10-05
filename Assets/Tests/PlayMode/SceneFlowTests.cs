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
        private readonly string[] keys={"revive.progress.v1.level01_scene_safety.best","revive.progress.v1.level01_scene_safety.done","revive.progress.v1.level02.unlocked"};
        private bool[] existed; private int[] values;
        [SetUp] public void PreserveProgress()
        {
            existed=keys.Select(PlayerPrefs.HasKey).ToArray();values=keys.Select(k=>PlayerPrefs.GetInt(k)).ToArray();
            foreach(var key in keys)PlayerPrefs.DeleteKey(key);
        }
        [TearDown] public void RestoreProgress()
        {for(int i=0;i<keys.Length;i++){if(existed[i])PlayerPrefs.SetInt(keys[i],values[i]);else PlayerPrefs.DeleteKey(keys[i]);}PlayerPrefs.Save();}
        [UnityTest] public IEnumerator FullSceneFlowIncludesRetryScoreProgressAndReplay()
        {
            yield return SceneManager.LoadSceneAsync("SceneSafety");yield return new WaitForSeconds(2.1f);
            var session=Object.FindFirstObjectByType<ScenarioSession>();
            var presentation=Object.FindFirstObjectByType<ScenarioPresentation>();
            var root=Object.FindFirstObjectByType<UIDocument>().rootVisualElement;
            Assert.That(presentation.Ready,Is.True);
            string[] correct={"assess","call","observe"};
            for(int question=0;question<3;question++)
            {
                var options=session.Narrative.Choices.Select(c=>c.ActionId).ToArray();Assert.That(options.Length,Is.EqualTo(4));
                foreach(var wrong in options.Where(id=>id!=correct[question]))
                {
                    Assert.That(root.Query<Button>(className:"choice").ToList().Count,Is.EqualTo(4));
                    Choose(session,wrong);Assert.That(session.IsPresenting,Is.True);Assert.That(session.AwaitingContinue,Is.False);
                    Assert.That(presentation.Response,Is.EqualTo(session.LastDecision.Consequence.worldResponse));
                    Assert.That(root.Query<Button>(className:"choice").ToList(),Is.Empty);
                    session.Continue();Assert.That(session.Manager.State.QuestionIndex,Is.EqualTo(question));
                    yield return new WaitForSecondsRealtime(3.35f);
                    Assert.That(session.AwaitingContinue,Is.True);Assert.That(session.Explanation,Is.Not.Empty);
                    Assert.That(root.Q<Button>("continue").text,Does.Contain("TRY AGAIN"));
                    Assert.That(session.NextLevelUnlocked,Is.False);
                    session.Continue();yield return null;
                    Assert.That(session.Narrative.Choices.Select(c=>c.ActionId),Is.EqualTo(options));
                    Assert.That(session.Manager.State.QuestionIndex,Is.EqualTo(question));
                }
                Choose(session,correct[question]);Assert.That(presentation.Response,Is.EqualTo(session.LastDecision.Consequence.worldResponse));
                yield return new WaitForSecondsRealtime(3.35f);
                Assert.That(root.Q<Button>("continue").text,Does.Contain("CONTINUE"));
                Assert.That(session.Manager.State.Complete,Is.False);Assert.That(session.NextLevelUnlocked,Is.False);
                session.Continue();yield return null;
                Assert.That(session.Manager.State.QuestionIndex,Is.EqualTo(question+1));
            }
            Assert.That(session.Manager.State.Complete,Is.True);Assert.That(session.Manager.State.Score,Is.EqualTo(10));
            Assert.That(session.Manager.State.Mistakes,Is.EqualTo(9));Assert.That(session.NextLevelUnlocked,Is.True);
            var persisted=new PlayerPrefsProgressStore();Assert.That(persisted.IsComplete(session.Scenario.Id),Is.True);
            Assert.That(persisted.BestScore(session.Scenario.Id),Is.EqualTo(10));Assert.That(persisted.IsUnlocked("level02"),Is.True);
            Assert.NotNull(root.Q<Button>("replay"));
            session.Restart();yield return new WaitForSeconds(2.1f);
            Assert.That(session.Manager.State.Score,Is.Zero);Assert.That(session.Manager.State.QuestionIndex,Is.Zero);
            Assert.That(session.Narrative.Choices.Count,Is.EqualTo(4));Assert.That(session.Manager.State.Complete,Is.False);
            Assert.That(session.BestScore,Is.EqualTo(10));Assert.That(session.NextLevelUnlocked,Is.True);
            LogAssert.NoUnexpectedReceived();
        }
        private static void Choose(ScenarioSession session,string id)=>session.Select(session.Narrative.Choices.First(c=>c.ActionId==id).Index);
    }
}
