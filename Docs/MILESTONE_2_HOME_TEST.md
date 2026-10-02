# Milestone 2 home verification

Status: **Unity unavailable during implementation. Every check below is untested.**
Use Unity **6000.6.3f1** (ProjectVersion.txt). Open
`Assets/Scenes/SampleScene.unity`, allow script import, select `Player`, and
inspect `ThirdPersonPlayer`. Enter Play Mode and click the Game view to capture
input. Keep the Player Inspector visible/locked while playing. The new
**Runtime Movement (Read Only)** panel requires no component assignment.
Stop Play Mode to recover after falling off the test ground; there is no respawn.
Make temporary tuning changes in Play Mode and record useful values before stopping.

## Exact checklist

- [ ] Not tested — Import: no C# compilation errors; the Player Inspector shows the normal settings and read-only runtime panel in Play Mode.
- [ ] Not tested — Idle: after settling and the brief Land state, state is Idle, horizontal/normalized speed is approximately 0, Grounded is true, Moving/Running/Jumping/Falling are false; no unexpected drift.
- [ ] Not tested — Walk: WASD moves in all four directions; on unobstructed flat ground, state is Walk and speed settles near 4 units/sec.
- [ ] Not tested — Diagonal: W+D and W+A reach the same steady speed as straight walking; repeat while running.
- [ ] Not tested — Camera-relative movement: orbit 90 and 180 degrees, then press W; movement follows the new camera heading. Looking up/down does not change travel speed.
- [ ] Not tested — Run: hold Left Shift with WASD; speed approaches 7 units/sec and state becomes Run. Release Shift and confirm return to Walk after deceleration; stationary Shift causes no movement.
- [ ] Not tested — Start/stop feel: starting is responsive, releasing input stops cleanly after a short deceleration, and rapid reversals remain controllable.
- [ ] Not tested — Jump: tap Space from idle, walking, and running; one predictable ascent, Grounded false, positive Vertical Velocity, Jump state and Jumping true.
- [ ] Not tested — Fall: after the apex, Vertical Velocity is zero/negative, state is Fall and Falling true. Walking off the ground also enters Fall without requiring Space.
- [ ] Not tested — Land: contact sets Grounded true, Vertical Velocity to about -2, and briefly sets Land before Idle/Walk/Run; no floating, ground jitter, or landing input lock. Just Landed is a single-update pulse and may be too brief to see in the Inspector.
- [ ] Not tested — Direction changes: change W to A/S/D and orbit while moving; Hanuman turns smoothly toward movement without snapping, jitter, or speed changes caused by camera pitch.
- [ ] Not tested — Walk/run configuration: temporarily tune Walk Speed and Run Speed separately; steady speed responds. Set both to 0: Normalized Movement Speed remains 0, with no invalid values; restore 4/7. Keep Run Speed above Walk Speed plus Run Speed Threshold for a reachable Run state.
- [ ] Not tested — Jump/gravity configuration: temporarily tune Jump Height and negative Gravity; height and airtime respond predictably. Restore 1.5/-20. A nonnegative Gravity entry is clamped negative; prefer realistic negative values when tuning.
- [ ] Not tested — Rotation/response configuration: tune Turn Speed, Acceleration, Deceleration, and Air Acceleration independently; confirm their effect, then restore 540/45/26/20. Check Ground Stick Speed 2 and Land State Duration 0.1 retain clean contact and responsive landing.
- [ ] Not tested — State thresholds: Moving Speed Threshold 0.05 and Run Speed Threshold 0.1 preserve the expected Idle/Walk/Run classifications; temporary edits affect classification without changing displacement or turn speed.
- [ ] Not tested — Animation values: compare idle/walk/run/jump/fall against the table below, including normalized walk speed about 0.57, run speed about 1, and Running false in the air. Values update without an Animator or clips.
- [ ] Not tested — Camera regression: mouse orbit, pitch limits, follow, Escape to release, and click to recapture all behave as before.
- [ ] Not tested — Focus regression: release controls with Escape and switch away/back; no new movement input while unlocked/unfocused and no stuck keys on return. Existing grounded deceleration and airborne momentum may continue briefly/through airtime.
- [ ] Not tested — Jump regression: holding Space does not repeatedly jump; pressing Space while airborne does not double-jump or queue a jump at landing. A fresh grounded press jumps again.
- [ ] Not tested — Air/obstacle regression: release WASD and Shift during a running jump; takeoff momentum remains until landing. Check no floating and, if a safe existing obstacle is available, no continued upward velocity after hitting its underside; otherwise leave the ceiling check untested.
- [ ] Not tested — Existing dash regression: tap Left Ctrl while moving, stationary, and airborne; burst, cooldown, gravity, and return to ordinary motion remain intact. Holding Ctrl does not repeat; airborne Jumping/Falling flags still describe the vertical phase during Dash.
- [ ] Not tested — Pause diagnostic: pause with the Editor controls to inspect a frame. If using a time-scale pause, Just Landed must clear on the next player Update with zero delta time; other measured values remain the last movement sample. Editor Pause freezes updates altogether.
- [ ] Not tested — Console: no new errors or exceptions during play, focus changes, and stopping/restarting Play Mode; review Hanuman's appearance separately in Unity before accepting visuals.

## Runtime contract and future Animator mapping

`PlayerAnimationDriver` now samples these properties in **LateUpdate**, after the
player Update. No Animator Controller, clips, or Animator dependency is added.
See [PLAYER_ANIMATION_BRIDGE.md](PLAYER_ANIMATION_BRIDGE.md) for driver setup,
the full proposed parameter mapping, and transition expectations.
The first meaningful contact/speed sample is after the first movement Update;
disabled/inactive players retain their last sample.

| Future Animator parameter | Player property | Meaning |
| --- | --- | --- |
| Speed (float, recommended normalized) | NormalizedMovementSpeed | Actual horizontal speed / max(Walk Speed, Run Speed), clamped 0..1; 0 if both speeds are 0 |
| Grounded (bool) | IsGrounded | Contact from the latest CharacterController.Move, excluding ascent |
| VerticalVelocity (float) | VerticalVelocity | Gravity accumulator in units/sec, including negative ground stick; VerticalSpeed is a compatible alias |
| IsRunning (bool) | IsRunning | State is Run, based on measured speed rather than Shift input |

`HorizontalSpeed` and `Velocity` describe actual controller motion after
collisions, rather than intended input. `IsMoving` means measured horizontal
speed >= Moving Speed Threshold, including airtime and dash. `IsJumping` and
`IsFalling` describe airborne vertical phase independently of the State enum.

| Situation, using existing defaults on flat ground | State | Horizontal / normalized speed | Grounded | Vertical Velocity | Moving / Running / Jumping / Falling |
| --- | --- | --- | --- | --- | --- |
| Settled idle | Idle | ~0 / ~0 | true | ~-2 | false / false / false / false |
| Steady walk | Walk | ~4 / ~0.57 | true | ~-2 | true / false / false / false |
| Steady run | Run | ~7 / ~1 | true | ~-2 | true / true / false / false |
| Ascent | Jump | depends on takeoff/steering | false | positive | speed-dependent / false / true / false |
| Apex/descent | Fall | depends on momentum/steering | false | <=0 | speed-dependent / false / false / true |
| Landing window | Land | depends on motion | true | ~-2 | speed-dependent / false / false / false |

Existing names Walk/Run/Jump/Fall cover Moving/Running/Jumping/Falling without
renaming the enum. Priority stays Dash > airborne Jump/Fall > Land > Idle/Run/Walk.
Run begins above Walk Speed + Run Speed Threshold (4.1 by default); therefore
acceleration/deceleration can delay a state change relative to Shift. Land takes
priority for 0.1 seconds but does not block input. IsRunning is false during Land,
Dash, and airtime. Normalization clamps dash/high airborne speeds to 1.
Dash State describes any dash travel in that frame; IsDashing describes time
remaining and can already be false on the final partial dash frame.

Expected values above are code-derived expectations, **not Unity observations**.

## Animation-driver runtime inspection

Unity Editor is unavailable here; every new check remains **Not tested**.
When Unity is available, add `PlayerAnimationDriver` to the existing Player root
beside `ThirdPersonPlayer`, then inspect **Runtime Animation (Read Only)** in
Play Mode. No references, Animator, controller, clips, or rig are required.
Compare against the movement panel after an Update/LateUpdate pair. Use Editor
Pause and frame stepping for brief Land and final Dash samples; do not tune feel.

- [ ] Not tested — Driver import/setup: scripts compile and the read-only animation panel appears after adding the driver to Player; no reference assignment is required.
- [ ] Not tested — Driver idle: after settling, State is Idle, Normalized Movement Speed is approximately 0, Grounded is true, Vertical Velocity matches the player's ground stick, and Walking/Running/Jumping/Falling/Landing/Dashing are false.
- [ ] Not tested — Driver walk: WASD on unobstructed flat ground produces State Walk and Walking true; Running/Landing/Dashing are false and normalized speed matches the player (about 0.57 at steady default walk speed).
- [ ] Not tested — Driver run: Left Shift with WASD produces State Run and Running true after acceleration; Walking is false and normalized speed matches the player (about 1 at steady default run speed); release Shift and verify the flags follow the player's deceleration/state change.
- [ ] Not tested — Driver jump: Space produces State Jump, Grounded false, positive Vertical Velocity and Jumping true; Walking/Running/Falling/Landing are false when not dashing.
- [ ] Not tested — Driver fall: at the apex/descent and when walking off the ground, State is Fall, Grounded false, Vertical Velocity is zero/negative, Falling true and Jumping false when not dashing.
- [ ] Not tested — Driver land: contact copies Just Landed and Grounded from the player; Landing follows State Land for its existing brief window, then clears for Idle/Walk/Run or an interrupting Jump/Dash; no extra landing delay is introduced.
- [ ] Not tested — Driver dash: ground and airborne dashes show State Dash and Dashing true, including the final partial dash frame where the player's remaining-time Dashing may already be false; airborne Jumping/Falling still match the player, Landing is false while Dash has priority, and the next state follows the player.
- [ ] Not tested — Driver without animation assets: with no Animator/controller/clips/rig, values update and no new repeated Console errors occur; walk/run/jump/fall/land/turn/dash and camera controls remain unchanged. Disabling only the driver also leaves movement and camera controls intact.
- [ ] Not tested — Driver lifecycle: disable/re-enable the driver and then the movement component; samples remain unchanged while either is disabled and resume after the next active Update/LateUpdate pair. Editor Pause freezes values; a zero-time movement Update clears Just Landed without advancing other movement samples.

## Future Hanuman presentation verification

Architecture and later setup: [HANUMAN_PRESENTATION.md](HANUMAN_PRESENTATION.md).
No hierarchy, model, Animator, or runtime component was added by this preparation.
Execute and record the existing locomotion and animation-driver checklist before
changing or extending those systems. Then use a temporary test copy of the scene
for presentation integration, preserving the baseline and all existing results.
Keep `Player/Visual` as the presentation container; a future `CharacterModel`
child holds the replaceable asset. Keep unavailable model/Animator checks
**Not tested** until the relevant assets and authorized integration exist.

- [ ] Not tested — Collision isolation: add and replace a visual model below Visual; Player's CharacterController remains its only collider, with the same settings and obstacle/ground contact behavior. Imported visuals add no active colliders or rigidbodies.
- [ ] Not tested — Movement following: the active model remains parented below Visual and follows Player during idle, walking, running, stopping, and turns, without accumulating local displacement.
- [ ] Not tested — Visual orientation: the aligned model faces Player-local +Z with +Y up and turns with Player; any imported-axis correction stays inside CharacterModel, with no corrective rotation applied to Player.
- [ ] Not tested — Movement authority: compare baseline movement and stationary/input-directed dash before and after temporarily rotating only CharacterModel; Player's gameplay heading, controller, and dash direction remain authoritative. Restore the model alignment afterward.
- [ ] Not tested — Camera unchanged: Main Camera still targets Player and Player still references Main Camera; orbit, pitch limits, follow, Escape, click recapture, and focus behavior match the baseline during model replacement/removal.
- [ ] Not tested — Scale and pivot: Player and Visual retain unit scale and Visual retains identity local position/rotation; the model body fits the height-2, radius-0.5 controller, with reference-pose feet at Player-local Y = -1. Crown/tail extensions do not require controller changes.
- [ ] Not tested — Jump/fall attachment: jump from idle, walk, and run, then walk off the ground; ascent, descent, and landing keep the model attached without detaching, accumulating offsets, or changing gameplay motion.
- [ ] Not tested — Dash attachment: ground, stationary, and airborne dashes keep the model attached and aligned without residual position/rotation offsets; existing dash timing and mechanics match the baseline.
- [ ] Not tested — Model removal: disable and then remove only the model subtree in the temporary scene; walking, running, jumping, falling, landing, turning, dash, collision, and camera remain functional. Restore the model without replacing Player.
- [ ] Not tested — Driver without presentation: with PlayerAnimationDriver on Player and no model or Animator, the read-only driver values still match movement after Update/LateUpdate and no missing-reference errors occur.
- [ ] Not tested — Future Animator isolation: when animation integration is separately available, Animator lives within the model subtree with Apply Root Motion off; the nine proposed parameters consume PlayerAnimationDriver values with verified sampling order, preserve existing semantics, and do not move/rotate Player or change locomotion timing. Removing the Animator/model remains supported.
- [ ] Not tested — Console and lifecycle: no new Console errors or exceptions during model addition, replacement, removal, restoration, and Play Mode restart; any future presentation consumer tolerates missing Animator/model/optional anchors and rebinds references when replaced.

## Static Hanuman visual tuning after runtime gates

Plan and exact existing transforms: [HANUMAN_VISUAL_TUNING.md](HANUMAN_VISUAL_TUNING.md).
This is a later Unity session workflow; no visual or runtime verification was
performed during the documentation audit. Run the gates in order. For tonight,
presentation isolation means the existing static Visual, not a new CharacterModel
or Animator. Future imported-model/Animator checks above remain Not tested until
their separate integration exists. Do not add assets or another runtime system
to clear this gate. Restore any temporary runtime-check settings before art trials;
do not tune movement or camera as part of visual tuning.

- [ ] Not tested — 1. Movement/runtime gate: complete and record the existing runtime checklist above at the baseline settings; resolve failures before proceeding.
- [ ] Not tested — 2. Animation-driver gate: complete the existing PlayerAnimationDriver setup and inspection checks above, including state agreement and unchanged movement; no Animator or animations are needed.
- [ ] Not tested — 3. Current presentation-isolation gate: verify the existing Visual follows Player through movement/turn/jump/land/dash; temporarily disable and restore Visual in Play Mode and confirm controller, movement, camera and driver still work. Confirm Visual identity transform, +Z orientation, feet around Player-local Y = -1 and no visual colliders. Restore Visual before proceeding; leave future replacement/Animator checks untested.
- [ ] Not tested — 4. Visual baseline: only after gates 1–3 pass, capture front, front 3/4, side, rear and normal gameplay-distance views of the current Hanuman. Use Scene view navigation and existing gameplay orbit without editing Player or camera. Identify which audit hypotheses actually appear.
- [ ] Not tested — 5. Controlled face tuning: follow the tuning document's ordered experiments, one visual change group at a time; keep a before/after record and accept or revert each trial before the next. Keep Player, CharacterController, movement, camera and Visual boundary unchanged; edit only existing visual leaves in temporary trials.
- [ ] Not tested — 6. Proportion regression: after significant head/body/limb/foot/tail proportion changes, briefly recheck walk/run/turn/jump/land and existing dash, feet/contact, visual attachment and camera behavior against the passed baseline. Revert problematic visual changes instead of compensating in gameplay.
- [ ] Not tested — 7. Capture best values: record exact target paths, before/accepted local position/rotation/scale, matching view captures and keep/revert reasons before leaving Play Mode. Record movement recheck results and deferred trials. Preserve the source SampleScene; applying accepted values persistently is a later step.
