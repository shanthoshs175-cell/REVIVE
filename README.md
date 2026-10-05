# REVIVE — Every Choice Matters

An interactive first-aid training simulation using explainable, classical symbolic reasoning.

## Play the Phase 1 slice

1. Open this project in **Unity 6000.3.25f1** (Apple Silicon supported).
2. Allow Package Manager and Ink imports to finish.
3. Open `Assets/Scenarios/Level01/SceneSafety.unity` and enter Play Mode.
4. Watch the incident; inspect markers or select observations from the right panel. Choose a response, read feedback, and continue or retry. Completion offers replay.

Use a 1440 × 900 Game view for the showcase composition. Buttons support keyboard focus and activation. Sound and reduced-motion controls are in the header. A real emergency call is never placed.

The project uses the existing **Built-in Render Pipeline**, not URP. The vector environment and UI Toolkit HUD do not require a renderer migration. No paid, downloaded art or external visual framework is used.

## Scope

Only Level 1 is playable. Levels 2–5 remain empty foundation folders. There is no ML, network inference, Python backend, search algorithm, adaptive difficulty, analytics or multiplayer gameplay.

## Engineering and content

- [Architecture and authoring](Documentation/Architecture.md)
- [Medical sources and Level 1 rule audit](Documentation/MedicalSources.md)
- [Verification report](Documentation/Phase1Verification.md)

Ink **2.0.0** is imported as an `InkFile` asset. Its compiled `storyJson` is passed to `Ink.Runtime.Story` at runtime; there is no generated story JSON file in Assets. Package lock pins the approved Git revision.

## Tests

Open **Window → General → Test Runner**. Run EditMode and PlayMode suites. PlayMode tests restore existing player progress after verification.

CLI (quit the editor first):

```sh
UNITY=/Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app/Contents/MacOS/Unity
"$UNITY" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults /tmp/revive-editmode.xml -logFile /tmp/revive-editmode.log
"$UNITY" -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults /tmp/revive-playmode.xml -logFile /tmp/revive-playmode.log
```

`REVIVE → Build Level 1 presentation` can regenerate presentation wiring from the existing ScenarioData asset. It replaces the Level 1 scene; do not use it after manually editing that scene unless you intend to rebuild it. It does not write medical rules.

Progress is local PlayerPrefs (`revive.progress.v1.<scenario-id>.best` and `.done`). Scores are educational feedback, not a measure of clinical competence.
