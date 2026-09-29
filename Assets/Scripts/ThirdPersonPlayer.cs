using UnityEngine;
using UnityEngine.InputSystem;

namespace RamakienWorld
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonPlayer : MonoBehaviour
    {
        [SerializeField] private Transform movementCamera;
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float runSpeed = 7f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private float verticalSpeed;

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

            if (keyboard != null && Application.isFocused && Cursor.lockState == CursorLockMode.Locked)
            {
                input = new Vector2(
                    (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                    (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
                input = Vector2.ClampMagnitude(input, 1f);
                running = keyboard.leftShiftKey.isPressed;
                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }

            // Use camera yaw only, so looking up/down never changes movement speed.
            Quaternion heading = Quaternion.Euler(0f, movementCamera.eulerAngles.y, 0f);
            Vector3 direction = heading * new Vector3(input.x, 0f, input.y);
            if (direction.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalSpeed <= 0f)
            {
                // A small downward velocity keeps the controller in contact with the floor.
                verticalSpeed = -2f;
                if (jumpPressed)
                    verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalSpeed += gravity * Time.deltaTime;
            Vector3 velocity = direction * (running ? runSpeed : walkSpeed);
            velocity.y = verticalSpeed;
            CollisionFlags collisions = controller.Move(velocity * Time.deltaTime);

            if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0f)
                verticalSpeed = 0f;
            if ((collisions & CollisionFlags.Below) != 0 && verticalSpeed < 0f)
                verticalSpeed = -2f;
        }

        private void OnValidate()
        {
            gravity = Mathf.Min(gravity, -0.01f);
        }
    }
}
