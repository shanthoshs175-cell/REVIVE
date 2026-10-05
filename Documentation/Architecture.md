# Phase 1 architecture

The user's Phase 1 brief takes precedence over the master document's older staging plan: generic ScenarioData and engine separation are implemented now; search and later systems remain deferred.

## Data flow

Ink tagged choice → ScenarioSession → GameStateManager → PerceptionModule / DecisionEngine → ForwardChainer → Consequence → events → animation, audio, UI → local progress.

- `Fact`: explicit ID + boolean. Absence means unknown, not false.
- `Rule`: ALL conditions, conclusions, source ID and rationale. Positive/negative propositions must be explicit.
- `ForwardChainer`: add-only closure, independent of scenes/UI. Evaluates rules until a pass adds nothing. Cycles terminate naturally, contradictory truth assignments and pass-limit exhaustion throw before session mutation. Fired rules form an ordered proof trace.
- `ScenarioData`: serialized authoring source, including initial facts, observations, action requirements/intents, ordered outcomes, consequences, score values, sources and an imported InkFile.
- `DecisionEngine`: evaluates an action against known facts plus its observation and temporary intent; selects the first matching configured outcome. It contains no Level 1 IDs or medical branches.
- `GameStateManager`: commits observations and consequence effects atomically after validation. Temporary intent and its derived facts are not persisted. Inference is recomputed from base facts. Publishes action, facts, state and consequence events.
- `InkNarrative`: validates action tags against scenario IDs; content controls choices and explanation knots. The inference engine selects the explanation knot through consequence data; Ink does not evaluate medical rules.
- `ScenarioSession`: small Unity composition root, narrative/attempt lifecycle and continue gate.
- `SimulationHUD`, `ScenarioPresentation`, `ScenarioAudio`: presentation subscribers; no rule evaluation. HUD renders arbitrary choice counts in a scrollable rail. Artwork is an original vector scene. Animator controls incident and reaction envelopes.
- `ScoreCalculator`: clamped arithmetic; data owns deltas/bonus. Duplicate observations are rejected; completion is one-shot.
- `PlayerPrefsProgressStore`: local completion and best score. A new attempt clears its own state without erasing prior best score.

## Authoring another scenario later

Create a ScenarioData asset and an Ink file, supply matching IDs, sources and explanation knots, then assign the asset to ScenarioSession. No new AI/manager/rule-engine class is needed. Current artwork depicts a maintenance bay and must be replaced or extended for a different location; engine reuse does not imply automatic art generation.

Ink choices carry exactly one `# action:<id>` tag. Outcome order matters: specific matches precede an unconditional fallback. Keep IDs stable. Validate the asset and cover new branches with tests.

This Phase 1 engine is monotonic: it does not retract facts or model a hazard changing from uncontrolled to controlled. Future state-transition work should explicitly define retraction rather than append contradictory booleans. Level 1 ends with a safe *response*, never a claim that the electrical hazard became safe.

## Presentation and input

UI Toolkit handles pointer/keyboard interaction through the existing input configuration. No Input System migration, URP migration or Timeline dependency was needed. Animator is used for the incident, ready state and warning/confirmation transitions. Reduced motion skips movement visually; audio can be muted. Timeline remains available as a future option when choreography requires it.

## Score design

Unique observation +10 (three opportunities); appropriate response +50; completion +20. Unassessed entry −15; entry after a confirmed hazard −20; passive waiting −5. Score clamps to 0–100. Early help is accepted and completes at 70 without forcing observation first. These weights are game design, not IFRC clinical metrics. All deltas and bonus are in ScenarioData.

## Limits

No claim of professional medical review or training certification. No injury diagnosis or treatment is simulated. No real emergency call. The finite scene has fixed conditions; nondeterminism, backward chaining, search, CSP, adaptive difficulty and future levels are not implemented.
