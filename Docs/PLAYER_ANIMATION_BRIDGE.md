# Player animation bridge

`ThirdPersonPlayer` remains the authority for input, movement, collision-resolved
speed, grounding, gravity, state priority, landing duration, and dash timing.
`PlayerAnimationDriver` caches the player on the same GameObject in Awake and
copies its published values in LateUpdate, after movement Update. It has no
input, movement calculations, timers, state machine, or Animator dependency.
It reuses `ThirdPersonPlayer.MovementState` rather than defining another enum.

## Setup and inspection

When Unity is available, add `PlayerAnimationDriver` to the existing Player root
alongside `ThirdPersonPlayer`. No reference wiring, Animator, controller, clips,
or rig is required. The existing prototype also works without adding the driver.
No scene or prefab has been changed. In Play Mode, inspect the driver's
**Runtime Animation (Read Only)** panel beside **Runtime Movement (Read Only)**.

Properties are read-only snapshots, not serialized settings. The first meaningful
sample requires movement Update followed by driver LateUpdate. Before that they
have C# defaults, not a confirmed grounded/idle sample. Disabled/inactive drivers
or disabled/inactive/missing players retain the last sample; consumers must not
treat it as fresh. Re-enabling resumes sampling on the next active LateUpdate.
Editor Pause freezes samples; a zero-delta-time movement Update clears JustLanded
but otherwise retains movement's previous values. No runtime logging is added.

## Proposed Animator parameters (not created or written yet)

| Parameter | Type | Driver property / authoritative source |
| --- | --- | --- |
| Speed | float | NormalizedMovementSpeed, copied from player: measured HorizontalSpeed / max(walkSpeed, runSpeed), clamped 0..1; zero when both configured speeds are zero |
| Grounded | bool | IsGrounded, copied from player's latest Move contact result, excluding ascent |
| VerticalVelocity | float | VerticalVelocity, copied gravity accumulator; includes negative ground-stick velocity |
| IsWalking | bool | IsWalking, player State == Walk |
| IsRunning | bool | IsRunning, copied from player (State == Run) |
| IsJumping | bool | IsJumping, copied from player: airborne ascending phase |
| IsFalling | bool | IsFalling, copied from player: airborne apex/descending phase |
| IsLanding | bool | IsLanding, player State == Land, using its existing presentation window |
| IsDashing | bool | IsDashing, player State == Dash, including final partial dash frame |

`State` exposes the authoritative presentation state directly. `JustLanded` copies
the player's one-Update contact pulse for diagnostics or a future landing event;
it is not a proposed trigger yet. `IsLanding` is the existing Land window, not
that pulse. Dash may mask Land even when JustLanded is true. The driver's
IsDashing intentionally follows presentation State, unlike the player's
remaining-time IsDashing, which can become false on the final dash travel frame.
Jumping/Falling remain available during airborne Dash; flags are not all mutually
exclusive. Walking/Running/Landing follow the selected presentation state.

## Expected future states and transitions

Use Idle, Walk, Run, Jump, Fall, Land, and Dash, with the existing priority:
**Dash > airborne Jump/Fall > Land > Idle/Run/Walk**. Idle/Walk/Run follow measured
speed classification (including acceleration/deceleration), not key presses.
Jump changes to Fall at the apex; walking off an edge enters Fall directly.
Ground contact enters Land when its existing duration permits and Dash is not
overriding it. Land yields to Idle/Walk/Run when the controller's window ends,
or immediately to Jump/Dash if gameplay selects those states. Dash exits to the
controller's current airborne, landing, or grounded state; it need not return to Idle.

A future Animator should respect this priority, particularly Dash over airborne
flags. Speed supplies locomotion blending (~0 idle, ~0.57 walk, ~1 run using
current defaults); dash/fast airborne speeds saturate at 1. Grounded disambiguates
ground stick from actual falling. Clip transitions, blending durations, and
landing visuals wait for real assets and Unity verification. Keep root motion
disabled when adding an Animator so the CharacterController retains movement
authority. A future parameter writer can live in this driver's LateUpdate;
consumers in another LateUpdate would need explicit ordering to read this frame.

These are code-derived expectations only. No Animator reference, parameter
writes, clips, controllers, root motion, or movement tuning were added.
Unity compilation, Inspector behavior, and runtime state timing remain unverified.
The runtime gate is [MILESTONE_2_HOME_TEST.md](MILESTONE_2_HOME_TEST.md).
