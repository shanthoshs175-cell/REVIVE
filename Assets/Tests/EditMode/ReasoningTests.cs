using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Revive.Game;
using Revive.Reasoning;
using Revive.Scenarios;
using UnityEditor;
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
        [Test] public void ObservationRevealsOnlyItsOwnFacts()
        {
            var session=new GameStateManager(scenario);session.Act("observe_area");
            Assert.That(session.State.Facts["safe_area_identified"],Is.True);Assert.That(session.State.Facts.ContainsKey("hazard_confirmed"),Is.False);
        }
        [Test] public void UnsafeSceneDerivesExplanationAndPenalty()
        {
            var session=new GameStateManager(scenario);session.Act("observe_equipment");session.Act("observe_area");session.Act("observe_people");
            var result=session.Act("approach");Assert.That(result.Consequence.kind,Is.EqualTo(ConsequenceKind.Unsafe));Assert.That(result.Inference.Facts["unsafe_entry"],Is.True);
            Assert.That(session.State.Score,Is.EqualTo(10));Assert.That(session.State.Complete,Is.False);Assert.That(session.State.Facts.ContainsKey("approach_requested"),Is.False);
        }
        [Test] public void UnassessedApproachIsNotTreatedAsSafe() => Assert.That(new GameStateManager(scenario).Act("approach").Consequence.id,Is.EqualTo("unassessed_entry"));
        [Test] public void SafeResponseDoesNotClaimHazardIsControlled()
        {
            var session=new GameStateManager(scenario);session.Act("observe_equipment");var result=session.Act("request_help");
            Assert.That(result.Consequence.kind,Is.EqualTo(ConsequenceKind.Safe));Assert.That(session.State.Complete,Is.True);Assert.That(session.State.Facts["scene_unsafe"],Is.True);Assert.That(session.State.Facts["hazard_controlled"],Is.False);
        }
        [Test] public void SafeSceneInferenceSupportsOtherScenarioData()
        {
            var safeRule=new Rule("safe",new[]{new Fact("hazard_controlled"),new Fact("route_clear")},new[]{new Fact("approach_possible")});
            var result=new ForwardChainer().Infer(new[]{new Fact("hazard_controlled"),new Fact("route_clear")},new[]{safeRule});Assert.That(result.Facts["approach_possible"],Is.True);
        }
        [Test] public void EarlyHelpIsAllowedWithoutObservationGate() {var s=new GameStateManager(scenario);s.Act("request_help");Assert.That(s.State.Complete,Is.True);Assert.That(s.State.Score,Is.EqualTo(70));}
        [Test] public void PerfectPathScores100() {var s=new GameStateManager(scenario);foreach(var id in new[]{"observe_equipment","observe_area","observe_people","request_help"})s.Act(id);Assert.That(s.State.Score,Is.EqualTo(100));}
        [Test] public void RepeatingObservationCannotFarmScore() {var s=new GameStateManager(scenario);s.Act("observe_area");Assert.Throws<InvalidOperationException>(()=>s.Act("observe_area"));Assert.That(s.State.Score,Is.EqualTo(10));}
        [Test] public void CompletedSessionRejectsFurtherActions() {var s=new GameStateManager(scenario);s.Act("request_help");Assert.Throws<InvalidOperationException>(()=>s.Act("request_help"));}
        [TestCase(0,-20,0,0)] [TestCase(95,50,20,100)] [TestCase(30,50,20,100)]
        public void ScoreIsBounded(int current,int delta,int bonus,int expected) => Assert.That(ScoreCalculator.Apply(current,delta,bonus,100),Is.EqualTo(expected));
        [Test] public void UnsafeDecisionAtZeroRetainsPenaltyAndMistake()
        {
            var s = new GameStateManager(scenario);
            s.Act("approach");
            Assert.That(s.State.Score, Is.Zero);
            Assert.That(s.State.Mistakes, Is.EqualTo(1));
            Assert.That(s.State.PenaltyPoints, Is.EqualTo(15));
            foreach (var id in new[]{"observe_equipment", "observe_area", "observe_people", "request_help"}) s.Act(id);
            Assert.That(s.State.Score, Is.EqualTo(85));
            Assert.That(s.State.Complete, Is.True);
        }
        [Test] public void ConfirmedUnsafeDecisionRetainsWholePenalty()
        {
            var s = new GameStateManager(scenario);
            s.Act("observe_equipment"); s.Act("approach");
            Assert.That(s.State.Score, Is.Zero);
            foreach (var id in new[]{"observe_area", "observe_people", "request_help"}) s.Act(id);
            Assert.That(s.State.Score, Is.EqualTo(80));
            Assert.That(s.State.Mistakes, Is.EqualTo(1));
            Assert.That(s.State.PenaltyPoints, Is.EqualTo(20));
        }
        [Test] public void RepeatedUnsafeActionsRemainBoundedAndNeverScorePerfectly()
        {
            var s = new GameStateManager(scenario);
            for (int i=0;i<12;i++) { s.Act("approach"); Assert.That(s.State.Score, Is.InRange(0,100)); }
            foreach (var id in new[]{"observe_equipment", "observe_area", "observe_people", "request_help"}) s.Act(id);
            Assert.That(s.State.Score, Is.Zero);
            Assert.That(s.State.Mistakes, Is.EqualTo(12));
            Assert.That(s.State.PenaltyPoints, Is.EqualTo(180));
        }
        [Test] public void WaitingPenaltyAtZeroAlsoRemainsAccountedFor()
        {
            var s = new GameStateManager(scenario); s.Act("wait");
            foreach (var id in new[]{"observe_equipment", "observe_area", "observe_people", "request_help"}) s.Act(id);
            Assert.That(s.State.Score, Is.EqualTo(95));
        }
        [Test] public void EventsPublishInOrder()
        {
            var s=new GameStateManager(scenario);var events=new List<string>();s.OnPlayerAction+=_=>events.Add("action");s.OnFactsUpdated+=_=>events.Add("facts");s.OnStateChanged+=_=>events.Add("state");s.OnConsequenceReady+=_=>events.Add("consequence");s.Act("observe_area");Assert.That(events,Is.EqualTo(new[]{"action","facts","state","consequence"}));
        }
        [Test] public void InkBridgeUsesTagsAndHasAllExplanations()
        {
            var s=new GameStateManager(scenario);var ink=new InkNarrative(scenario);Assert.That(ink.Present(s.State),Is.Not.Empty);
            Assert.That(ink.Choices.Count,Is.EqualTo(6));
            foreach(var outcome in scenario.Consequences)Assert.That(ink.Explain(outcome.explanationKnot),Is.Not.Empty);
            ink.Present(s.State);var choice=ink.Choices.First(c=>c.ActionId=="observe_area");Assert.That(ink.Choose(choice.Index),Is.EqualTo("observe_area"));s.Act(choice.ActionId);ink.Present(s.State);Assert.That(ink.Choices.Any(c=>c.ActionId==choice.ActionId),Is.False);
        }
        [Test] public void EveryOfferedChoiceEvaluatesAfterEveryObservationSubset()
        {
            for(int bits=0;bits<8;bits++)
            {
                var s=new GameStateManager(scenario);var obs=new[]{"observe_equipment","observe_area","observe_people"};
                for(int j=0;j<3;j++)if((bits&(1<<j))!=0)s.Act(obs[j]);
                var ink=new InkNarrative(scenario);ink.Present(s.State);
                foreach(var choice in ink.Choices)
                {
                    var clone=new GameStateManager(scenario);for(int j=0;j<3;j++)if((bits&(1<<j))!=0)clone.Act(obs[j]);
                    Assert.DoesNotThrow(()=>clone.Act(choice.ActionId),choice.ActionId+" / "+bits);
                }
            }
        }
    }
}
