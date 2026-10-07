using System;
using System.Linq;
using Revive.Game;
using Revive.Scenarios;
using Revive.Presentation;
using UnityEngine;
using UnityEngine.UIElements;
namespace Revive.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SimulationHUD : MonoBehaviour
    {
        [SerializeField] private ScenarioSession session;
        [SerializeField] private ScenarioPresentation presentation;
        [SerializeField] private ScenarioAudio audioFeedback;
        [SerializeField] private StyleSheet stylesheet;
        private VisualElement root, choices, feedback, rail, sceneFrame;
        private Label score, prompt, feedbackTitle, feedbackText, phase, observationCount, completeText;
        private Button continueButton, motionButton, soundButton;
        private EnvironmentCanvas canvas;
        private bool readyLast;
        private void OnEnable() { ResolveDependencies(); if (session != null) session.Refreshed += Refresh; }
        private void OnDisable() { if (session != null) session.Refreshed -= Refresh; }
        private void ResolveDependencies()
        {
            if (session == null) session = FindFirstObjectByType<ScenarioSession>();
            if (presentation == null) presentation = FindFirstObjectByType<ScenarioPresentation>();
            if (audioFeedback == null) audioFeedback = FindFirstObjectByType<ScenarioAudio>();
            if (stylesheet == null)
            {
#if UNITY_EDITOR
                stylesheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/GameplayUI/Simulation.uss");
#endif
            }
        }
        private void Start() { ResolveDependencies(); Build(); Refresh(); }
        private static Label Text(string text,string cls) { var label=new Label(text);label.AddToClassList(cls);return label; }
        private static VisualElement Box(VisualElement parent,string cls)
        { var e=new VisualElement();e.AddToClassList(cls);parent.Add(e);return e; }
        private static Button Button(string text,Action clicked,string cls)
        {var b=new Button(clicked){text=text};b.AddToClassList(cls);return b;}
        private void Build()
        {
            ResolveDependencies();
            root=GetComponent<UIDocument>().rootVisualElement;root.Clear();
            if (stylesheet != null) root.styleSheets.Add(stylesheet);
            root.AddToClassList("app");
            root.RegisterCallback<GeometryChangedEvent>(_=>root.EnableInClassList("compact",root.resolvedStyle.width<1000));
            var header=Box(root,"header");
            var brand=Box(header,"brand");brand.Add(Text("REVIVE", "wordmark"));brand.Add(Text("EVERY CHOICE MATTERS", "micro"));
            header.Add(Text("FIELD TRAINING   /   SIMULATION", "header-course"));
            var controls=Box(header,"controls");
            motionButton=Button("Motion: on",()=>{presentation.ReducedMotion=!presentation.ReducedMotion;motionButton.text=presentation.ReducedMotion?"Motion: reduced":"Motion: on";},"utility");motionButton.name="motion-toggle";controls.Add(motionButton);
            soundButton=Button("Sound: on",()=>{audioFeedback.ToggleMute();soundButton.text=audioFeedback.Muted?"Sound: off":"Sound: on";},"utility");soundButton.name="sound-toggle";controls.Add(soundButton);
            var heading=Box(root,"heading");var titles=Box(heading,"titles");titles.Add(Text("GUIDED PRACTICE   /   FUNDAMENTALS", "eyebrow"));titles.Add(Text(session.Scenario.Title,"title"));
            var scoreBox=Box(heading,"score-box");scoreBox.Add(Text("TRAINING SCORE", "micro"));score=Text("000 / 100","score");scoreBox.Add(score);
            var main=Box(root,"main");
            var left=Box(main,"left-column");sceneFrame=Box(left,"scene-frame");
            canvas=new EnvironmentCanvas();canvas.AddToClassList("environment");sceneFrame.Add(canvas);
            var sceneTop=Box(sceneFrame,"scene-top");sceneTop.Add(Text("SIMULATION / LIVE", "scene-badge"));phase=Text("INCIDENT", "scene-badge");sceneTop.Add(phase);

            var caption=Box(left,"scene-caption");caption.Add(Text(session.Scenario.Environment.ToUpperInvariant(),"micro"));caption.Add(Text(session.Scenario.ViewpointCaption, "caption"));
            var objective=Box(left,"objective");objective.Add(Text("YOUR OBJECTIVE", "eyebrow"));objective.Add(Text(session.Scenario.LearningObjective,"objective-copy"));
            var progress=Box(left,"progress-row");observationCount=Text("0 / 3 observations", "caption");progress.Add(observationCount);progress.Add(Text("OBSERVE  →  DECIDE  →  REFLECT", "micro"));
            rail=Box(main,"rail");
            var scroll=new ScrollView(ScrollViewMode.Vertical);scroll.AddToClassList("rail-scroll");rail.Add(scroll);
            scroll.Add(Text("YOUR NEXT MOVE", "eyebrow"));prompt=Text("", "prompt");scroll.Add(prompt);
            choices=Box(scroll,"choices");
            feedback=Box(scroll,"feedback");feedbackTitle=Text("", "feedback-title");feedbackText=Text("", "feedback-copy");feedback.Add(feedbackTitle);feedback.Add(feedbackText);
            continueButton=Button("Continue assessment  →",()=>session.Continue(),"primary");continueButton.name="continue";feedback.Add(continueButton);
            completeText=Text("", "complete-copy");scroll.Add(completeText);
            var foot=Box(root,"footer");foot.Add(Text(session.Scenario.Sources.FirstOrDefault()?.shortTitle ?? "", "micro"));foot.Add(Text("Training simulation • No real emergency call is made", "caption"));
        }
        private void Update()
        {
            if(canvas==null)return;
            canvas.Incident=presentation.IncidentProgress;canvas.Reaction=presentation.ReactionStrength;canvas.ReducedMotion=presentation.ReducedMotion;
            canvas.Warning=session.LastDecision?.Consequence.kind==ConsequenceKind.Unsafe;
            canvas.Response=presentation.Response;canvas.ResponseProgress=presentation.ResponseProgress;
            canvas.AssessmentVisible=presentation.AssessmentVisible;canvas.CommunicationVisible=presentation.CommunicationVisible;
            canvas.MarkDirtyRepaint();
            if(readyLast!=presentation.Ready){readyLast=presentation.Ready;Refresh();}
        }
        public void Refresh()
        {
            if(root==null || session.Manager==null)return;
            var state=session.Manager.State;
            score.text=$"{state.Score:000} / {session.Scenario.MaximumScore}";
            observationCount.text=state.Complete ? "3 / 3 decisions completed" : $"QUESTION {state.QuestionIndex+1} / {session.Scenario.Questions.Count}";
            phase.text=!presentation.Ready?"INCIDENT":session.IsPresenting?"ACTION IN PROGRESS":state.Complete?"RESPONSE RECORDED":session.AwaitingContinue?"REFLECT":"OBSERVE & DECIDE";
            prompt.text=!presentation.Ready?session.Scenario.Incident:state.Complete?session.Scenario.CompletionTitle:session.IsPresenting?session.LastDecision.Consequence.presentationCaption:session.Prompt;
            choices.Clear();
            feedback.style.display=session.AwaitingContinue?DisplayStyle.Flex:DisplayStyle.None;
            completeText.style.display=state.Complete?DisplayStyle.Flex:DisplayStyle.None;
            completeText.text=state.Complete?$"{session.Scenario.CompletionMessage}\n\nFINAL SCORE  {state.Score}/{session.Scenario.MaximumScore}\nBest score {session.BestScore}/{session.Scenario.MaximumScore}\n\n{session.Scenario.CompletionSummary}\n\nUnsafe decisions: {state.Mistakes} · Penalties: {state.PenaltyPoints} points":"";
            if(session.AwaitingContinue)
            {
                var d=session.LastDecision;
                feedback.EnableInClassList("unsafe",!d.Consequence.advancesQuestion);
                feedbackTitle.text=d.Consequence.title;feedbackText.text=session.Explanation;
                continueButton.style.display=DisplayStyle.Flex;
                continueButton.text=d.Consequence.advancesQuestion?"CONTINUE  →":"TRY AGAIN  ↻";
                continueButton.EnableInClassList("retry",!d.Consequence.advancesQuestion);
                foreach(var old in feedback.Children().OfType<Foldout>().ToArray())old.RemoveFromHierarchy();
                var reasoning=new Foldout{text="Why this result?",value=false};reasoning.AddToClassList("reasoning");
                reasoning.Add(Text($"Decision score: {d.Consequence.scoreDelta:+0;-0;0}","caption"));
                foreach(var rule in d.Inference.FiredRules)reasoning.Add(Text(rule.Rationale,"caption"));
                reasoning.Add(Text(session.Scenario.Sources.First(source=>source.id==d.Consequence.sourceId).title,"source"));feedback.Add(reasoning);
                continueButton.schedule.Execute(()=>continueButton.Focus());
                return;
            }
            if(state.Complete)
            {
                var replay=Button("REPLAY SCENE  ↻",()=>session.Restart(),"primary");replay.name="replay";choices.Add(replay);
                replay.schedule.Execute(()=>replay.Focus());
                return;
            }
            if(session.IsPresenting || !presentation.Ready)return;
            int number=0;
            foreach(var choice in session.Narrative.Choices)
            {
                var current=choice;
                var b=Button($"{(char)('A'+number)}   {choice.Text}",()=>session.Select(current.Index),"choice");
                number++;
                b.name="choice-"+choice.ActionId;
                choices.Add(b);
                if(number==1)b.schedule.Execute(()=>b.Focus());
            }
        }
    }
}
