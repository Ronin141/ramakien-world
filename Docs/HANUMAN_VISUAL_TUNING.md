# Hanuman visual structure audit and Home Test tuning plan

Status: **documentation only; Unity was not launched. All visual experiments and runtime checks are Not tested.** Audited 2026-10-02 from the current working tree, including existing uncommitted work. No scene, mesh, transform, material, script, prefab, camera, or gameplay changes were made by this audit.

This is a tuning plan for the existing static prototype, not a new runtime system or a claim of verified Hanuman likeness. Proposed directions are design hypotheses for this project's Ramakien-inspired character, not an assertion of historical mask accuracy. Do not add assets, objects, meshes, animation, an Animator, or a rig to execute this plan.

## Evidence and reading conventions

Sources: [SampleScene](../Assets/Scenes/SampleScene.unity), [face OBJ](../Assets/HanumanFace.obj), [mesh importer mapping](../Assets/HanumanFace.obj.meta), the four [materials](../Assets/Materials), and existing [presentation contract](HANUMAN_PRESENTATION.md), [animation bridge](PLAYER_ANIMATION_BRIDGE.md), and [Home Test](MILESTONE_2_HOME_TEST.md). Prior prototype descriptions are context, not visual evidence for this audit.

- **Serialized fact** means a current hierarchy, transform, component, or reference read from files.
- **Source-derived estimate** means arithmetic using scene transforms and source mesh vertices or standard Unity primitive dimensions. OBJ importer scale is 1, useFileScale is 0. These are not Unity-rendered bounds; actual import orientation, normals, occlusion, and appearance still require inspection.
- **Visual hypothesis** means a possible cause or improvement to confirm in Unity. No screenshots or live render were inspected for this task.
- P and S below are local position and scale `(x, y, z)`. Q is the actual serialized local quaternion `(x, y, z, w)`, **not Inspector Euler degrees**. Preserve the original values when recording a baseline; do not paste quaternion numbers into Euler fields. Tiny float residues are retained in the inventory.
- All listed grouping transforms are identity. Thus leaf positions also express Player-local coordinates at this baseline. Head, Crown, limbs, waist, and tail are sibling groups; they are not a bone chain. Head features are siblings, so moving Muzzle does not move Nose, Mouth, or Fangs. Moving Head does not move Crown.

## Complete discovered hierarchy

Paths below are relative to `Player`. Every object under Visual is listed, including containers. There is no CharacterModel child or player prefab in this scene.

```text
Player
  Visual
    Body
    Neck
    Head
      Skull
      Muzzle
      Mouth
      LowerJaw
      Nose
      LeftEar
      LeftInnerEar
      LeftEye
      LeftBrow
      RightEar
      RightInnerEar
      RightEye
      RightBrow
      LeftCheek
      LeftEyeWhite
      LeftPupil
      LeftFang
      LeftMouthRim
      RightCheek
      RightEyeWhite
      RightPupil
      RightFang
      RightMouthRim
    LeftArm
      UpperArm
      Forearm
      Hand
      ArmBand
      WristCuff
    LeftLeg
      Thigh
      Shin
      AnkleBand
      Foot
    RightArm
      UpperArm
      Forearm
      Hand
      ArmBand
      WristCuff
    RightLeg
      Thigh
      Shin
      AnkleBand
      Foot
    WaistDecoration
      RedWrap
      GoldBelt
      FrontPanel
      FrontInlay
      BackPanel
      Buckle
    Crown
      HeadBand
      TierBase
      TierMiddle
      TierUpper
      Spire
      CrestGem
      LeftFlame
      RightFlame
    Tail
      Segment01
      Segment02
      Segment03
      Segment04
      Segment05
      Segment06
      Segment07
      Segment08
      Segment09
      Segment10
      Segment11
```

Total: **76 descendants beneath Visual**, plus Visual itself; **68 MeshFilter/MeshRenderer pairs**. All listed GameObjects are active and renderers enabled in the serialized scene. Containers have Transform only; mesh objects have Transform, MeshFilter, and MeshRenderer only. No colliders, rigidbodies, Animator, or runtime scripts occur in this subtree.

### Mesh and material reference key

Built-in mesh references use GUID `0000000000000000e000000000000000`, type 0: Cube `10202`, Cylinder `10206`, Sphere `10207`, Capsule `10208`. Standard sphere/cube bounds span 1 unit per axis; capsule/cylinder bounds span 2 along local Y and 1 across X/Z before scaling. Scale is therefore not always the object's full dimension.

OBJ references use GUID `f185daf9b53e47d8af4c761066458f27`, type 3. The importer maps `4300000` Skull, `4300002` Muzzle, `4300004` Jaw, `4300006` Eye, `4300008` Ear, `4300010` Wedge, and `4300012` Point. A mesh name is not a second GameObject: e.g. Nose uses the shared Jaw mesh. Source Skull/Muzzle/Jaw are faceted ring forms; Eye is a flattened angular shape; Ear is a swept polygonal shape; Wedge and Point provide angular details. These are not spherical head/eye/ear primitives.

Every MR entry below identifies the scene MeshRenderer fileID. Each renderer has one material slot (material fileID `2100000`, type 2), resolved as follows. No material tuning is proposed.

| Key | Asset | GUID | Serialized BaseColor RGBA |
| --- | --- | --- | --- |
| White | `Assets/Materials/Hanuman_White.mat` | `36408a7380f8dcd40a45e4effbb69aec` | `(0.93, 0.95, 0.96, 1)` |
| Dark | `Assets/Materials/Hanuman_Dark.mat` | `ff8addcc69536bd4f96f4c6d04707254` | `(0.035, 0.024, 0.028, 1)` |
| Gold | `Assets/Materials/Hanuman_Gold.mat` | `2ff072231dc74904aad5a39ba28da0a6` | `(0.94, 0.57, 0.09, 1)` |
| Red | `Assets/Materials/Hanuman_Red.mat` | `b2560e5b25dd7c14d954e6721eecc50f` | `(0.65, 0.035, 0.055, 1)` |

### Conceptual groups (without changing the hierarchy)

| Group | Existing objects, relative to Visual |
| --- | --- |
| Head | `Head`, `Head/Skull` |
| Face/muzzle | `Head/Muzzle`, `Mouth`, `LowerJaw`, `Nose`, Left/Right `Cheek`, `Fang`, `MouthRim` (all under Head) |
| Eyes | Head's Left/Right `Eye`, `EyeWhite`, `Pupil`, `Brow` |
| Ears | Head's Left/Right `Ear`, `InnerEar` |
| Crown/head ornament | `Crown` and all eight of its children |
| Torso | `Body`, `Neck` |
| Arms | `LeftArm`, `RightArm`, each `UpperArm`, `Forearm` |
| Hands | `LeftArm/Hand`, `RightArm/Hand` |
| Legs | `LeftLeg`, `RightLeg`, each `Thigh`, `Shin` |
| Feet | `LeftLeg/Foot`, `RightLeg/Foot` |
| Tail | `Tail`, `Segment01` through `Segment11` beneath it |
| Accessories | `WaistDecoration` and its six children; both arms' `ArmBand`/`WristCuff`; both legs' `AnkleBand` |

### Exact local transform and renderer inventory

Every path in this table starts at `Player/`. Role labels are anatomical interpretations of the existing names and placement. A dash means no mesh/renderer, not a missing asset.

| GameObject path | Apparent part | Mesh (fileID) | P | Q | S | Renderer / material |
| --- | --- | --- | --- | --- | --- | --- |
| `Visual` | presentation boundary | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/Body` | torso/chest and abdomen | Sphere (10207) | `(0, 0.19, 0)` | `(0, 0, 0, 1)` | `(0.61, 0.77, 0.36)` | MR `1494690793` / White |
| `Visual/Neck` | neck | Capsule (10208) | `(0, 0.53, 0)` | `(0, 0, 0, 1)` | `(0.23, 0.14, 0.23)` | MR `1743258289` / White |
| `Visual/Head` | head grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/Head/Skull` | cranium | OBJ Skull (4300000) | `(0, 0.76, -0.025)` | `(0, 0, 0, 1)` | `(0.44, 0.5, 0.38)` | MR `1220633133` / White |
| `Visual/Head/Muzzle` | projecting upper muzzle | OBJ Muzzle (4300002) | `(0, 0.69, 0.27)` | `(0, 0, 0, 1)` | `(0.36, 0.18, 0.36)` | MR `94862531` / White |
| `Visual/Head/Mouth` | dark mouth opening | OBJ Jaw (4300004) | `(0, 0.606, 0.325)` | `(0, 0, 0, 1)` | `(0.28, 0.051, 0.22)` | MR `1438094782` / Dark |
| `Visual/Head/LowerJaw` | lower jaw/chin | OBJ Jaw (4300004) | `(0, 0.55, 0.24)` | `(0, 0, 0, 1)` | `(0.3, 0.085, 0.39)` | MR `1627714319` / White |
| `Visual/Head/Nose` | nose | OBJ Jaw (4300004) | `(0, 0.747, 0.439)` | `(0, 0, 0, 1)` | `(0.084, 0.045, 0.042)` | MR `1849982461` / Dark |
| `Visual/Head/LeftEar` | left outer ear | OBJ Ear (4300008) | `(-0.263, 0.774, -0.024)` | `(0, 0, 0.1391731, 0.9902681)` | `(0.125, 0.245, 0.07)` | MR `736060394` / White |
| `Visual/Head/LeftInnerEar` | left inner ear insert | OBJ Ear (4300008) | `(-0.265, 0.783, 0.015)` | `(0, 0, 0.1391731, 0.9902681)` | `(0.063, 0.143, 0.012)` | MR `1424756094` / Red |
| `Visual/Head/LeftEye` | left dark eye outline | OBJ Eye (4300006) | `(-0.112, 0.805, 0.189)` | `(0, 0, -0.1045285, 0.9945219)` | `(0.132, 0.072, 0.034)` | MR `1840008292` / Dark |
| `Visual/Head/LeftBrow` | left raised brow | OBJ Wedge (4300010) | `(-0.112, 0.85, 0.182)` | `(0, 0, -0.1045285, 0.9945219)` | `(0.153, 0.048, 0.07)` | MR `2083990831` / White |
| `Visual/Head/RightEar` | right outer ear | OBJ Ear (4300008) | `(0.263, 0.774, -0.024)` | `(0, 0, -0.1391731, 0.9902681)` | `(0.125, 0.245, 0.07)` | MR `374153251` / White |
| `Visual/Head/RightInnerEar` | right inner ear insert | OBJ Ear (4300008) | `(0.265, 0.783, 0.015)` | `(0, 0, -0.1391731, 0.9902681)` | `(0.063, 0.143, 0.012)` | MR `928109840` / Red |
| `Visual/Head/RightEye` | right dark eye outline | OBJ Eye (4300006) | `(0.112, 0.805, 0.189)` | `(0, 0, 0.1045285, 0.9945219)` | `(0.132, 0.072, 0.034)` | MR `1768885423` / Dark |
| `Visual/Head/RightBrow` | right raised brow | OBJ Wedge (4300010) | `(0.112, 0.85, 0.182)` | `(0, 0, 0.1045285, 0.9945219)` | `(0.153, 0.048, 0.07)` | MR `477024220` / White |
| `Visual/Head/LeftCheek` | left cheek plate | OBJ Skull (4300000) | `(-0.179, 0.69, 0.153)` | `(0, 0, 0.1218693, 0.9925462)` | `(0.135, 0.18, 0.145)` | MR `2100000002` / White |
| `Visual/Head/LeftEyeWhite` | left white eye interior | OBJ Eye (4300006) | `(-0.112, 0.805, 0.209)` | `(0, 0, -0.1045285, 0.9945219)` | `(0.102, 0.043, 0.012)` | MR `2100000012` / White |
| `Visual/Head/LeftPupil` | left pupil | OBJ Eye (4300006) | `(-0.104, 0.804, 0.218)` | `(0, 0, -0.1045285, 0.9945219)` | `(0.027, 0.033, 0.014)` | MR `2100000022` / Dark |
| `Visual/Head/LeftFang` | left small fang | OBJ Point (4300012) | `(-0.082, 0.604, 0.438)` | `(0, 0, 1, 6.123234e-17)` | `(0.033, 0.066, 0.035)` | MR `2100000032` / White |
| `Visual/Head/LeftMouthRim` | left mouth corner/rim | OBJ Wedge (4300010) | `(-0.144, 0.626, 0.332)` | `(0, 0, 0.1305262, 0.9914449)` | `(0.022, 0.069, 0.1)` | MR `2100000042` / Red |
| `Visual/Head/RightCheek` | right cheek plate | OBJ Skull (4300000) | `(0.179, 0.69, 0.153)` | `(0, 0, -0.1218693, 0.9925462)` | `(0.135, 0.18, 0.145)` | MR `2100000052` / White |
| `Visual/Head/RightEyeWhite` | right white eye interior | OBJ Eye (4300006) | `(0.112, 0.805, 0.209)` | `(0, 0, 0.1045285, 0.9945219)` | `(0.102, 0.043, 0.012)` | MR `2100000062` / White |
| `Visual/Head/RightPupil` | right pupil | OBJ Eye (4300006) | `(0.104, 0.804, 0.218)` | `(0, 0, 0.1045285, 0.9945219)` | `(0.027, 0.033, 0.014)` | MR `2100000072` / Dark |
| `Visual/Head/RightFang` | right small fang | OBJ Point (4300012) | `(0.082, 0.604, 0.438)` | `(0, 0, 1, 6.123234e-17)` | `(0.033, 0.066, 0.035)` | MR `2100000082` / White |
| `Visual/Head/RightMouthRim` | right mouth corner/rim | OBJ Wedge (4300010) | `(0.144, 0.626, 0.332)` | `(0, 0, -0.1305262, 0.9914449)` | `(0.022, 0.069, 0.1)` | MR `2100000092` / Red |
| `Visual/LeftArm` | left arm grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/LeftArm/UpperArm` | upper arm | Capsule (10208) | `(-0.365, 0.255, 0.0075)` | `(0.112059206, -0, 0.9711798, 0.21036312)` | `(0.16, 0.23907939, 0.16)` | MR `1967486124` / White |
| `Visual/LeftArm/Forearm` | forearm | Capsule (10208) | `(-0.445, -0.045, 0.045)` | `(0.88935447, -0, 0.44467723, 0.1063525)` | `(0.13, 0.22358751, 0.13)` | MR `1741833424` / White |
| `Visual/LeftArm/Hand` | hand placeholder | Sphere (10207) | `(-0.46, -0.255, 0.075)` | `(0, 0, 0, 1)` | `(0.15, 0.19, 0.15)` | MR `1713828484` / White |
| `Visual/LeftArm/ArmBand` | upper-arm ornament | Cylinder (10206) | `(-0.354, 0.28, 0.006)` | `(0, 0, -0.2079117, 0.9781476)` | `(0.18, 0.034, 0.18)` | MR `492086407` / Gold |
| `Visual/LeftArm/WristCuff` | wrist ornament | Cylinder (10206) | `(-0.458, -0.16, 0.067)` | `(0, 0, -0.043619387, 0.99904823)` | `(0.155, 0.046, 0.155)` | MR `1376256425` / Gold |
| `Visual/LeftLeg` | left leg grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/LeftLeg/Thigh` | upper leg | Capsule (10208) | `(-0.1675, -0.42000002, 0.0125)` | `(0.70589066, -0, 0.70589083, 0.05862153)` | `(0.205, 0.25353807, 0.205)` | MR `229561587` / White |
| `Visual/LeftLeg/Shin` | lower leg | Capsule (10208) | `(-0.185, -0.71500003, 0.0125)` | `(-0.9274822, 0, 0.3709925, 0.046274364)` | `(0.145, 0.21812367, 0.145)` | MR `1160860753` / White |
| `Visual/LeftLeg/AnkleBand` | ankle ornament | Cylinder (10206) | `(-0.19, -0.85, 0)` | `(0, 0, 0, 1)` | `(0.17, 0.032, 0.17)` | MR `1925140129` / Gold |
| `Visual/LeftLeg/Foot` | foot placeholder | Sphere (10207) | `(-0.19, -0.92, 0.075)` | `(0, 0, 0, 1)` | `(0.2, 0.16, 0.34)` | MR `1677101205` / White |
| `Visual/RightArm` | right arm grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/RightArm/UpperArm` | upper arm | Capsule (10208) | `(0.365, 0.255, 0.0075)` | `(0.112059206, 0, -0.9711798, 0.21036312)` | `(0.16, 0.23907939, 0.16)` | MR `810614060` / White |
| `Visual/RightArm/Forearm` | forearm | Capsule (10208) | `(0.445, -0.045, 0.045)` | `(0.88935447, 0, -0.44467723, 0.1063525)` | `(0.13, 0.22358751, 0.13)` | MR `1305182834` / White |
| `Visual/RightArm/Hand` | hand placeholder | Sphere (10207) | `(0.46, -0.255, 0.075)` | `(0, 0, 0, 1)` | `(0.15, 0.19, 0.15)` | MR `1059059867` / White |
| `Visual/RightArm/ArmBand` | upper-arm ornament | Cylinder (10206) | `(0.354, 0.28, 0.006)` | `(0, 0, 0.2079117, 0.9781476)` | `(0.18, 0.034, 0.18)` | MR `1467517639` / Gold |
| `Visual/RightArm/WristCuff` | wrist ornament | Cylinder (10206) | `(0.458, -0.16, 0.067)` | `(0, 0, 0.043619387, 0.99904823)` | `(0.155, 0.046, 0.155)` | MR `1361418341` / Gold |
| `Visual/RightLeg` | right leg grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/RightLeg/Thigh` | upper leg | Capsule (10208) | `(0.1675, -0.42000002, 0.0125)` | `(0.70589066, 0, -0.70589083, 0.05862153)` | `(0.205, 0.25353807, 0.205)` | MR `1655789510` / White |
| `Visual/RightLeg/Shin` | lower leg | Capsule (10208) | `(0.185, -0.71500003, 0.0125)` | `(-0.9274822, 0, -0.3709925, 0.046274364)` | `(0.145, 0.21812367, 0.145)` | MR `738544579` / White |
| `Visual/RightLeg/AnkleBand` | ankle ornament | Cylinder (10206) | `(0.19, -0.85, 0)` | `(0, 0, 0, 1)` | `(0.17, 0.032, 0.17)` | MR `1836389682` / Gold |
| `Visual/RightLeg/Foot` | foot placeholder | Sphere (10207) | `(0.19, -0.92, 0.075)` | `(0, 0, 0, 1)` | `(0.2, 0.16, 0.34)` | MR `1172726690` / White |
| `Visual/WaistDecoration` | waist ornament grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/WaistDecoration/RedWrap` | waist cloth mass | Sphere (10207) | `(0, -0.245, 0)` | `(0, 0, 0, 1)` | `(0.49, 0.29, 0.345)` | MR `1701092015` / Red |
| `Visual/WaistDecoration/GoldBelt` | waist belt | Cylinder (10206) | `(0, -0.135, 0)` | `(0, 0, 0, 1)` | `(0.52, 0.055, 0.38)` | MR `139730427` / Gold |
| `Visual/WaistDecoration/FrontPanel` | front hanging panel | Cube (10202) | `(0, -0.325, 0.19)` | `(0, 0, 0, 1)` | `(0.2, 0.3, 0.045)` | MR `72658646` / Gold |
| `Visual/WaistDecoration/FrontInlay` | front panel inset | Cube (10202) | `(0, -0.33, 0.217)` | `(0, 0, 0, 1)` | `(0.095, 0.21, 0.013)` | MR `1873659316` / Red |
| `Visual/WaistDecoration/BackPanel` | rear hanging panel | Cube (10202) | `(0, -0.34, -0.19)` | `(0, 0, 0, 1)` | `(0.21, 0.28, 0.045)` | MR `1715561367` / Gold |
| `Visual/WaistDecoration/Buckle` | belt centerpiece | Sphere (10207) | `(0, -0.13, 0.202)` | `(0, 0, 0, 1)` | `(0.12, 0.1, 0.055)` | MR `729083658` / Red |
| `Visual/Crown` | head ornament grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/Crown/HeadBand` | crown base band | Cylinder (10206) | `(0, 0.934, 0)` | `(0, 0, 0, 1)` | `(0.49, 0.042, 0.41)` | MR `1268273058` / Gold |
| `Visual/Crown/TierBase` | lower crown tier | OBJ Skull (4300000) | `(0, 1, -0.025)` | `(0, 0, 0, 1)` | `(0.34, 0.12, 0.3)` | MR `1786048916` / Gold |
| `Visual/Crown/TierMiddle` | middle crown tier | OBJ Skull (4300000) | `(0, 1.067, -0.025)` | `(0, 0, 0, 1)` | `(0.24, 0.05, 0.22)` | MR `279922819` / Gold |
| `Visual/Crown/TierUpper` | upper pointed crown tier | OBJ Point (4300012) | `(0, 1.132, -0.025)` | `(0, 0, 0, 1)` | `(0.18, 0.11, 0.16)` | MR `1674392907` / Gold |
| `Visual/Crown/Spire` | crown tip | OBJ Point (4300012) | `(0, 1.235, -0.025)` | `(0, 0, 0, 1)` | `(0.072, 0.16, 0.072)` | MR `1849290372` / Gold |
| `Visual/Crown/CrestGem` | forehead ornament | OBJ Eye (4300006) | `(0, 0.95, 0.216)` | `(0, 0, 0.7071068, 0.7071068)` | `(0.085, 0.105, 0.03)` | MR `1433791118` / Red |
| `Visual/Crown/LeftFlame` | left side crown point | OBJ Point (4300012) | `(-0.205, 1.014, 0)` | `(0, 0, 0.190809, 0.9816272)` | `(0.073, 0.21, 0.075)` | MR `1222080707` / Gold |
| `Visual/Crown/RightFlame` | right side crown point | OBJ Point (4300012) | `(0.205, 1.014, 0)` | `(0, 0, -0.190809, 0.9816272)` | `(0.073, 0.21, 0.075)` | MR `683980709` / Gold |
| `Visual/Tail` | tail grouping | — | `(0, 0, 0)` | `(0, 0, 0, 1)` | `(1, 1, 1)` | — |
| `Visual/Tail/Segment01` | tail segment 01 | Capsule (10208) | `(0.03, -0.215, -0.255)` | `(-0.72274, 0, -0.2282337, 0.65234673)` | `(0.09, 0.14574721, 0.09)` | MR `1482206437` / White |
| `Visual/Tail/Segment02` | tail segment 02 | Capsule (10208) | `(0.125, -0.24000001, -0.445)` | `(-0.6083104, 0, -0.41621232, 0.6758149)` | `(0.086500004, 0.15879221, 0.086500004)` | MR `167588309` / White |
| `Visual/Tail/Segment03` | tail segment 03 | Capsule (10208) | `(0.285, -0.225, -0.62)` | `(-0.40806535, 0, -0.4845777, 0.7737359)` | `(0.083000004, 0.16818859, 0.083000004)` | MR `842894304` / White |
| `Visual/Tail/Segment04` | tail segment 04 | Capsule (10208) | `(0.48499998, -0.14500001, -0.75)` | `(-0.23001912, 0, -0.48304, 0.8448453)` | `(0.079500005, 0.1683968, 0.079500005)` | MR `482761002` / White |
| `Visual/Tail/Segment05` | tail segment 05 | Capsule (10208) | `(0.67999995, 0, -0.82)` | `(-0.08422156, 0, -0.3789974, 0.92155725)` | `(0.076000005, 0.16684099, 0.076000005)` | MR `1265844473` / White |
| `Visual/Tail/Segment06` | tail segment 06 | Capsule (10208) | `(0.83, 0.2, -0.84)` | `(0, 0, -0.24708748, 0.9689932)` | `(0.072500005, 0.16154966, 0.072500005)` | MR `637779743` / White |
| `Visual/Tail/Segment07` | tail segment 07 | Capsule (10208) | `(0.91499996, 0.425, -0.825)` | `(0.0637077, 0, -0.106179625, 0.992304)` | `(0.069, 0.15313812, 0.069)` | MR `418825340` / White |
| `Visual/Tail/Segment08` | tail segment 08 | Capsule (10208) | `(0.91499996, 0.635, -0.78999996)` | `(0.1010831, -0, 0.12635385, 0.98682165)` | `(0.0655, 0.13299969, 0.0655)` | MR `1949592261` / White |
| `Visual/Tail/Segment09` | tail segment 09 | Capsule (10208) | `(0.82, 0.78, -0.75)` | `(0.12795353, -0, 0.44783774, 0.884912)` | `(0.062, 0.11931759, 0.062)` | MR `824936142` / White |
| `Visual/Tail/Segment10` | tail segment 10 | Capsule (10208) | `(0.675, 0.815, -0.71500003)` | `(0.15143244, -0, 0.7571613, 0.63543296)` | `(0.0585, 0.10719228, 0.0585)` | MR `835842908` / White |
| `Visual/Tail/Segment11` | tail segment 11 | Capsule (10208) | `(0.57500005, 0.74, -0.69)` | `(0.3631604, -0, 0.90790206, 0.20935236)` | `(0.055, 0.09326473, 0.055)` | MR `874186419` / White |

## Major proportions and likely silhouette problems

These are ranked candidates to inspect, not a diagnosis from a rendered image.

| Area | Serialized fact / source-derived estimate | Visual hypothesis requiring Unity confirmation |
| --- | --- | --- |
| Muzzle dominance | Muzzle S is `(0.36, 0.18, 0.36)` versus Skull `(0.44, 0.5, 0.38)`. Source-derived bounds are about `0.359 × 0.160 × 0.360` versus `0.439 × 0.500 × 0.379`. Maximum muzzle width is about 82% of skull width. However, the muzzle tapers: its front ring is only about 0.220 wide. | A broad base and long projection may dominate the facial planes, even though this is already a tapered polygonal muzzle. Do not describe it as a verified oversized round sphere. |
| Profile | Skull front is approximately Z = 0.165; Muzzle spans Z = 0.09 to 0.45. Its front is roughly 0.285 beyond the skull's front. Nose center is Z = 0.439. | Projection may read as a long generic animal snout. A depth reduction may help more than width reduction; too much reduction could erase the monkey identity. |
| Jaw and mouth | LowerJaw uses a separate faceted Jaw mesh, S `(0.3, 0.085, 0.39)`. Its source-derived height is about 0.079. Mouth height is about 0.047. Both end at Z = 0.435, behind the muzzle front at 0.45. Fangs are only 0.066 high; mouth rims are 0.022 wide. | The jaw, mouth opening, rims, and fangs may merge or become hidden by the muzzle and viewing pitch. They exist; lack of visible definition is a hypothesis, not missing geometry. |
| Skull and cheeks | Skull width/height is about 0.88, with faceted rings rather than a sphere. Cheeks are separate Skull-mesh instances at X = ±0.179, Y = 0.69, with S `(0.135, 0.18, 0.145)` and opposite 14-degree Z rotations. | Shared White material and overlapping volumes may make the assembly read as one round mass despite the facets. The cheeks may need clearer exposure, not extra objects. |
| Eyes and brows | Dark eye S `(0.132, 0.072, 0.034)`; source Eye Y extent is 0.76, so unrotated eye-outline height is about 0.055. Centers are 0.224 apart. Eyes/brows rotate outward by opposite 12-degree Z angles. White interiors and smaller pupils are separate siblings. | These are already narrow/angular, not giant circular eyes. Outlines, inward pupil offsets (0.008 relative to each eye center), or lost brows might still create a cute/cartoon expression. Inspect before shrinking anything. |
| Ears | Ear S `(0.125, 0.245, 0.07)`, swept Ear mesh, centers X = ±0.263; opposite 16-degree Z rotations. Raw mesh height before rotation is about 0.240, almost half Skull height. Red inner inserts exist. | Ears may overemphasize the monkey outline or resemble decorative discs at distance, but they are not circular primitives. Inspect front and three-quarter views. |
| Crown | Eight existing children: band, three tiers, spire, gem, two flames. Tier widths descend from S.x 0.34 to 0.24 to 0.18, then spire 0.072. Source-derived spire top is Y = 1.315, versus Skull top 1.01. | Ornamentation is already substantial. Thin tiers/side points may merge, hide behind the head, or read as a generic crown. A taller crown alone does not establish Hanuman identity. |
| Torso | Body is a sphere scaled `(0.61, 0.77, 0.36)`, centered Y = 0.19. Its width is about 1.39 times Skull width. There is no separately adjustable chest or abdomen object. | The ellipsoid body may look soft or toy-like, but the data does not prove an oversized torso. A small width change is feasible; a new tapered torso shape is outside this transform-only plan. |
| Limbs / hands | UpperArm capsule axial length is about 0.478 with diameter 0.16; Forearm about 0.447 with diameter 0.13. Thigh about 0.507 with diameter 0.205; Shin about 0.436 with diameter 0.145, before rotation. Hands are spheres scaled `(0.15, 0.19, 0.15)`. | Uniform rounded segments and mitten-like hands may contribute to a toy silhouette. The dimensions do not establish that the limbs are excessively short or thick. Do not automatically lengthen all limbs. |
| Feet | S `(0.2, 0.16, 0.34)`, P.y = -0.92, identity Q. Standard sphere bounds give bottom Y = `-0.92 - 0.16 / 2 = -1`. | Elongated round feet may read as slippers. Any future horizontal refinement must preserve the established sole height and ankle attachment. Ground contact is still unverified. |
| Tail | Eleven sibling capsules form a taper, diameter 0.09 to 0.055. Segment centers reach X ≈ 0.915 and Z = -0.84; the curl rises to center Y = 0.815. | The large one-sided curl may be a useful recognition cue, or may dominate the rear silhouette. Capsule joins may look beaded. There is no evidence it lacks a curl or needs additional segments. |
| Accessories | Gold crown, belt, arm/wrist/ankle bands and front/back panels; red wrap, inlay, buckle, crest and facial inserts already exist. No separate chest ornament object exists. | Small ornaments may disappear at the normal camera distance. This is a visibility problem to check first, not evidence that ornamentation is absent. Do not add a chest ornament in this task. |

The most defensible first candidate is the balance between projecting muzzle and existing eye/jaw/cheek features. Rounded body primitives are secondary candidates. Final likeness, whether the design evokes a particular cartoon, and which cue dominates cannot be established from this data alone.

## Prioritized tuning plan

All paths below are relative to `Player/Visual`. Left/Right names mean both actual objects listed in the inventory; they are not new groups. Current P/Q/S are fully recorded above; abbreviated baselines below identify the relevant variables. Directions are conditional experiments, not approved final values. No universal percentage or target proportion is justified without seeing the baseline.

Reversion rule: leaf transform changes are independent of gameplay, but connected visual parts may need to be restored together. Record each touched object's original P/Q/S before an experiment. “Group revert” means restore those values together, without reverting unrelated successful experiments. Do not scale or move grouping transforms to shortcut an adjustment: their pivots are at the Player origin, not anatomical joints.

### P0 — Identity

| Target GameObject(s) | Current transform / proportion | Proposed direction and reason | Inspect afterward | Independent reversion / risk |
| --- | --- | --- | --- | --- |
| `Head/Muzzle`, attachment correction on `Head/Nose` only as needed | Muzzle P `(0, 0.69, 0.27)`, S `(0.36, 0.18, 0.36)`; Nose P.z 0.439; width and projection above | First reduce muzzle depth, then test width separately. Try vertical placement only if the eye-to-mouth relationship remains crowded. Expose facial planes while retaining a recognizable projecting muzzle. | Side projection, front width, three-quarter taper; nose must stay seated, mouth/fangs must still connect. Avoid a flattened human face. | Revert muzzle and any nose correction together. Visual attachment risk; no gameplay effect. Other facial siblings do not follow automatically. |
| `Head/LeftEye`, `RightEye`, `LeftEyeWhite`, `RightEyeWhite`, `LeftPupil`, `RightPupil`; matching `LeftBrow`, `RightBrow` for spacing only | Outline S `(0.132, 0.072, 0.034)`; white S `(0.102, 0.043, 0.012)`; pupil S `(0.027, 0.033, 0.014)`; eye centers X ±0.112 | If eyes still dominate, reduce outline/interior/pupil in-plane size as one mirrored group, keeping centers and angles. Then test spacing independently; slightly inward is a candidate only if the eyes look too widely separated. Keep the existing angular shape. | Expression and readable pupils at gameplay distance; no crossed-eye effect, hidden whites, or outlines swallowing the interiors. Compare baseline if eyes are already suitably narrow. | Group revert for eye layers; include brows when spacing changes. Do not change depth offsets while testing size. |
| `Head/LeftBrow`, `RightBrow` | P.y 0.85, P.z 0.182; S `(0.153, 0.048, 0.07)` | If the brow silhouette is lost, test a little more forward exposure at fixed size/angle. This may clarify the existing angular upper face. | Front three-quarter separation from skull and eyes; no floating brow, heavy angry bar, or accidental eye occlusion. | Mirrored pair safely reverts independently if spacing is unchanged. |
| `Head/LowerJaw`, `LeftCheek`, `RightCheek` | Jaw P `(0, 0.55, 0.24)`, S `(0.3, 0.085, 0.39)`; cheeks P `(±0.179, 0.69, 0.153)` | Test jaw height/definition first; separately test slightly more forward cheek exposure if cheeks disappear into the skull. Seek a distinct lower face and cheek planes using existing meshes. | Jaw must meet muzzle/mouth, not become a chin slab. Cheeks must look connected and angular, not added round cheek puffs. Check from side as well as front. | Jaw alone can revert; cheek pair separately. Any mouth attachment correction must be recorded/restored with the change that required it. |
| `Head/Mouth`, `LeftFang`, `RightFang`, `LeftMouthRim`, `RightMouthRim` | Mouth S.y 0.051, P.y 0.606; fangs P `(±0.082, 0.604, 0.438)`, S.y 0.066; rims S.x 0.022 | Once jaw/muzzle work, expose the existing mouth opening if obscured; then separately test fang visibility. Preserve a readable open-mouth/fang cue already intended by this prototype; do not add teeth or change materials. | Dark opening survives gameplay distance without turning into a broad cartoon smile; fangs remain attached and do not become tusks. | Revert each experiment's attachment group. Do not use floating fangs to compensate for a hidden mouth. |
| `Crown/Spire`, then only if needed `TierUpper`, `TierMiddle`, `TierBase`, `LeftFlame`, `RightFlame` in separate experiments | Spire P.y 1.235, S.y 0.16; tiers P.y 1 / 1.067 / 1.132, S.y 0.12 / 0.05 / 0.11; flames S.y 0.21 | Preserve the existing pointed taper. First test spire prominence only if the top silhouette is lost; then tier separation or side-point exposure separately. Keep the ornament subordinate to a readable face. | Front/rear taper, side attachment, no disconnected floating tiers; recognizable overall silhouette at distance rather than a larger generic crown. | Spire can revert alone with its attachment correction. Restore a tier/point experiment as a group. Crown extends beyond the controller already; never enlarge collision to match. |

### P1 — Proportion

| Target GameObject(s) | Current transform / proportion | Proposed direction and reason | Inspect afterward | Independent reversion / risk |
| --- | --- | --- | --- | --- |
| `Head/Skull` | P `(0, 0.76, -0.025)`, S `(0.44, 0.5, 0.38)`; width/height ≈ 0.88 | Only if the head still reads as a round blob: test narrower X first; test taller Y separately if needed. Keep the other axis and position fixed for each comparison. Improve distinction between cranium, cheeks, and muzzle. | Eye/cheek seating, ear roots, neck and crown overlap. A taller skull can swallow the crown base; a narrower one can leave eyes floating. | Skull change reverts alone only before dependent attachments are adjusted; otherwise group revert. Do not scale Head or Player. |
| `Head/LeftEar`, `RightEar`, `LeftInnerEar`, `RightInnerEar` | Outer S `(0.125, 0.245, 0.07)`, centers X ±0.263; inner S `(0.063, 0.143, 0.012)` | If ears dominate, reduce outer/inner in-plane size as a paired group. Later test moving inward toward skull contact, retaining the swept angle. Reduce the generic wide-eared silhouette while keeping ears readable. | Inner inserts remain seated, ear roots meet skull, ears do not disappear behind crown or cheeks. | Outer/inner mirrored group revert; no independent single insert edit as a proportion experiment. |
| `Body` | P.y 0.19, S `(0.61, 0.77, 0.36)` | If torso looks balloon-like after face tuning, test less X width at fixed Y/height first. Do not assume the character needs a longer torso. Seek a clearer standing warrior silhouette with the existing ellipsoid. | Shoulder/neck connections, arm clearance, belt fit, balance against head/legs from front and side. | Body alone can revert unless belt/attachments were subsequently refit. Do not move Visual to fix resulting connections. |
| `LeftArm/UpperArm`, `Forearm`; `RightArm/UpperArm`, `Forearm`; associated bands/cuffs only for fit | Upper/forearm diameter 0.16 / 0.13, axial length ≈ 0.478 / 0.447 | Only if rounded thickness is a problem, test slightly slimmer X/Z on one mirrored segment pair at a time; hold lengths/centers/rotations. This can reduce the padded-limb look without inventing a rig. | Elbow/shoulder continuity and cuff contact; hands must still meet forearms. A slim arm must not appear fragile beside the torso. | Restore segment pair and any refitted ornaments together. Axes are segment-local; never infer their length from scene Y scale alone. |
| `LeftLeg/Thigh`, `Shin`; `RightLeg/Thigh`, `Shin` | Diameters 0.205 / 0.145; axial lengths ≈ 0.507 / 0.436 | If legs look heavy, test thickness on one mirrored segment pair at a time, preserving length and all centers initially. Keep the feet fixed. Lengthening is deferred unless a visual baseline establishes a need. | Hip/knee/ankle joins, body balance, foot contact while moving. | Pair/group revert. Length or position changes could violate sole alignment and require coordinated refitting; do not scale the leg container. |
| `LeftArm/Hand`, `RightArm/Hand`; separately `LeftLeg/Foot`, `RightLeg/Foot` | Hand S `(0.15, 0.19, 0.15)`; Foot S `(0.2, 0.16, 0.34)`, P.y -0.92 | If mitten/slipper forms dominate, test reduced hand width/depth as one pair; separately test reduced foot depth, holding foot height and Y fixed. Keep these secondary to facial identity. | Wrist/ankle contact, visible toes/feet at distance, silhouette while turning; no floating or sinking sole. | Each mirrored pair safely reverts independently with any attachment corrections. Foot Y/height/rotation changes are a floor-alignment risk; not needed for the depth trial. |
| `Tail/Segment01`–`Segment11` | Identity Tail container; 11 independently placed capsules; reach and taper recorded above | Preserve the curl. Only if it dominates, defer to a dedicated experiment reducing outer arc reach while retaining a connected base and tapered tip. This is a coordinated segment-placement task, not a first face-tuning step. | Rear and both side views: readable separated curl, no gaps/beads, no intersections with legs/waist, no ground clipping while turning. | Restore the entire edited segment set together. Do not scale Tail about the Player origin: it can detach the base. Never add tail collision or compensate with camera changes. |

### P2 — Polish

| Target GameObject(s) | Current transform / proportion | Proposed direction and reason | Inspect afterward | Independent reversion / risk |
| --- | --- | --- | --- | --- |
| `Head/Nose`; separately Left/Right `Pupil`, `MouthRim` under Head | Nose S `(0.084, 0.045, 0.042)`; pupils X ±0.104 versus eye centers ±0.112; rim S `(0.022, 0.069, 0.1)` | After identity is stable, reduce nose emphasis only if it remains a dark button; separately compare pupils closer to their own eye centers if the inward gaze is distracting; refine rim exposure only if mouth edges are lost. Small refinements should support the larger face planes. | No floating nose, wall-eyed/crossed pupils, hidden rims or excessive smile outline. Judge at normal distance before close-up perfection. | Nose independently; pupils/rims as mirrored pairs. Preserve feature attachment and material assignments. |
| `Crown/HeadBand`, `CrestGem` | Band P.y 0.934, S `(0.49, 0.042, 0.41)`; gem P `(0, 0.95, 0.216)`, S `(0.085, 0.105, 0.03)` | Refit band only if prior skull changes create gaps; then test gem exposure separately if hidden. Keep the crown visually attached without making the gem the focal point. | Side fit, no band clipping/hovering, gem stays on the front and does not obscure brows. | Band and gem trials separately reversible; dependent on chosen skull dimensions. |
| `WaistDecoration/RedWrap`, `GoldBelt`, `Buckle`, `FrontPanel`, `FrontInlay`, `BackPanel`; both arms' `ArmBand`, `WristCuff`; both legs' `AnkleBand` | Wrap S `(0.49, 0.29, 0.345)`; belt `(0.52, 0.055, 0.38)`; front/back panels `(0.2, 0.3, 0.045)` / `(0.21, 0.28, 0.045)`; remaining P/Q/S in inventory | Refit one ornament group only where prior proportions cause gaps. If identity is already clear, expose an existing obscured band/panel slightly before considering enlargement. Retain the red/gold costume structure. | Bands hug limbs; panels/inlay do not intersect or hover; costume remains legible from front/rear and normal gameplay distance. | Each attachment group reverts independently of others, relative to the accepted body/limb baseline. Never change shared materials to fix a fit problem. |

## Reducing the generic cartoon / Paul Frank look

Treat this phrase as the user's target to move away from, not a visually verified description. The strongest existing suspects are Muzzle's broad base/projection, the contrast between the small jaw and large muzzle, cheek/brow planes merging into Skull, and ear prominence. Spherical Body/Hand/Foot primitives may reinforce the impression. Narrow faceted eyes, swept ears, fangs, and crown already counter it; making all of them smaller could make the identity worse.

### Ordered experiment sequence

Start only after the runtime/driver/isolation gates in the Home Test. Capture the unchanged character from all five views first. Skip a trial if the baseline does not show its suspected problem, recording “not needed” as a later Home Test observation rather than a pass now.

Use one design variable (or one clearly coupled pair) per trial. A mirrored pair is one design variable. Eye/ear layers may need several sibling transforms edited coherently, but do not also change size, position, angle, and depth together. Use the same views and zoom for A/B comparison. Keep a change only if the improvement survives front three-quarter and normal gameplay distance; otherwise restore the immediately preceding accepted baseline.

1. **Muzzle depth, then width in a separate comparison.** Reduce `Head/Muzzle` S.z with P/Q and S.x/y fixed. Preserve Nose's seating with a recorded Z correction only if necessary. Source front Z is `Muzzle.P.z + 0.5 * Muzzle.S.z`, so retaining the current nose-to-front offset implies `Nose.P.z += 0.5 * change in Muzzle.S.z`; inspect before accepting this source-derived correction. After accepting/reverting depth, test only Muzzle S.x. Do not make width and depth changes in a single A/B trial. Inspect muzzle/mouth/fang attachment from the side; stop if coherence cannot be preserved with the small change.
2. **Muzzle vertical placement.** Only if crowding remains, test a slightly lower `Muzzle` P.y at fixed size; move `Nose` by the same Y delta if needed to retain its seating. Mouth, jaw, and fangs are siblings, so do not assume they follow. Reject a detached result; any necessary mouth-attachment correction is a separate recorded fit substep before judging likeness. Do not lower the entire Head.
3. **Eye size.** If eyes dominate, reduce the in-plane size of the two outlined eye assemblies (`Left/RightEye`, `EyeWhite`, `Pupil`) coherently; preserve their centers, Z layering, angle, and the accepted muzzle. Do not reduce the brows in this trial. Keep the narrow existing shape and readable whites. Revert if the smaller eyes disappear at distance.
4. **Eye spacing.** At accepted size, test a small inward shift only if spacing appears excessive. Translate each eye's outline, white, pupil, and matching brow by the same mirrored X delta. Preserve each pupil's offset within its eye; pupil gaze is a later polish trial. Compare against unchanged spacing before keeping it.
5. **Brow exposure.** If the brow planes are not visible, test only the two brows' forward Z placement. Preserve size, angle and eye spacing. Stop before they float off the skull.
6. **Skull aspect ratio: width first, height second only if needed.** Test `Skull` S.x narrower with S.y/z and center fixed. Accept/revert, then test S.y taller as a distinct trial if roundness remains. Do not scale `Head`. Check crown, ears, eyes, cheeks and neck before accepting either; treat any attachment repair as a recorded dependent fit change, not another likeness experiment.
7. **Ear size, then location separately.** Reduce outer/inner ear in-plane size coherently if dominant. After accepting/reverting, test inward X placement of outer/inner pairs if needed for skull contact. Retain rotations; do not test size and spacing simultaneously. The goal is smaller swept ears, not absent ears.
8. **Crown silhouette.** Start with `Crown/Spire` height only if the point is lost. Preserve its base plane with `change in P.y = 0.5 * change in S.y` (Point source bottom Y = -0.5). Then, only if needed, test tier separation or mirrored flame exposure in separate comparisons. Do not scale Crown or adjust all eight pieces together. A clearer crown supplements the face; it cannot substitute for it.
9. **Jaw definition, then cheek exposure separately.** First test `LowerJaw` S.y at fixed center to make the lower plane clearer, inspecting the mouth seam. Accept/revert; then test the cheek pair's forward P.z if they are swallowed by Skull/Muzzle. Preserve cheek width/angles. Avoid making the entire lower face wider.
10. **Mouth opening, then fangs separately.** After the preceding proportions settle, expose `Mouth` if it is hidden (first test forward placement at fixed size). Accept/revert; then test only fang visibility, with the mirrored pair remaining attached to the mouth region. Do not also enlarge the red rims. Keep a restrained open-mouth cue rather than a smile line or oversized tusks.
11. **Stop and choose the best face before body or polish work.** Compare the accepted face to the original five views at unchanged gameplay framing. Record all winning local transforms and any dependent fit corrections. Only then consider the conditional P1 torso/limb/tail trials or P2 details; there is no requirement to complete every trial tonight.

These directions are starting hypotheses. If the actual baseline contradicts one, retain it and record why; do not force every transformation. Small step sizes should be chosen in the Inspector against visible contact and silhouette, not copied from arbitrary numerical targets.

## Practical silhouette checkpoints

Use Scene view navigation for front, three-quarter, side, and rear inspection. Do not move/rotate the Player or edit Main Camera to obtain these views. For gameplay framing, use the existing Game view and ordinary orbit controls without changing camera settings. Keep comparison framing consistent.

| View | What to inspect | Recognition checkpoint for this prototype |
| --- | --- | --- |
| Front | Muzzle versus cheek width, eyes/brows, distinct lower jaw/opening, ear spread, crown taper, feet and costume balance | A white angular monkey face with readable mouth/fangs and head ornament; muzzle and ears should not overwhelm all other features. Gold/red waist structure should remain visible. |
| Front 3/4 | Whether cheek and brow planes separate; muzzle taper/projection; nose, jaw and fangs remain attached; far eye is not lost | Face should have several readable planes and a clear lower jaw rather than one smooth muzzle blob. Crown should sit on the head. This is the main face-selection view. |
| Side (both sides if possible) | Snout depth against cranium, mouth/jaw separation, crown seating, neck, foot sole and tail attachment | A projecting but controlled muzzle, connected jaw/neck and pointed head ornament. No detached nose or fang, floating tier, or foot under the ground. |
| Rear | Crown taper, shoulder/waist/leg balance, back panel and tail curve; inspect both sides of the curl | Crown, white body, costume and a distinct attached curled tail must carry the identity when the face is hidden. Tail should not swallow the body outline. |
| Normal gameplay distance | Read the unzoomed Game view at its actual resolution while idle, turning and moving; camera distance is serialized as 5, targetHeight 0.5 | The combined head/crown, white body, red/gold waist, and curled tail should remain distinct. If a face edit helps only in close-up, defer it. Check actual face/ornament visibility; do not enlarge everything to compensate. |

Do not require tiny fangs or pupils to remain individually resolved at every distance. The combined silhouette and major facial contrast should work first. No view is currently visually verified.

## Gameplay and presentation constraints

- **Player root stays unchanged:** current scene-root P `(0, 1.1, 0)`, Q `(0, 0, 0, 1)`, S `(1, 1, 1)`. Ordinary movement in Play Mode still moves/rotates it; do not edit its transform, pivot, gameplay scripts or movement configuration to accommodate the art.
- **CharacterController stays unchanged:** height 2, radius 0.5, center `(0, 0, 0)`, slope limit 45, step offset 0.3, skin width 0.05, minimum move distance 0. Keep it the only Player collider. Add no visual colliders or rigidbodies.
- **Movement keeps rotation authority.** Keep Player-local +Z forward, +Y up. Do not redirect Player rotation through a head, body, visual or camera adjustment; stationary dash still uses the gameplay root's forward direction.
- **Camera stays unchanged:** Main Camera targets Player, and Player's movementCamera references Main Camera. Preserve wiring, settings and behavior. Scene view navigation is for inspection only. Do not change camera framing to hide a proportion problem.
- **Visual remains the boundary:** preserve its name, parenting, identity P/Q and unit S. Tune existing leaf objects beneath it only. Do not create CharacterModel, another container, an Animator or a runtime system for this session.
- **Feet stay at Player-local Y ≈ -1:** existing unrotated Foot centers Y = -0.92 and height 0.16 already imply sole Y = -1. Horizontal foot depth/width trials leave that baseline intact. For any later height change, the unrotated relation is `Foot.P.y = -1 + Foot.S.y / 2`; this does not apply unchanged after rotating the foot or changing parent transforms. Verify visible ground contact in Unity instead of moving Player or changing the capsule.
- **Visual extents do not define collision:** crown and tail already extend beyond the capsule. Do not resize the controller to enclose them. Larger body/limb/foot changes can increase visible clipping or apparent floating even while collision remains unchanged; reject/refine the visual change, then rerun movement verification.

Highest constraint risks: scaling Head/Crown/limb/Tail containers about the root; moving Visual; changing leg length, foot height/rotation or attachment positions; using Player or camera offsets to compensate. These are not shortcuts in the plan. Tail/limb refitting is less independently reversible than a leaf facial experiment. All visual edits must remain free of gameplay effects.

## Tonight's setup and recording

Follow the appended [Home Test visual workflow](MILESTONE_2_HOME_TEST.md#static-hanuman-visual-tuning-after-runtime-gates). The existing driver setup is part of that earlier runtime gate: the driver exists in source but is not serialized on Player in this scene. This documentation adds no setup, component or wiring now. Do not use the visual tuning step to add another runtime system.

For tonight's isolation gate, verify the existing static Visual follows Player and can be temporarily disabled/restored without changing collision, movement, camera, or driver samples. Confirm identity Visual transform, +Z alignment, and sole baseline. The earlier future-model/Animator tests remain Not tested until that separate integration exists; do not introduce a model or Animator just to clear them. Only proceed after the applicable current-static-presentation checks pass.

Use temporary Play Mode visual trials, or the previously documented temporary scene copy, during the later Home Test. Record winning values before exiting Play Mode; changes there normally revert. Do not overwrite the source SampleScene merely to preserve an experiment. Persisting accepted art changes is later work.

Suggested compact record (one row per changed object so undo is reliable):

| Trial / target path | Before P / Inspector rotation / S | Candidate P / Inspector rotation / S | Views / screenshot reference | Keep or revert / reason | Movement recheck |
| --- | --- | --- | --- | --- | --- |
| Not tested — no trials run | — | — | — | — | Not tested |

Record the chosen face baseline separately from later body changes. If runtime verification fails, stop visual tuning and record the failure; do not retune movement to make an art experiment pass.

## What remains unknown without Unity

Actual OBJ import/submesh resolution, rendered material/shading behavior, face and crown overlap, contact/seams, facet readability, perceived expression, silhouette/likeness, and screen-size readability remain unverified. The scene data cannot tell whether the result actually evokes the generic cartoon/Paul Frank look, nor which proposed change will improve it. No exact winning transform values can be selected here. Ground contact during play, runtime/driver correctness, camera behavior and presentation isolation still need their existing Home Test gates. Static measurements are evidence for selecting experiments, not visual approval.
