# Hanuman presentation boundary and asset contract

Status: documentation only. Unity is unavailable; scene import, compilation,
appearance, and runtime behavior are not verified. Execute and record the existing
[Home Test](MILESTONE_2_HOME_TEST.md) locomotion and animation-driver checks before
changing or extending either system. The integration steps below are future work.

## Existing architecture discovered statically

`Assets/Scenes/SampleScene.unity` contains a scene-root Player, not a prefab:

```text
Player (ThirdPersonPlayer, CharacterController)
└── Visual (Transform only; identity local transform)
    ├── Body, Neck, Head, Crown
    ├── LeftArm, RightArm, LeftLeg, RightLeg
    ├── WaistDecoration
    └── Tail
Main Camera (ThirdPersonCamera; separate scene root)
```

- Player Transform fileID `476662782` has position `(0, 1.1, 0)`, identity
  rotation, unit scale, and no parent. Its only child is Visual (`444910293`).
- CharacterController is on Player: height 2, radius 0.5, center `(0, 0, 0)`.
  The capsule bottom is therefore Player-local Y = -1. Keep the existing
  controller settings and root pivot; Player is not a feet-origin transform.
- Visual and its descendants have only Transform, MeshFilter, and MeshRenderer
  components. There are no visual colliders, rigidbodies, Animator components,
  skinned meshes, or gameplay scripts in that subtree. This is a static blockout,
  not a skeleton. Named hands, feet, and Tail are not a rig/anchor contract.
- Body/limbs/tail use primitives. Head parts reference `Assets/HanumanFace.obj`
  through its importer metadata. Four shared materials live in `Assets/Materials`.
  Prototype notes specify local +Z forward and feet at Player-local Y = -1;
  appearance and actual ground alignment still require Unity inspection.
- `ThirdPersonPlayer` owns input, CharacterController.Move, gravity, state, dash,
  and Player rotation. Camera yaw determines movement direction; movement turns
  the gameplay root toward input or the stored dash direction. A stationary dash
  falls back to Player's `transform.forward`.
- `ThirdPersonCamera` on Main Camera (`330585546`) updates camera yaw before
  movement and follows Player in LateUpdate. Its target references Player;
  Player's movementCamera references Main Camera. Neither references Visual.
- `PlayerAnimationDriver` exists in source but is not serialized on Player in
  this scene. It requires/caches ThirdPersonPlayer on the same GameObject and
  samples it in LateUpdate. It does not find or require a model or Animator.
  Its Editor inspector only displays these values. No Player prefab was found.

Future visual code that rotates Player would compete with movement's rotation
and could alter stationary dash direction. Scaling or moving Player to fit a
mesh would also change the gameplay/collision relationship. All model alignment
and eventual visual-only pose changes must stay below the gameplay root.

## Smallest presentation boundary

Keep the existing name and transform `Player/Visual`; it already serves as the
presentation root. No rename, additional wrapper, runtime component, reference
registry, or model-swap API is needed now. Transform parenting supplies following
without another Update loop. The existing scripts have no visual references to
repair when a model is absent.

When a real asset is available, use this layout in Unity:

```text
Player (existing movement/collision; PlayerAnimationDriver after Home Test setup)
└── Visual (stable presentation container; local position/rotation zero, scale one)
    └── CharacterModel (replaceable asset instance; future Animator here)
        ├── Rig / root bone / hips / remaining bones
        ├── Meshes
        └── Optional anchors under the appropriate bones
```

CharacterModel is a role, not a required imported object name. Put the Animator
on the model/rig root appropriate to the imported asset, inside CharacterModel's
replaceable subtree. Preserve the imported rig hierarchy. Model-specific axis,
pivot, and scale corrections belong on CharacterModel (or an alignment wrapper
inside that subtree when the asset needs one), not Player or the stable Visual.

The current blockout parts are directly under Visual. Later, preserve them as a
disabled prototype group while testing a replacement, then keep only the intended
model active. Grouping must preserve their existing transforms. No scene grouping
or asset replacement is performed by this task. Earlier prototype notes permit
replacing Visual wholesale; this contract instead retains it as a stable container
and replaces its contents. Neither approach requires movement changes, but keeping
the container avoids unnecessary future presentation-reference churn.

Ownership stays one-way: movement publishes state, PlayerAnimationDriver samples
it, and a future presentation consumer applies it to an optional Animator. No
presentation component writes input, controller settings, gameplay transforms,
timers, or state back to movement. Do not create a second movement-state cache or
state machine. Future Animator/anchor references belong to presentation and must
be cleared/rebound on replacement; absent models, Animators, or anchors must be a
supported no-op. No such consumer or replacement code is implemented yet.

## Transform and rig integration contract

- Use +Y up and +Z forward after model alignment, matching Player's forward.
  Start with identity authored root orientation where practical. Correct any
  imported axis mismatch locally within CharacterModel, never by rotating Player.
- Target one Unity unit per meter, unit scale on Player and Visual, and preferably
  unit scale on the imported model after import configuration. If correction is
  necessary, use uniform scale within CharacterModel; avoid negative/nonuniform
  scale. Fit the body to the existing height-2, radius-0.5 capsule; do not resize
  the controller to accommodate the asset. Crown/tail may extend beyond it and
  have no independent collision in this milestone.
- Prefer a model asset origin centered between the feet on its ground plane.
  For that asset convention at unit scale, set CharacterModel local position to
  `(0, -1, 0)` under identity Visual. For a center-origin asset, use the measured
  model offset instead. The invariant is feet at Player-local Y = -1 in the
  reference pose, not a universal -1 offset for every imported asset. Verify
  alignment in Unity; do not move Player's pivot or change controller center.
- Prefer a valid Humanoid rig/avatar for a compatible biped if future animation
  retargeting needs it. A specific vendor, bone naming scheme, or purchased asset
  is not assumed. Inspect the real rig before selecting import configuration;
  non-humanoid extras such as the tail need separate future assessment.
- Keep one identifiable skeleton root within the model subtree, with hips and
  the remaining skeleton below it as supplied by the asset. The rig root and
  hips are visual transforms, never the Player gameplay root. Document the actual
  root/hips mapping during import; do not reparent bones to Player.
- Initially disable Animator Apply Root Motion. Future clips should present
  movement in place without accumulating whole-model displacement or rotation.
  Hips may pose within the rig; they must not drive Player or its controller.
  Do not add OnAnimatorMove movement forwarding. Any future root-motion design
  would require a separately reviewed gameplay change after runtime verification.
- Make right/left hand, head, and chest/spine bones identifiable for later
  attachments. Keep needed transforms accessible when configuring the real rig;
  verify anchor access before enabling any bone-stripping/optimization option.
  No model is rejected now for missing optional extras.

## Future Animator contract

Animator stays inside the replaceable model subtree, not on Player or the stable
Visual container. PlayerAnimationDriver stays on Player with ThirdPersonPlayer;
placing that driver on the model would violate its same-object movement contract.
Animator consumes driver output through a future optional parameter writer; no
writer, Animator reference, controller, clips, or animation behavior is added here.

The existing [animation bridge contract](PLAYER_ANIMATION_BRIDGE.md) is the source
of truth. Proposed parameter names/types and their driver sources are:

| Parameter | Type | PlayerAnimationDriver property |
| --- | --- | --- |
| Speed | float | NormalizedMovementSpeed (0..1, not meters/second) |
| VerticalVelocity | float | VerticalVelocity (gravity accumulator, including ground stick) |
| Grounded | bool | IsGrounded |
| IsWalking | bool | IsWalking |
| IsRunning | bool | IsRunning |
| IsJumping | bool | IsJumping |
| IsFalling | bool | IsFalling |
| IsLanding | bool | IsLanding |
| IsDashing | bool | IsDashing |

Preserve Dash > airborne Jump/Fall > Land > Idle/Run/Walk priority. Jumping/Falling
can remain true during airborne Dash. IsLanding is the existing Land window, not
the JustLanded pulse; IsDashing follows presentation State, including its final
partial travel frame. Do not reinterpret flags as button presses or let clip
completion determine dash, jump, landing, or movement timing.

The driver samples in LateUpdate. A future separate writer must explicitly read
after that sample; another unordered LateUpdate is insufficient. The earlier
bridge notes also allow a future optional writer in the driver, but that choice
and Animator evaluation timing must wait for the runtime gate and real assets.
Before the first sample values are defaults; disabled/inactive sources retain
stale values. Keep those existing semantics and verify eventual pose timing in
Unity rather than promising same-frame animation evaluation from static review.

## Optional Hanuman anchors (concepts only)

| Suggested role | Future parent/location |
| --- | --- |
| RightHandWeapon | Right hand bone; asset-specific grip orientation |
| LeftHand | Left hand bone |
| Head | Head bone |
| Chest | Chest or upper-spine bone |
| Back | Upper-spine/chest bone with a back-facing local offset |
| LeftFoot / RightFoot | Corresponding foot bones for future contact presentation |
| LandingOrigin | Model ground-plane origin for future central landing presentation |
| TailRoot | Actual tail base bone, or documented pelvis-relative attachment |

Names are suggestions, not mandatory lookup paths. Set local offsets/orientation
against the actual rig and expose only anchors a future consumer uses. Bone
anchors follow the pose; LandingOrigin is an optional ground-plane reference,
not a ground detector. Replacement invalidates old bone/anchor references, so a
future consumer must rebind and tolerate absence. Do not attach gameplay ownership
to these anchors. No empty anchors, weapons, equipment logic, tail simulation,
VFX, abilities, or procedural animation are created in this task.

## Asset organization proposal

Current project conventions are `Assets/Scripts`, `Assets/Scenes`, and
`Assets/Materials`, with the prototype OBJ at the Assets root. Keep those files
and their metadata in place. For future production Hanuman assets only, propose:

```text
Assets/Art/Characters/Hanuman/
    Models/
    Materials/
    Textures/
    Animations/
    Prefabs/
```

Models holds imported meshes/rigs; Prefabs holds presentation model prefabs, not
new movement implementations. Animations is reserved for later approved assets.
Create each folder in Unity when it first contains a real asset; do not add empty
directories or fake assets now. Shared prototype materials stay in their current
location. Any later relocation must use Unity and preserve .meta identity and
references; keep HanumanFace.obj together with its importer metadata. Add no
packages or external assets as part of this architectural preparation.

## Later Unity work and validation limits

1. Execute and record the existing Home Test before extending locomotion or the
   bridge. Add the existing PlayerAnimationDriver to Player for its documented
   tests; it needs no model, Animator, or Inspector reference assignment.
2. After that baseline, inspect a real asset's axes, scale, origin, rig, and
   unwanted scripts/colliders/rigidbodies before adding its visual subtree. Keep
   CharacterController as the only Player collider. Retain camera wiring.
3. Keep Visual at identity, group/disable the prototype, place CharacterModel,
   and align its feet to Player-local Y = -1. Verify capsule fit and parenting
   in the Editor. Only when animation work is authorized, configure the model's
   Animator with Apply Root Motion off and connect a driver-state consumer.
4. Run the appended future Hanuman presentation checks in the Home Test, including
   replacement/removal and no-model operation. Leave unavailable asset/Animator
   checks Not tested rather than inferring a pass from this document.

Source review establishes no visual dependency in movement/camera/driver and
no circular presentation responsibility. Serialized hierarchy and references
support this boundary, but do not prove Unity scene/prefab/import correctness.
There are no C# changes to compile from this task. Actual C# compilation, Inspector
operation, model import, animation timing, camera behavior, and runtime correctness
remain unverified. All behavior, tuning, scene data, assets, and packages are
unchanged by this documentation-only task.
