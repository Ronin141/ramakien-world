# Movement prototype

Open `Assets/Scenes/SampleScene.unity` in Unity 6000.6.3f1, let scripts import,
then press Play and click the Game view. The scene is already wired and included
in the build scene list. No packages or Inspector setup are required.

| Control | Action |
| --- | --- |
| WASD | Move relative to the camera |
| Hold Left Shift | Run |
| Space | Jump when grounded |
| Mouse | Orbit the player |
| Escape | Release the cursor and movement controls |
| Left click in Game view | Capture the cursor and resume controls |

`Scripts/ThirdPersonPlayer.cs` owns keyboard input, movement, turning, jumping,
and gravity. Its CharacterController is the player's only collider; there is
no Rigidbody. Speeds, jump height, and gravity are adjustable in the Inspector.
Diagonal input is normalized. Holding Space does not repeatedly jump.

`Scripts/ThirdPersonCamera.cs` owns mouse orbit and cursor capture. It updates
heading before movement, then follows the player in LateUpdate. Pitch is limited
to keep the camera above the flat ground. Camera obstruction handling is outside
this milestone.

The scene uses a primitive Hanuman prototype under `Player/Visual` and a
40-by-40 cube ground with a BoxCollider. The Player root keeps the existing
CharacterController and movement script; Visual contains only transforms,
primitive meshes, and renderers, with no colliders or gameplay scripts.
The white body, muzzle, ears, limbs, curved segmented tail, gold crown and waist
decoration use four shared URP Simple Lit materials in `Assets/Materials`.
The model faces local +Z and follows the Player root's existing rotation.
The existing camera, lighting, and global volume are reused without adjustments.
Keyboard and mouse are read through the installed
Input System; the template input-actions asset is not needed by this prototype.

This is a static blockout with no limb or tail animation. The crown and tail
extend beyond the unchanged gameplay capsule and do not collide independently.
Replace the entire `Visual` child with the future rigged model, preserving the
root controller, local +Z forward, and feet at local Y = -1. The four prototype
materials can then be replaced or removed when no longer referenced.

## Manual play check

1. Confirm Hanuman settles on the ground and remains stable while idle. Check
   the muzzle and ears from the front, and crown, limbs, and curled tail from behind.
2. Move in all four directions; check diagonal travel is no faster than straight travel.
3. Hold Left Shift while moving and confirm the speed increases.
4. Tap Space: confirm one jump, a fall, and a stable landing. Try Space again in
   midair to confirm there is no double jump; hold Space to confirm no auto-jump.
5. Orbit the camera, then press W: movement should follow the new camera heading.
6. Press Escape, click to recapture, and switch away from and back to the Editor
   to check cursor release and recovery.
7. Walk off the ground to check falling. Stop and restart Play to reset the scene;
   this milestone intentionally has no respawn system.
