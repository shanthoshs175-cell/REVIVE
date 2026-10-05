using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Revive.Game;
using Revive.Reasoning;
using Revive.Scenarios;
using UnityEditor;
using Revive.Scenarios;
namespace Revive.Tests
{
    public sealed class ReasoningTests
    {
        private ScenarioData scenario;
        [SetUp] public void Setup() => scenario=AssetDatabase.LoadAssetAtPath<ScenarioData>("Assets/Data/ScenarioData/Level01_SceneSafety.asset");
        [Test] public void FactCreationUsesExplicitTruth() { Assert.That(new Fact("known",false).Value,Is.False);Assert.Throws<ArgumentException>(()=>new Fact("")); }
        [Test] public void FactEqualityIncludesValue() {Assert.That(new Fact("a"),Is.EqualTo(new Fact("a")));Assert.That(new Fact("a"),Is.Not.EqualTo(new Fact("a",false)));}
        [Test] public void MissingFactIsNotFalse() => Assert.That(RuleEngine.Matches(new Dictionary<string,bool>(),new[]{new Fact("unknown",false)}),Is.False);
        [Test] public void RuleRequiresAllConditions() => Assert.That(RuleEngine.Matches(new Dictionary<string,bool>{{"a",true}},new[]{new Fact("a"),new Fact("b")}),Is.False);
        [Test] public void ForwardChainReachesFixedPointEvenInReverseOrder()
        {
            var rules=new[]{new Rule("second",new[]{new Fact("b")},new[]{new Fact("c")}),new Rule("first",new[]{new Fact("a")},new[]{new Fact("b")})};
            var result=new ForwardChainer().Infer(new[]{new Fact("a")},rules);
            Assert.That(result.Facts["c"],Is.True);Assert.That(result.FiredRules.Select(r=>r.Id),Is.EqualTo(new[]{"first","second"}));Assert.That(result.Passes,Is.EqualTo(3));
        }
        [Test] public void CyclesTerminateWithoutDuplicateTrace()
        {
            var result=new ForwardChainer().Infer(new[]{new Fact("a")},new[]{new Rule("ab",new[]{new Fact("a")},new[]{new Fact("b")}),new Rule("ba",new[]{new Fact("b")},new[]{new Fact("a")})});
            Assert.That(result.Facts.Count,Is.EqualTo(2));Assert.That(result.FiredRules.Count,Is.EqualTo(1));
        }
        [Test] public void ContradictoryConclusionsFailClosed() => Assert.Throws<InvalidOperationException>(()=>new ForwardChainer().Infer(new[]{new Fact("a")},new[]{new Rule("conflict",new[]{new Fact("a")},new[]{new Fact("a",false)})}));
        [Test] public void PassLimitFailsExplicitly() => Assert.Throws<InvalidOperationException>(()=>new ForwardChainer().Infer(new[]{new Fact("a")},new[]{new Rule("next",new[]{new Fact("a")},new[]{new Fact("b")})},1));
        [Test] public void ScenarioLoadsWithCompiledInkAndSources() {Assert.NotNull(scenario);Assert.DoesNotThrow(scenario.Validate);Assert.That(scenario.Story.hasErrors,Is.False);Assert.That(scenario.Sources.Count,Is.GreaterThan(0));}
        [Test] public void UnobservedHazardIsUnknown() {var session=new GameStateManager(scenario);Assert.That(session.State.Facts.ContainsKey("hazard_confirmed"),Is.False);}
        private static void Correct(GameStateManager manager, string action)
        { manager.Act(action); manager.ResolveDecision(); }
        private GameStateManager At(int question)
        {
            var manager = new GameStateManager(scenario);
            if(question>0) Correct(manager,"assess");
            if(question>1) Correct(manager,"call");
            return manager;
        }
        [Test] public void QuestionsContainExactlyFourValidOptions()
        {
            Assert.That(scenario.Questions.Count,Is.EqualTo(3));
            for(int i=0;i<3;i++)
            {
                var manager=At(i);var ink=new InkNarrative(scenario);
                Assert.That(ink.Present(manager.State),Is.Not.Empty);
                Assert.That(ink.Choices.Count,Is.EqualTo(4));
                Assert.That(ink.Choices.Select(c=>c.ActionId),Is.EqualTo(scenario.Questions[i].actionIds));
            }
        }
        [Test] public void ObservationRevealsOnlyItsOwnFacts()
        {
            var manager=At(0);manager.Act("assess");
            Assert.That(manager.State.Facts["hazard_confirmed"],Is.True);
            Assert.That(manager.State.Facts["hazard_controlled"],Is.False);
            Assert.That(manager.State.Facts.ContainsKey("help_requested"),Is.False);
        }
        [TestCase(0,"approach",WorldResponse.Approach)]
        [TestCase(0,"touch",WorldResponse.Reach)]
        [TestCase(0,"leave",WorldResponse.Leave)]
        [TestCase(1,"enter",WorldResponse.Approach)]
        [TestCase(1,"send",WorldResponse.DirectBystander)]
        [TestCase(1,"handle",WorldResponse.Reach)]
        [TestCase(2,"enter_after_call",WorldResponse.Approach)]
        [TestCase(2,"send_again",WorldResponse.DirectBystander)]
        [TestCase(2,"touch_again",WorldResponse.Reach)]
        public void EveryWrongOptionPreservesQuestionAndAllowsSameFourChoices(int question,string action,WorldResponse response)
        {
            var manager=At(question);var before=manager.State.Facts.ToDictionary(x=>x.Key,x=>x.Value);
            var decision=manager.Act(action);
            Assert.That(decision.Consequence.id,Is.EqualTo(action+"_result"));
            Assert.That(decision.Consequence.worldResponse,Is.EqualTo(response));
            Assert.That(decision.Consequence.advancesQuestion,Is.False);
            Assert.That(manager.State.QuestionIndex,Is.EqualTo(question));
            Assert.That(manager.State.Complete,Is.False);
            Assert.That(manager.State.Mistakes,Is.EqualTo(1));
            Assert.That(manager.State.PenaltyPoints,Is.EqualTo(10));
            Assert.Throws<InvalidOperationException>(()=>manager.Act(action));
            manager.ResolveDecision();
            Assert.That(manager.State.QuestionIndex,Is.EqualTo(question));
            Assert.That(manager.State.Facts,Is.EquivalentTo(before));
            var ink=new InkNarrative(scenario);ink.Present(manager.State);
            Assert.That(ink.Choices.Select(c=>c.ActionId),Is.EqualTo(scenario.Questions[question].actionIds));
        }
        [TestCase(0,"assess",WorldResponse.Survey)]
        [TestCase(1,"call",WorldResponse.Call)]
        [TestCase(2,"observe",WorldResponse.Monitor)]
        public void CorrectDecisionRequiresContinueAndAdvancesExactlyOnce(int question,string action,WorldResponse response)
        {
            var manager=At(question);var decision=manager.Act(action);
            Assert.That(decision.Consequence.advancesQuestion,Is.True);
            Assert.That(decision.Consequence.worldResponse,Is.EqualTo(response));
            Assert.That(manager.State.QuestionIndex,Is.EqualTo(question));
            Assert.That(manager.State.Complete,Is.False);
            Assert.Throws<InvalidOperationException>(()=>manager.Act(action));
            manager.ResolveDecision();Assert.That(manager.State.QuestionIndex,Is.EqualTo(question+1));
            Assert.That(manager.State.Complete,Is.EqualTo(question==2));
            Assert.Throws<InvalidOperationException>(()=>manager.ResolveDecision());
        }
        [Test] public void FutureActionsCannotSkipRequiredDecisions()
        {
            Assert.Throws<InvalidOperationException>(()=>At(0).Act("call"));
            Assert.Throws<InvalidOperationException>(()=>At(0).Act("observe"));
            Assert.Throws<InvalidOperationException>(()=>At(1).Act("observe"));
        }
        [Test] public void HelpPersistsWithoutControllingHazard()
        {
            var manager=At(2);Assert.That(manager.State.Facts["help_requested"],Is.True);
            Assert.That(manager.State.Facts["hazard_controlled"],Is.False);Assert.That(manager.State.Complete,Is.False);
            Correct(manager,"observe");Assert.That(manager.State.Facts["hazard_controlled"],Is.False);
        }
        [Test] public void SafeSceneInferenceSupportsOtherScenarioData()
        {
            var rule=new Rule("safe",new[]{new Fact("controlled"),new Fact("route_clear")},new[]{new Fact("approach_possible")});
            Assert.That(new ForwardChainer().Infer(new[]{new Fact("controlled"),new Fact("route_clear")},new[]{rule}).Facts["approach_possible"],Is.True);
        }
        [Test] public void PerfectPathScores100OnlyAfterThreeCorrectDecisions()
        {
            var manager=At(0);Correct(manager,"assess");Assert.That(manager.State.Score,Is.EqualTo(30));
            Correct(manager,"call");Assert.That(manager.State.Score,Is.EqualTo(60));
            manager.Act("observe");Assert.That(manager.State.Score,Is.EqualTo(90));Assert.That(manager.State.Complete,Is.False);
            manager.ResolveDecision();Assert.That(manager.State.Score,Is.EqualTo(100));Assert.That(manager.State.Complete,Is.True);
            Assert.Throws<InvalidOperationException>(()=>manager.Act("observe"));
        }
        [Test] public void RepeatingObservationCannotFarmScore()
        {
            var manager=At(0);manager.Act("assess");Assert.Throws<InvalidOperationException>(()=>manager.Act("assess"));
            manager.ResolveDecision();Assert.Throws<InvalidOperationException>(()=>manager.Act("assess"));Assert.That(manager.State.Score,Is.EqualTo(30));
        }
        [TestCase(1,90)] [TestCase(12,0)]
        public void UnsafePenaltyAtZeroCannotDisappearOrPreventCompletion(int attempts,int expected)
        {
            var manager=At(0);
            for(int i=0;i<attempts;i++){Correct(manager,"approach");Assert.That(manager.State.Score,Is.InRange(0,100));}
            Correct(manager,"assess");Correct(manager,"call");Correct(manager,"observe");
            Assert.That(manager.State.Mistakes,Is.EqualTo(attempts));Assert.That(manager.State.PenaltyPoints,Is.EqualTo(attempts*10));
            Assert.That(manager.State.Score,Is.EqualTo(expected));Assert.That(manager.State.Complete,Is.True);
        }
        [TestCase(0,-20,0,0)] [TestCase(95,50,20,100)] [TestCase(30,50,20,100)]
        public void ScoreIsBounded(int current,int delta,int bonus,int expected) => Assert.That(ScoreCalculator.Apply(current,delta,bonus,100),Is.EqualTo(expected));
        [Test] public void EventsPublishInOrder()
        {
            var manager=At(0);var events=new List<string>();manager.OnPlayerAction+=_=>events.Add("action");manager.OnFactsUpdated+=_=>events.Add("facts");manager.OnStateChanged+=_=>events.Add("state");manager.OnConsequenceReady+=_=>events.Add("consequence");manager.Act("assess");Assert.That(events,Is.EqualTo(new[]{"action","facts","state","consequence"}));
        }
        [Test] public void InkBridgeUsesTagsAndHasAllExplanations()
        {
            var manager=At(0);var ink=new InkNarrative(scenario);
            foreach(var outcome in scenario.Consequences)Assert.That(ink.Explain(outcome.explanationKnot),Is.Not.Empty);
            ink.Present(manager.State);var choice=ink.Choices.First(c=>c.ActionId=="assess");Assert.That(ink.Choose(choice.Index),Is.EqualTo("assess"));
        }
        [Test] public void EveryQuestionHasExactlyOneCorrectChoice()
        {
            Assert.That(scenario.Questions.Count,Is.EqualTo(3));
            foreach(var q in scenario.Questions)
            {
                int correctCount=q.actionIds.Count(id=>scenario.Outcome(scenario.Action(id).outcomes[0]).advancesQuestion);
                Assert.That(correctCount,Is.EqualTo(1));
            }
        }
        [Test] public void Level2UnlocksOnlyAfterLevel1Completion()
        {
            var store=new PlayerPrefsProgressStore();
            UnityEngine.PlayerPrefs.DeleteKey("revive.progress.v1.level02.unlocked");
            UnityEngine.PlayerPrefs.DeleteKey("revive.progress.v1.level01_scene_safety.done");
            Assert.That(store.IsUnlocked("level02"),Is.False);
            var manager=At(0);Correct(manager,"assess");Correct(manager,"call");Correct(manager,"observe");
            Assert.That(manager.State.Complete,Is.True);
            store.Record(scenario.Id,manager.State.Score,scenario.UnlocksScenarioId);
            Assert.That(store.IsUnlocked("level02"),Is.True);
        }
        [Test] public void ReplayResetsAttemptState()
        {
            var manager=At(0);Correct(manager,"assess");
            Assert.That(manager.State.QuestionIndex,Is.EqualTo(1));
            var restarted=new GameStateManager(scenario);
            Assert.That(restarted.State.QuestionIndex,Is.Zero);
            Assert.That(restarted.State.Score,Is.Zero);
            Assert.That(restarted.State.Complete,Is.False);
        }
    }
}
