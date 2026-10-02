using UnityEngine;

namespace RamakienWorld.Character
{
    // Reads the completed movement sample; owns only assigned visual transforms.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ThirdPersonPlayer))]
    public sealed class ProceduralLocomotionAnimator : MonoBehaviour
    {
        [Header("Limb Roots")]
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;

        [Header("Optional Visuals")]
        [SerializeField, Tooltip("Presentation container to bob, never the Player root.")]
        private Transform visualRoot;
        [SerializeField] private Transform tail;

        [Header("Local Swing Axes")]
        [SerializeField, Tooltip("Axis in the limb's cached idle local frame. SampleScene limb roots use +X.")]
        private Vector3 leftArmSwingAxis = Vector3.right;
        [SerializeField] private Vector3 rightArmSwingAxis = Vector3.right;
        [SerializeField] private Vector3 leftLegSwingAxis = Vector3.right;
        [SerializeField] private Vector3 rightLegSwingAxis = Vector3.right;

        [Header("Gait (Degrees)")]
        [SerializeField, Min(0f)] private float walkLegSwing = 25f;
        [SerializeField, Min(0f)] private float runLegSwing = 40f;
        [SerializeField, Min(0f)] private float walkArmSwing = 20f;
        [SerializeField, Min(0f)] private float runArmSwing = 35f;

        [Header("Timing and Speed Calibration")]
        [SerializeField, Min(0f), Tooltip("Radians per second at the walk reference speed.")]
        private float walkFrequency = 6f;
        [SerializeField, Min(0f), Tooltip("Radians per second at the run reference speed.")]
        private float runFrequency = 10.5f;
        [SerializeField, Min(0.01f), Tooltip("Visual calibration only; does not change movement speed.")]
        private float walkReferenceSpeed = 4f;
        [SerializeField, Min(0.01f)] private float runReferenceSpeed = 7f;
        [SerializeField, Min(0f)] private float movingSpeedThreshold = 0.05f;
        [SerializeField, Min(0.01f), Tooltip("Exponential blend rate per second for all poses.")]
        private float animationSmoothness = 16f;

        [Header("Body Bob")]
        [SerializeField] private bool bodyBobEnabled = true;
        [SerializeField, Range(0f, 0.05f)] private float bodyBobAmount = 0.03f;

        [Header("Airborne Pose (Degrees)")]
        [SerializeField] private float jumpArmAngle = 20f;
        [SerializeField] private float jumpLegAngle = -15f;

        [Header("Tail")]
        [SerializeField] private bool tailSwayEnabled = true;
        [SerializeField, Range(0f, 10f)] private float tailSwayAmount = 2f;

        private ThirdPersonPlayer player;
        private LimbPose leftArmPose;
        private LimbPose rightArmPose;
        private LimbPose leftLegPose;
        private LimbPose rightLegPose;
        private LimbPose tailPose;
        private Transform bobTarget;
        private Vector3 idleVisualPosition;
        private float phase;

        private void Awake()
        {
            player = GetComponent<ThirdPersonPlayer>();
            leftArmPose = CacheLimb(leftArm, "Left Arm");
            rightArmPose = CacheLimb(rightArm, "Right Arm", leftArmPose);
            leftLegPose = CacheLimb(leftLeg, "Left Leg", leftArmPose, rightArmPose);
            rightLegPose = CacheLimb(rightLeg, "Right Leg", leftArmPose, rightArmPose, leftLegPose);
            if (tail != null)
                tailPose = CacheLimb(tail, "Tail", leftArmPose, rightArmPose, leftLegPose, rightLegPose);

            if (visualRoot != null && IsVisualChild(visualRoot, "Visual Root"))
            {
                // Bob may contain limbs, but must not be a limb or below one.
                if (IsInsideLimb(visualRoot, leftArmPose) || IsInsideLimb(visualRoot, rightArmPose)
                    || IsInsideLimb(visualRoot, leftLegPose) || IsInsideLimb(visualRoot, rightLegPose)
                    || IsInsideLimb(visualRoot, tailPose))
                    Debug.LogWarning("ProceduralLocomotionAnimator: Visual Root must be a separate presentation container; bob skipped.", this);
                else
                {
                    bobTarget = visualRoot;
                    idleVisualPosition = bobTarget.localPosition;
                }
            }
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
                return;

            bool hasSample = player != null && player.isActiveAndEnabled;
            bool airborne = hasSample && !player.IsGrounded;
            // This property ignores vertical velocity and is measured after collision resolution.
            float speed = hasSample ? player.HorizontalSpeed : 0f;
            float walkSpeed = Mathf.Max(0.01f, walkReferenceSpeed);
            float runSpeed = Mathf.Max(walkSpeed + 0.01f, runReferenceSpeed);
            float locomotion = !airborne && speed > movingSpeedThreshold
                ? Mathf.Clamp01(speed / walkSpeed) : 0f;
            float runBlend = Mathf.InverseLerp(walkSpeed, runSpeed, speed);

            // Pause the gait in the air and at rest; resume the same phase on landing.
            if (locomotion > 0f)
            {
                float frequency = Mathf.Lerp(walkFrequency, runFrequency, runBlend) * locomotion;
                phase = Mathf.Repeat(phase + frequency * deltaTime, Mathf.PI * 2f);
            }

            float cycle = Mathf.Sin(phase) * locomotion;
            float legAngle = cycle * Mathf.Lerp(walkLegSwing, runLegSwing, runBlend);
            float armAngle = cycle * Mathf.Lerp(walkArmSwing, runArmSwing, runBlend);
            float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, animationSmoothness) * deltaTime);

            // Opposite arm/leg pairs share a phase. All angles are offsets from idle.
            leftLegPose?.Apply(airborne ? jumpLegAngle : legAngle, leftLegSwingAxis, blend);
            rightLegPose?.Apply(airborne ? jumpLegAngle : -legAngle, rightLegSwingAxis, blend);
            leftArmPose?.Apply(airborne ? jumpArmAngle : -armAngle, leftArmSwingAxis, blend);
            rightArmPose?.Apply(airborne ? jumpArmAngle : armAngle, rightArmSwingAxis, blend);

            if (bobTarget != null)
            {
                // Two small upward bobs per cycle, never below the authored idle height.
                float bob = bodyBobEnabled
                    ? (1f - Mathf.Cos(phase * 2f)) * 0.5f * bodyBobAmount * locomotion : 0f;
                bobTarget.localPosition = Vector3.Lerp(bobTarget.localPosition,
                    idleVisualPosition + Vector3.up * bob, blend);
            }

            tailPose?.Apply(tailSwayEnabled ? cycle * tailSwayAmount : 0f, Vector3.up, blend);
        }

        private void OnDisable()
        {
            // Re-enabling never recaches an animated pose, so offsets cannot accumulate.
            leftArmPose?.Restore();
            rightArmPose?.Restore();
            leftLegPose?.Restore();
            rightLegPose?.Restore();
            tailPose?.Restore();
            if (bobTarget != null)
                bobTarget.localPosition = idleVisualPosition;
            phase = 0f;
        }

        private LimbPose CacheLimb(Transform limb, string label, params LimbPose[] existing)
        {
            if (limb == null)
            {
                Debug.LogWarning($"ProceduralLocomotionAnimator: assign {label} on {name}; that limb will be skipped.", this);
                return null;
            }

            if (!IsVisualChild(limb, label))
                return null;

            foreach (LimbPose pose in existing)
            {
                if (pose != null && (limb.IsChildOf(pose.Target) || pose.Target.IsChildOf(limb)))
                {
                    Debug.LogWarning($"ProceduralLocomotionAnimator: {label} overlaps another animated limb; skipped. Assign separate limb roots.", this);
                    return null;
                }
            }

            return new LimbPose(limb);
        }

        private bool IsVisualChild(Transform candidate, string label)
        {
            if (candidate != transform && candidate.IsChildOf(transform))
                return true;

            Debug.LogWarning($"ProceduralLocomotionAnimator: {label} must be below this Player, never the Player root or an unrelated object; skipped.", this);
            return false;
        }

        private static bool IsInsideLimb(Transform candidate, LimbPose pose)
        {
            return pose != null && candidate.IsChildOf(pose.Target);
        }

        private sealed class LimbPose
        {
            public Transform Target { get; }
            private readonly Quaternion idleRotation;

            public LimbPose(Transform target)
            {
                Target = target;
                idleRotation = target.localRotation;
            }

            public void Apply(float angle, Vector3 axis, float blend)
            {
                if (Target == null)
                    return;

                Vector3 localAxis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.right;
                Quaternion targetRotation = idleRotation * Quaternion.AngleAxis(angle, localAxis);
                Target.localRotation = Quaternion.Slerp(Target.localRotation, targetRotation, blend);
            }

            public void Restore()
            {
                if (Target != null)
                    Target.localRotation = idleRotation;
            }
        }
    }
}
