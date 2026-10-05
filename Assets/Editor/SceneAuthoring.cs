using System;
using System.IO;
using Ink.UnityIntegration;
using Revive.Game;
using Revive.Scenarios;
using Revive.Presentation;
using Revive.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
namespace Revive.Editor
{
    /// <summary>Rebuilds presentation wiring only. Never authors or overwrites medical scenario rules.</summary>
    public static class SceneAuthoring
    {
        public const string ScenePath = "Assets/Scenarios/Level01/SceneSafety.unity";
        public const string DataPath = "Assets/Data/ScenarioData/Level01_SceneSafety.asset";
        [MenuItem("REVIVE/Build Level 1 presentation")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var data = AssetDatabase.LoadAssetAtPath<ScenarioData>(DataPath);
            if(data==null)throw new InvalidOperationException("Scenario asset missing.");
            const string inkPath = "Assets/Scenarios/Level01/SceneSafety.ink";
            var importer = new SerializedObject(AssetImporter.GetAtPath(inkPath));
            importer.FindProperty("compileAsMasterFileOverride").boolValue = true;
            importer.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.ImportAsset(inkPath, ImportAssetOptions.ForceUpdate);
            // Importer's main subasset ID is owned by Ink; resolve it rather than guessing.
            var ink=AssetDatabase.LoadAssetAtPath<InkFile>(inkPath);
            Assign(data,"story",ink); data.Validate();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraObject=new GameObject("Presentation Camera");var camera=cameraObject.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.047f,.106f,.141f);camera.orthographic=true;cameraObject.AddComponent<AudioListener>();
            var app=new GameObject("REVIVE - Scenario Session");
            var session=app.AddComponent<ScenarioSession>();Assign(session,"scenario",data);
            var animator=app.AddComponent<Animator>();
            var presentation=app.AddComponent<ScenarioPresentation>();Assign(presentation,"session",session);
            var audio=app.AddComponent<ScenarioAudio>();Assign(audio,"session",session);
            animator.runtimeAnimatorController=MakeAnimator();
            var ui=new GameObject("Simulation HUD");var doc=ui.AddComponent<UIDocument>();
            const string panelPath="Assets/UI/GameplayUI/SimulationPanel.asset";
            var panel=AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
            if(panel==null){panel=ScriptableObject.CreateInstance<PanelSettings>();AssetDatabase.CreateAsset(panel,panelPath);}
            panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(1440,900);panel.match=.5f;
            EditorUtility.SetDirty(panel);
            doc.panelSettings=panel;
            var hud=ui.AddComponent<SimulationHUD>();Assign(hud,"session",session);Assign(hud,"presentation",presentation);Assign(hud,"audioFeedback",audio);
            Assign(hud,"stylesheet",AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/GameplayUI/Simulation.uss"));
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();Debug.Log("REVIVE: scene, InkFile, ScenarioData and animation wiring verified.");
        }
        private static void Assign(UnityEngine.Object target,string field,UnityEngine.Object value)
        {var so=new SerializedObject(target);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static RuntimeAnimatorController MakeAnimator()
        {
            const string path="Assets/Animation/ScenarioPresentation.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(controller!=null)return controller;
            controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Warn",AnimatorControllerParameterType.Trigger);controller.AddParameter("Confirm",AnimatorControllerParameterType.Trigger);
            var machine=controller.layers[0].stateMachine;
            var arrival=machine.AddState("Arrival");arrival.motion=Clip("Arrival","incidentProgress",0,1,1.8f);
            var ready=machine.AddState("Ready");ready.motion=Clip("Ready","incidentProgress",1,1,1);
            var warn=machine.AddState("Warn");warn.motion=Clip("Pause","reactionStrength",1,0,.9f);
            var confirm=machine.AddState("Confirm");confirm.motion=Clip("Confirm","reactionStrength",.5f,0,.6f);
            machine.defaultState=arrival;
            var first=arrival.AddTransition(ready);first.hasExitTime=true;first.exitTime=1;first.duration=0;
            foreach(var pair in new[]{(warn,"Warn"),(confirm,"Confirm")})
            {
                var t=machine.AddAnyStateTransition(pair.Item1);t.AddCondition(AnimatorConditionMode.If,0,pair.Item2);t.duration=.08f;t.canTransitionToSelf=false;
                var back=pair.Item1.AddTransition(ready);back.hasExitTime=true;back.exitTime=1;back.duration=.1f;
            }
            return controller;
        }
        private static AnimationClip Clip(string name,string field,float start,float end,float seconds)
        {
            var clip=new AnimationClip{name=name};clip.SetCurve("",typeof(ScenarioPresentation),field,AnimationCurve.EaseInOut(0,start,seconds,end));
            if(field!="incidentProgress")clip.SetCurve("",typeof(ScenarioPresentation),"incidentProgress",AnimationCurve.Constant(0,seconds,1));
            AssetDatabase.CreateAsset(clip,"Assets/Animation/"+name+".anim");return clip;
        }
    }
}
