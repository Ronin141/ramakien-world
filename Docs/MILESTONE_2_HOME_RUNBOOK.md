# Milestone 2 Home Verification Runbook

**Status: Not tested. Unity Editor is unavailable here; no Unity verification has been performed.** Follow this document top to bottom tonight. All values below are existing expectations, not observed results. Leave results unselected until actually observed.

Consolidated from [Home Test](MILESTONE_2_HOME_TEST.md), [animation bridge](PLAYER_ANIMATION_BRIDGE.md), [presentation contract](HANUMAN_PRESENTATION.md), and [visual audit](HANUMAN_VISUAL_TUNING.md). Those documents remain unchanged; the steps needed for this session are included here.

## 1. Before Play Mode

- [ ] Not tested — Open `Assets/Scenes/SampleScene.unity` in Unity **6000.6.3f1**. Allow import/compilation, open **Console**, and check for errors before continuing.
- [ ] Not tested — Select the scene-root **Player**. Keep `ThirdPersonPlayer` and `CharacterController` on it. If absent, add the existing **PlayerAnimationDriver** alongside them. It needs no reference assignment, Animator, controller, clips, or rig.
- [ ] Not tested — Confirm no unintended Inspector changes. Baseline: walk/run **4/7**, jump height/gravity **1.5/-20**, turn speed **540**, acceleration/deceleration/air acceleration **45/26/20**, ground stick **2**, Land duration **0.1**, moving/run thresholds **0.05/0.1**. Record discrepancies rather than silently retuning.
- [ ] Not tested — Confirm Player starts at position `(0, 1.1, 0)`, zero rotation, unit scale. `Player/Visual` remains parented at local position/rotation zero and unit scale. CharacterController: height **2**, radius **0.5**, center `(0, 0, 0)`, slope **45**, step **0.3**, skin width **0.05**, minimum move distance **0**. Main Camera targets Player; Player's movement camera references Main Camera. Do not edit these to fit the visuals.
- [ ] Not tested — Enter Play Mode, click Game view to capture input, and keep the Player Inspector visible/locked. Confirm **Runtime Movement (Read Only)** and **Runtime Animation (Read Only)** appear. Wait for a movement Update followed by driver LateUpdate before interpreting samples.

Controls: **WASD** move, **Left Shift** run, **Space** jump, **Left Ctrl** dash, mouse orbit, **Escape** release, click Game view to recapture. Stop/restart Play Mode after falling off the ground; there is no respawn. Record temporary values before stopping because Play Mode changes normally revert. Preserve the source scene and existing working-tree changes; do not save experimental settings over them.

## 2. Runtime Gate A — Existing movement

Watch the movement panel's **State, Horizontal Speed, Normalized Movement Speed, Grounded, Vertical Velocity, Moving, Running, Jumping, Falling, Just Landed, Dashing**. Use default values on unobstructed flat ground for speed comparisons.

| Status | Execute in order | Expected observation |
| --- | --- | --- |
| [ ] Not tested | A1. Release input and settle. | Brief Land, then Idle; horizontal/normalized speed ≈ 0, Grounded true, vertical velocity ≈ -2; Moving/Running/Jumping/Falling false; no drift. |
| [ ] Not tested | A2. Walk W/A/S/D, then W+D and W+A. Start/stop and reverse direction. | Walk settles near 4 units/sec, normalized ≈ 0.57. Diagonals are no faster. Acceleration is responsive; release decelerates cleanly; turns/reversals remain smooth and controllable. |
| [ ] Not tested | A3. Hold Shift while walking; release Shift, then all input. Try Shift while stationary. | Run approaches 7 units/sec, normalized ≈ 1; returns to Walk then Idle through deceleration. Stationary Shift does not move. Repeat diagonal comparison while running. State follows measured speed, not the key alone; Run begins above 4.1 at baseline. |
| [ ] Not tested | A4. Orbit 90° and 180°, then press W; orbit while moving and change W to A/S/D. Look up/down. | Movement follows camera heading; Player turns smoothly, without snapping/jitter or travel-speed changes from camera pitch. |
| [ ] Not tested | A5. Jump from idle, walking, and running; watch ascent → apex → fall → contact. Release WASD/Shift during a running jump. | Jump: Grounded false, positive vertical velocity. Apex/descent: Fall, velocity ≤ 0. Airborne momentum persists after release. Contact: Grounded true, velocity ≈ -2, brief Land (0.1 sec), then Idle/Walk/Run; no floating, jitter or input lock. Just Landed is a one-update pulse. |
| [ ] Not tested | A6. Hold Space through landing; separately press Space while airborne, then make a fresh grounded press. Walk off the edge once. | No held-key repeat, double jump, or buffered landing jump; fresh grounded press works. Leaving the edge enters Fall without Space. Stop/restart Play Mode to recover. |
| [ ] Not tested | A7. Tap Ctrl while moving, stationary, and airborne; also hold it. | Existing burst/cooldown and return to ordinary movement remain intact; stationary dash follows Player forward. Holding does not repeat. Gravity continues during airborne dash; Jumping/Falling still describe its vertical phase. |
| [ ] Not tested | A8. Observe ground contact and existing collision behavior. If a safe existing obstacle is available, check its underside during a jump. | No floating or ground penetration; underside contact stops continued upward velocity. If no safe obstacle exists, leave that ceiling check Not tested; do not create one. |
| [ ] Not tested | A9. Orbit through pitch limits; release with Escape, click to recapture, switch focus away/back. Review Console after Play Mode restart. | Follow/orbit/pitch limits unchanged; no new movement input while unlocked/unfocused or stuck keys on return. Existing deceleration/airborne momentum may continue. No new errors/exceptions. |

Existing configuration checks, kept separate from feel tuning: perform temporary Play Mode trials one setting at a time and restore the recorded baseline before Gate B. These are the checks already in the Home Test, not a request to adopt new settings.

- [ ] Not tested — Walk/run settings respond independently; both at 0 give normalized speed 0 without invalid values; restore **4/7**. Jump height and negative gravity respond; nonnegative gravity clamps negative; restore **1.5/-20**.
- [ ] Not tested — Turn speed, acceleration, deceleration and air acceleration respond independently; restore **540/45/26/20**. Ground stick **2** and Land duration **0.1** retain clean contact/responsiveness. Moving/run threshold changes affect classification without changing displacement/turn speed; restore **0.05/0.1**. Do not keep movement tuning changes from these trials.

## 3. Runtime Gate B — PlayerAnimationDriver

Compare the two read-only panels after **Update → LateUpdate**. Driver fields to watch exactly:

**State, Normalized Movement Speed, Grounded, Vertical Velocity, Walking, Running, Jumping, Falling, Landing, Dashing, Just Landed.** These correspond to `State`, `NormalizedMovementSpeed`, `IsGrounded`, `VerticalVelocity`, `IsWalking`, `IsRunning`, `IsJumping`, `IsFalling`, `IsLanding`, `IsDashing`, `JustLanded`.

Repeat the short action sequence below. Use Editor Pause/frame stepping to catch Land and the final dash travel frame; a missed brief frame is not a verified pass.

| Status | Action / State | Driver values to observe |
| --- | --- | --- |
| [ ] Not tested | Settle / Idle | Speed ≈ 0, Grounded true, vertical velocity ≈ -2; Walking/Running/Jumping/Falling/Landing/Dashing all false. |
| [ ] Not tested | WASD / Walk | Walking true, speed ≈ 0.57 at steady walk; Grounded true. Running/Jumping/Falling/Landing/Dashing false. |
| [ ] Not tested | Shift + WASD / Run | Running true, Walking false, speed ≈ 1 at steady run; Grounded true. Jumping/Falling/Landing/Dashing false. Release Shift: state/flags follow measured deceleration. |
| [ ] Not tested | Space / Jump | Grounded false, positive vertical velocity, Jumping true; Walking/Running/Falling/Landing/Dashing false when not dashing. |
| [ ] Not tested | Apex/descent or walk off edge / Fall | Grounded false, vertical velocity ≤ 0, Falling true; Walking/Running/Jumping/Landing/Dashing false when not dashing. |
| [ ] Not tested | Contact / Land | Grounded and Just Landed match player. Landing true during the existing Land window, not just the contact pulse; clears for Idle/Walk/Run or an interrupting Jump/Dash. No extra landing delay. |
| [ ] Not tested | Ground and airborne Ctrl / Dash | State Dash and Dashing true; Walking/Running/Landing false. Grounded/vertical velocity match player; airborne Jumping/Falling continue to match phase. Speed remains the player's normalized sample, clamped to 0..1. Exit follows the player's next state, not necessarily Idle. |

Walking/Running/Landing follow state priority: **Dash > airborne Jump/Fall > Land > Idle/Run/Walk**. Jumping/Falling are phase flags and may coexist with Dashing. Just Landed copies the player pulse; Dash can mask Land even during contact.

- [ ] Not tested — **Observe the final partial dash frame.** Pause and frame-step through dash completion. On the final frame with dash travel, the player's remaining-time **Dashing can already be false**, while player **State remains Dash** and driver **Dashing must remain true**. Capture that distinction and then the next state. Repeat if missed; do not assume correctness from ordinary dash motion.
- [ ] Not tested — **Observe disabled-source / retained-sample behavior.** While the driver holds a non-idle sample, disable only the driver; its values should stay unchanged while movement/camera still work. Re-enable and observe sampling resume after the next active Update/LateUpdate pair. Then disable only `ThirdPersonPlayer`: driver values must retain their last sample, not reset to Idle/zero. Re-enable and observe fresh samples resume. Restore both enabled. Retained values are stale, not proof the disabled player is still moving.
- [ ] Not tested — Editor Pause freezes samples. If using the existing time-scale-pause diagnostic, a zero-time movement Update clears Just Landed while other movement values remain the previous sample. Do not add a pause system for this check. Verify no new Console errors through these lifecycle tests.

## 4. Runtime Gate C — Presentation isolation

Use the **existing static Visual**. No real model, CharacterModel container, Animator or animation assets are required; future imported-model/Animator tests remain Not tested.

- [ ] Not tested — Player remains the gameplay root, with movement/rotation authority and the sole CharacterController. Visual stays at identity local transform/unit scale, faces Player-local +Z with +Y up, and has no colliders/rigidbodies/gameplay scripts. Feet remain around Player-local Y = **-1**; crown/tail extensions do not require collision changes.
- [ ] Not tested — Watch Visual during idle, walk/run, stopping, turns, jump/fall/land and ground/air dash: it stays attached without accumulating position/rotation offsets. Controller/ground-contact behavior matches Gate A; camera wiring, orbit, follow and focus behavior remain unchanged.
- [ ] Not tested — Temporarily **disable Visual only** in Play Mode. Repeat walk/run/turn/jump/land/dash and camera controls while watching Player/runtime values. Movement/collision/camera still function, driver samples still agree, and no missing-presentation errors occur without Animator/clips/rig. Restore Visual and confirm attachment. This is tonight's presentation-absence test; no deletion from the source scene is needed. Leave future model replacement/removal integration checks pending.

## 5. Decision gate

**STOP visual tuning if any Gate A/B/C item fails.** Capture the failing action/behavior, Console errors and relevant Inspector/runtime values (including both panels for driver issues). Record baseline settings and reproduction steps. Do not compensate for a gameplay bug by changing visuals, collision, movement or camera.

Proceed only after the applicable runtime checks are observed and the foundation is stable. An unobserved required behavior remains Not tested; do not silently count it as PASS. An unavailable safe-obstacle ceiling check, conditional time-scale diagnostic and future asset/Animator integration checks remain explicitly pending rather than prompting new setup.

## 6. Hanuman visual tuning session

Capture the unchanged five views in section 7 first. Work in temporary Play Mode trials and record values before exiting. Only adjust existing visual **leaf** objects under `Player/Visual`; preserve Player, CharacterController, movement rotation/settings, camera, Visual identity transform and feet near Y = -1. No materials, meshes, new objects or animation systems are needed. Do not scale Head/Crown/limb containers: their pivots are at the Player origin.

**For every experiment and separate subtrial, record each affected object's exact path, original value, tested value, keep/revert decision and brief visual observation.** Use local Inspector position/rotation/scale, not serialized quaternion numbers in Euler fields. Record attachment corrections too, so the group can be reverted together.

One design variable at a time: a mirrored pair or coherently resized eye assembly counts as one variable. Decide keep/revert before the next trial. Size, spacing and angle are separate trials. Skip a suspected problem if the baseline does not show it. No numerical winning values are prescribed.

All paths below are relative to `Player/Visual`; Left/Right means the corresponding existing named objects.

| Status / order | Experiment | What to inspect before recording keep/revert |
| --- | --- | --- |
| [ ] Not tested — 1 | **Muzzle depth:** reduce `Head/Muzzle` scale Z only, holding position and other scales. If Nose needs reseating, record its Z correction; the audit's source-derived relation is `Nose position-Z change = 0.5 × Muzzle scale-Z change`. | Side/3/4 projection and connected nose, mouth, fangs; retain a projecting monkey muzzle. |
| [ ] Not tested — 2 | **Muzzle width:** after depth is accepted/reverted, test less `Head/Muzzle` scale X only. | Front width against cheeks/skull, then gameplay distance; do not erase the existing taper. |
| [ ] Not tested — 3 | **Muzzle vertical position:** if crowded, test lower local Y at fixed size. If needed, move `Head/Nose` by the same Y delta to retain seating. | Eye-to-mouth separation and jaw seam. Mouth/jaw/fangs are siblings and do not follow; reject detachment or record a separate fit correction before judging likeness. |
| [ ] Not tested — 4 | **Eye size:** if dominant, reduce in-plane size coherently on both sides' `Head/LeftEye`, `LeftEyeWhite`, `LeftPupil` and Right equivalents. Preserve centers, angles and Z layers; leave brows unchanged. | Angular shape, visible whites/pupils and expression at gameplay distance. |
| [ ] Not tested — 5 | **Eye spacing:** if too wide, shift each eye assembly plus its matching brow inward by the same mirrored X delta. Preserve pupil offsets within eyes. | No crossed-eye effect, hidden far eye or floating layers; compare unchanged spacing. |
| [ ] Not tested — 6 | **Brow exposure:** test only forward Z placement of `Head/LeftBrow` and `RightBrow` if lost. | Clear brow planes without floating bars or eye occlusion. |
| [ ] Not tested — 7 | **Skull width/height:** test narrower `Head/Skull` scale X first. Accept/revert; only then test taller scale Y separately if still needed. Hold center fixed. | Eyes/cheeks/ears/neck remain seated; skull must not swallow the crown base. |
| [ ] Not tested — 8 | **Ears:** if dominant, resize outer/inner ear pairs coherently. Accept/revert; separately test inward X placement if needed for skull contact. Retain angles. | Smaller swept ears with seated inner inserts, not absent or detached ears. |
| [ ] Not tested — 9 | **Crown:** if the point is lost, test `Crown/Spire` height first. Preserve its base with `position-Y change = 0.5 × scale-Y change`. Only afterward test tier separation or mirrored flame exposure in separate trials if needed. | Attached pointed taper from front/side/rear and gameplay distance; do not resize all crown pieces together. |
| [ ] Not tested — 10 | **Jaw/cheeks:** test `Head/LowerJaw` scale Y for clearer definition first. Accept/revert; separately test forward Z of `Head/LeftCheek`/`RightCheek` if swallowed by other volumes. | Distinct connected lower face, no chin slab or round cheek puffs; retain cheek widths/angles. |
| [ ] Not tested — 11 | **Mouth/fangs:** if hidden, first test forward Z of `Head/Mouth` at fixed size. Accept/revert; separately test the existing Left/RightFang pair's visibility while attached. Do not simultaneously enlarge mouth rims. | Readable opening and restrained fangs rather than a broad smile, floating teeth or tusks. |

Use one record per object/subtrial:

`Experiment / path: ___ | Original: ___ | Tested: ___ | KEEP / REVERT: ___ | Visual observation + view: ___`

Choose the best face before considering further body/polish work. No need to complete every trial tonight. After significant proportion changes, briefly recheck movement/contact/attachment; revert visual problems rather than compensating in gameplay. Persisting accepted values to the source scene is later work.

## 7. Visual checkpoints

Use these **before tuning**, after each trial in relevant views, and across all views when choosing the final candidate. Navigate Scene view for inspection; do not reposition Player or edit Main Camera. Use ordinary existing orbit for Game view and keep A/B framing consistent.

| View | Quick check |
| --- | --- |
| Front | Muzzle does not overwhelm eyes/cheeks; lower jaw/opening, swept ears and crown taper remain distinct. |
| Front 3/4 | Readable brow/cheek planes and muzzle taper; nose/jaw/fangs stay connected and far eye remains legible. |
| Side | Controlled snout projection, attached jaw/neck/crown; no floating features or foot/ground mismatch. |
| Rear | Crown, white body, red/gold waist/back panel and attached curled tail remain distinct. |
| **Normal gameplay camera — most important** | At unchanged distance (baseline 5), actual Game view resolution and ordinary movement/turning, the combined silhouette reads clearly. A close-up improvement that worsens gameplay silhouette should be reverted or deferred, not automatically kept. |

Tiny pupils/fangs need not resolve at every distance. Favor the combined face and silhouette over isolated detail.

## 8. Final regression pass

Run while the accepted temporary visual values are still present, before leaving Play Mode. Compare against passed Gates A/B/C; do not retune gameplay.

- [ ] Not tested — **Idle → walk → run → stop/turn:** same no-drift idle, 4/7 steady speeds, acceleration/deceleration and smooth direction changes; visuals stay attached.
- [ ] Not tested — **Jump → apex/fall → land:** same state/velocity/grounding behavior, clean feet/contact and no detachment; driver agrees.
- [ ] Not tested — **Dash:** moving/stationary/airborne behavior and recovery remain unchanged; no residual visual offsets.
- [ ] Not tested — **Camera and Console:** orbit/follow/pitch limits, Escape/click recapture and focus behavior remain unchanged; no new errors/exceptions. Record winning values and captures before stopping, then check Console through Play Mode restart.

If regression fails, return to the STOP procedure in section 5. Record which visual group was reverted and whether the problem persists at the original visual baseline.

## 9. Session result template

Leave choices unselected until observed; note untested/deferred checks under Remaining issues rather than inferring a pass.

### Milestone 2 Home Test Result

- Movement: PASS / FAIL
- Animation driver: PASS / FAIL
- Presentation isolation: PASS / FAIL
- Console clean: YES / NO
- Hanuman tuning attempted: YES / NO

Best Hanuman changes:

- ...

Reverted changes:

- ...

Remaining issues:

- ...

Next recommended task:

- ...
