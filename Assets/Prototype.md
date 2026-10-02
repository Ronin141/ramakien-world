# Movement prototype

Open `Assets/Scenes/SampleScene.unity` in Unity 6000.6.3f1, let scripts import,
then press Play and click the Game view. The scene is already wired and included
in the build scene list. No packages or Inspector setup are required.

| Control | Action |
| --- | --- |
| WASD | Move relative to the camera |
| Hold Left Shift | Run |
| Space | Jump when grounded |
| Left Ctrl | Dash in the input direction, or forward when stationary |
| Mouse | Orbit the player |
| Escape | Release the cursor and movement controls |
| Left click in Game view | Capture the cursor and resume controls |

`Scripts/ThirdPersonPlayer.cs` owns keyboard input, movement, turning, jumping,
and gravity. Its CharacterController is the player's only collider; there is
no Rigidbody. Diagonal input is normalized. Holding Space does not repeatedly
jump; airborne presses are ignored. A single controller Move combines horizontal
travel and gravity. Ground state comes from that Move's collision flags, with a
small downward contact speed on landing. There is no extra ground probe that
could incorrectly re-ground an ascending jump.

The Player Inspector is configured as follows:

| Setting | Value |
| --- | --- |
| Walk / run speed | 4 / 7 units/sec |
| Turn speed | 540 degrees/sec (previously 720) |
| Acceleration / stopping deceleration | 45 / 26 units/sec squared |
| Air acceleration | 20 units/sec squared |
| Jump height / gravity | 1.5 units / -20 units/sec squared |
| Ground stick speed | 2 units/sec downward |
| Land state duration | 0.1 seconds; does not lock movement or jumping |
| Dash enabled / speed / duration | Yes / 12 units/sec / 0.25 seconds |
| Dash cooldown | 0.8 seconds after the burst ends |

Acceleration takes approximately 0.09 seconds to reach walking speed and 0.16
seconds to reach running speed. Releasing input stops a grounded run in about
0.27 seconds. These are configuration-derived estimates, not Play Mode timings.
Airborne steering is responsive but releasing movement or Shift preserves
horizontal momentum until landing. Dash direction is fixed at activation;
gravity and ceiling/floor collision handling continue during the burst. Holding
Ctrl does not auto-repeat, and early presses are ignored rather than buffered.
Dash is available on the ground and in the air, with the same cooldown. It does
not grant invulnerability or interact with combat.

For a future Animator driver, read `State`, `Velocity`, `HorizontalSpeed`,
`VerticalSpeed`, `IsGrounded`, `IsDashing`, `JustLanded`, and
`DashCooldownRemaining` in LateUpdate. States are Idle, Walk, Run, Jump, Fall,
Land, and Dash. `JustLanded` is a one-update pulse; Land is a short presentation
state. Dash takes state priority, while `IsGrounded` still reports contact.
`Velocity` is actual controller motion; `VerticalSpeed` is the gravity accumulator
and includes ground stick. Cooldown remaining includes any active dash time.
No Animator or animation assets have been added.

Milestone 2 also exposes `NormalizedMovementSpeed`, `VerticalVelocity` (with
`VerticalSpeed` retained as an alias), `IsMoving`, `IsRunning`, `IsJumping`, and
`IsFalling`. Select Player in Play Mode to see the read-only Runtime Movement
panel. State thresholds are configurable; their defaults preserve the previous
classification. See [the home checklist](../Docs/MILESTONE_2_HOME_TEST.md) for
exact parameter semantics and pending checks, and
[office progress](../Docs/OFFICE_PROGRESS.md) for this session's changes.

`Scripts/ThirdPersonCamera.cs` owns mouse orbit and cursor capture. It updates
heading before movement, then follows the player in LateUpdate. Pitch is limited
to keep the camera above the flat ground. Camera obstruction handling is outside
this milestone.

The scene uses a primitive Hanuman prototype under `Player/Visual` and a
40-by-40 cube ground with a BoxCollider. The Player root keeps the existing
CharacterController and movement script; Visual contains only transforms,
meshes, and renderers, with no colliders or gameplay scripts.
The body, limbs, curved segmented tail and waist decoration remain primitives.
The head uses original static low-poly meshes in `Assets/HanumanFace.obj`:
seven shared shapes totaling 240 source triangles. Its importer metadata maps
the named meshes to the scene's mesh references; keep the OBJ and its .meta
together. There is no runtime geometry generation or additional asset package.
All parts use the same four shared URP Simple Lit materials in `Assets/Materials`.

The face has a faceted cranium, projecting tapered muzzle, angular cheek plates,
separate lower jaw and dark mouth opening, two small fangs, a smaller nose,
narrow outlined eyes with white interiors and pupils, raised brows, and swept
ears. The existing gold head ornament has a compact faceted tier stack and
pointed crest. This is a stylized game interpretation, not an accurate Khon mask
reproduction. The white face and open mouth draw on the
[Fine Arts Department's description of Hanuman masks](https://www.finearts.go.th/storage/contents/detail_file/PaT4Bu9nBZAsZDXKz7LU0Ce4Oc5eA67XXlpn0a8b.pdf).
The model faces local +Z and follows the Player root's existing rotation.
The existing camera, lighting, and global volume are reused without adjustments.
The CharacterController remains height 2, radius 0.5, slope limit 45 degrees,
step offset 0.3, skin width 0.05, minimum move distance 0, and center (0, 0, 0).
There is no player prefab; all player settings are on the scene's Player object.
Keyboard and mouse are read through the installed
Input System; the template input-actions asset is not needed by this prototype.

This is a static blockout with no limb or tail animation. The crown and tail
extend beyond the unchanged gameplay capsule and do not collide independently.
Replace the entire `Visual` child with the future rigged model, preserving the
root controller, local +Z forward, and feet at local Y = -1. The four prototype
materials can then be replaced or removed when no longer referenced.

## Validation status for this iteration

Unity Play Mode was **not run**. The computer-use native connection failed after
recovery attempts, and no running Unity process or standard Editor installation
was found in the working environment. Compilation, OBJ import, Console status,
physical contact behavior, and the in-game appearance are therefore unverified.
Static scene-reference/hierarchy checks and an offline three-view geometry
preview were performed; the preview is not a Unity rendering or Play Mode test.

After Unity imports the project, open SampleScene and inspect the face's mesh
references before pressing Play. No manual component wiring should be needed.
Check the following in the Editor before accepting the iteration:

## Manual play check

1. Confirm Hanuman settles on the ground and remains stable while idle. Check
   the muzzle and ears from the front, and crown, limbs, and curled tail from behind.
2. Move in all four directions; check diagonal travel is no faster than straight
   travel, turning is smooth, starts are quick, and stopping has no long slide.
3. Hold Left Shift while moving and confirm speed increases from 4 to 7 units/sec
   after acceleration. Measure root displacement over a steady-speed interval.
4. Tap Space: confirm one jump, a fall, and a stable landing. Try Space again in
   midair to confirm there is no double jump; hold Space to confirm no auto-jump.
5. Orbit the camera, then press W: movement should follow the new camera heading.
   Run and jump, then release Shift and WASD in midair: horizontal travel should
   continue. Check the fall, landing, grounded state, and idle position for jitter.
6. Press Escape, click to recapture, and switch away from and back to the Editor
   to check cursor release and recovery.
7. Walk off the ground to check falling. Stop and restart Play to reset the scene;
   this milestone intentionally has no respawn system.
8. Tap Left Ctrl while moving, stationary, and airborne. Confirm a short burst,
   forward fallback when stationary, ongoing gravity, normal movement afterward,
   and no repeated dash from holding Ctrl or pressing it during recovery.
9. Review the face front-on and in profile at gameplay distance: muzzle taper,
   cheek/jaw silhouette, sharp eyes, mouth separation, fangs, swept ears and
   pointed crest should read clearly. This visual judgment still needs in-game
   review, particularly at the eventual mobile resolution.
10. Confirm no new Console errors and that camera orbit, pitch limits, follow,
    and cursor capture behave as before.

Next iteration: perform the above Play Mode checks and tune from observed feel.
Terrain slope/step stress tests and mobile draw-call profiling remain pending;
the static head still uses separate renderers and is not a rigged production mesh.
