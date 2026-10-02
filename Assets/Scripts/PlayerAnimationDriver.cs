using UnityEngine;

namespace RamakienWorld
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ThirdPersonPlayer))]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        public ThirdPersonPlayer.MovementState State { get; private set; }
        public float NormalizedMovementSpeed { get; private set; }
        public bool IsGrounded { get; private set; }
        public float VerticalVelocity { get; private set; }
        public bool IsWalking { get; private set; }
        public bool IsRunning { get; private set; }
        // Vertical phase remains available during airborne Dash.
        public bool IsJumping { get; private set; }
        public bool IsFalling { get; private set; }
        public bool IsLanding { get; private set; }
        public bool IsDashing { get; private set; }
        public bool JustLanded { get; private set; }

        private ThirdPersonPlayer player;

        private void Awake()
        {
            player = GetComponent<ThirdPersonPlayer>();
        }

        private void LateUpdate()
        {
            // Movement publishes its sample in Update. Inactive sources retain
            // the previous sample; no Animator or animation assets are needed.
            if (player == null || !player.isActiveAndEnabled)
                return;

            State = player.State;
            NormalizedMovementSpeed = player.NormalizedMovementSpeed;
            IsGrounded = player.IsGrounded;
            VerticalVelocity = player.VerticalVelocity;
            IsWalking = State == ThirdPersonPlayer.MovementState.Walk;
            IsRunning = player.IsRunning;
            IsJumping = player.IsJumping;
            IsFalling = player.IsFalling;
            IsLanding = State == ThirdPersonPlayer.MovementState.Land;
            // Presentation includes the final partial dash frame even when the
            // movement controller's remaining-time IsDashing is already false.
            IsDashing = State == ThirdPersonPlayer.MovementState.Dash;
            JustLanded = player.JustLanded;
        }
    }
}
