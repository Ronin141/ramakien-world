using UnityEngine;
using UnityEngine.InputSystem;

namespace RamakienWorld
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonPlayer : MonoBehaviour
    {
        // Walk/Run/Jump/Fall represent Moving/Running/Jumping/Falling.
        // Keep the existing names and ordering for compatibility with consumers.
        public enum MovementState { Idle, Walk, Run, Jump, Fall, Land, Dash }

        [Header("Movement")]
        [SerializeField] private Transform movementCamera;
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float runSpeed = 7f;
        [SerializeField, Min(0f), Tooltip("Rotation speed in degrees per second.")]
        private float turnSpeed = 540f;
        [SerializeField, Min(0.01f)] private float acceleration = 45f;
        [SerializeField, Min(0.01f)] private float deceleration = 26f;
        [SerializeField, Min(0.01f)] private float airAcceleration = 20f;

        [Header("Jump and Ground")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField, Min(0.01f)] private float groundStickSpeed = 2f;
        [SerializeField, Min(0f)] private float landStateDuration = 0.1f;

        [Header("Dash")]
        [SerializeField] private bool dashEnabled = true;
        [SerializeField, Min(0f)] private float dashSpeed = 12f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.25f;
        [SerializeField, Min(0f), Tooltip("Recovery time after a dash ends.")]
        private float dashCooldown = 0.8f;

        [Header("Movement State")]
        [SerializeField, Min(0.001f), Tooltip("Actual horizontal speed below which the character is idle (units/sec).")]
        private float movingSpeedThreshold = 0.05f;
        [SerializeField, Min(0f), Tooltip("Speed above walk speed required for the Run state (units/sec).")]
        private float runSpeedThreshold = 0.1f;

        // Read after Update (e.g. in an Animator driver's LateUpdate).
        public MovementState State { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float HorizontalSpeed { get; private set; }
        // Actual speed normalized to the faster configured locomotion speed.
        // Dash/airborne speeds above that reference clamp to one.
        public float NormalizedMovementSpeed
        {
            get
            {
                float maxSpeed = Mathf.Max(walkSpeed, runSpeed);
                return maxSpeed > 0f ? Mathf.Clamp01(HorizontalSpeed / maxSpeed) : 0f;
            }
        }
        // Gravity accumulator, including negative ground-stick velocity at rest.
        public float VerticalVelocity => verticalSpeed;
        public float VerticalSpeed => VerticalVelocity;
        public bool IsGrounded { get; private set; }
        public bool IsMoving => HorizontalSpeed >= movingSpeedThreshold;
        // Ground locomotion state, not the Shift key; false during Land/Dash/airtime.
        public bool IsRunning => State == MovementState.Run;
        // Independent of presentation state so airborne Dash still exposes its phase.
        public bool IsJumping => !IsGrounded && verticalSpeed > 0f;
        public bool IsFalling => !IsGrounded && verticalSpeed <= 0f;
        public bool IsDashing => dashTimeRemaining > 0f;
        public bool JustLanded { get; private set; }
        public float DashCooldownRemaining => dashRecoveryRemaining;

        private CharacterController controller;
        private float verticalSpeed;
        private Vector3 horizontalVelocity;
        private Vector3 dashDirection;
        private float dashTimeRemaining;
        private float dashRecoveryRemaining;
        private float landTimeRemaining;
        private bool hasMoved;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (movementCamera == null)
            {
                Debug.LogError("Assign the movement camera on the player.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            bool running = false;
            bool jumpPressed = false;
            bool dashPressed = false;

            if (keyboard != null && Application.isFocused && Cursor.lockState == CursorLockMode.Locked)
            {
                input = new Vector2(
                    (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                    (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
                input = Vector2.ClampMagnitude(input, 1f);
                running = keyboard.leftShiftKey.isPressed;
                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
                dashPressed = keyboard.leftCtrlKey.wasPressedThisFrame;
            }

            float deltaTime = Time.deltaTime;
            JustLanded = false;
            if (deltaTime <= 0f)
                return;

            bool wasGrounded = controller.isGrounded && verticalSpeed <= 0f;
            landTimeRemaining = Mathf.Max(0f, landTimeRemaining - deltaTime);
            dashRecoveryRemaining = Mathf.Max(0f, dashRecoveryRemaining - deltaTime);

            // Use camera yaw only, so looking up/down never changes movement speed.
            Quaternion heading = Quaternion.Euler(0f, movementCamera.eulerAngles.y, 0f);
            Vector3 direction = heading * new Vector3(input.x, 0f, input.y);
            bool hasInput = direction.sqrMagnitude > 0f;
            float targetSpeed = running ? runSpeed : walkSpeed;
            // Releasing Shift or movement mid-jump must not erase takeoff momentum.
            if (!wasGrounded)
                targetSpeed = Mathf.Max(targetSpeed, horizontalVelocity.magnitude);
            Vector3 targetVelocity = direction * targetSpeed;
            if (wasGrounded || hasInput)
            {
                float rate = wasGrounded
                    ? (hasInput ? acceleration : deceleration)
                    : airAcceleration;
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity,
                    targetVelocity, rate * deltaTime);
            }

            if (dashEnabled && dashPressed && !IsDashing && dashRecoveryRemaining <= 0f)
            {
                dashDirection = hasInput ? direction.normalized : transform.forward;
                dashTimeRemaining = dashDuration;
                dashRecoveryRemaining = dashDuration + dashCooldown;
            }

            bool dashedThisFrame = IsDashing;
            Vector3 facingDirection = dashedThisFrame ? dashDirection : direction;
            if (facingDirection.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(facingDirection), turnSpeed * deltaTime);
            }

            if (wasGrounded)
            {
                // A small downward velocity keeps the controller in contact with the floor.
                verticalSpeed = -groundStickSpeed;
                if (jumpPressed)
                    verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalSpeed += gravity * deltaTime;
            // Integrate the final partial dash frame without extending the burst.
            float dashStep = Mathf.Min(dashTimeRemaining, deltaTime);
            Vector3 displacement = horizontalVelocity * (deltaTime - dashStep)
                + dashDirection * (dashSpeed * dashStep);
            dashTimeRemaining = Mathf.Max(0f, dashTimeRemaining - deltaTime);
            displacement.y = verticalSpeed * deltaTime;
            CollisionFlags collisions = controller.Move(displacement);

            if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0f)
                verticalSpeed = 0f;
            // Use this Move's contact result; do not ground a rising jump with a probe.
            IsGrounded = (collisions & CollisionFlags.Below) != 0 && verticalSpeed <= 0f;
            if (IsGrounded)
                verticalSpeed = -groundStickSpeed;

            JustLanded = hasMoved && !wasGrounded && IsGrounded;
            hasMoved = true;
            if (JustLanded)
                landTimeRemaining = landStateDuration;

            UpdateMovementState(dashedThisFrame);
        }

        private void UpdateMovementState(bool dashedThisFrame)
        {
            // Publish measured motion only after collision resolution.
            Velocity = controller.velocity;
            HorizontalSpeed = new Vector2(Velocity.x, Velocity.z).magnitude;
            State = dashedThisFrame ? MovementState.Dash
                : !IsGrounded ? (verticalSpeed > 0f ? MovementState.Jump : MovementState.Fall)
                : landTimeRemaining > 0f ? MovementState.Land
                : !IsMoving ? MovementState.Idle
                : HorizontalSpeed > walkSpeed + runSpeedThreshold ? MovementState.Run
                : MovementState.Walk;
        }

        private void OnValidate()
        {
            gravity = Mathf.Min(gravity, -0.01f);
            movingSpeedThreshold = Mathf.Max(0.001f, movingSpeedThreshold);
            runSpeedThreshold = Mathf.Max(0f, runSpeedThreshold);
        }
    }
}
