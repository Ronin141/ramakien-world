using UnityEngine;
using UnityEngine.InputSystem;

namespace RamakienWorld
{
    // Update yaw before player movement; follow the resulting position in LateUpdate.
    [DefaultExecutionOrder(-100)]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float targetHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float distance = 5f;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.15f;
        [SerializeField, Range(10f, 70f)] private float pitch = 25f;

        private float yaw;

        private void OnEnable()
        {
            if (target == null)
            {
                Debug.LogError("Assign the player target on the third-person camera.", this);
                enabled = false;
                return;
            }

            yaw = transform.eulerAngles.y;
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (!Application.isFocused)
                return;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    SetCursorLocked(true);
                return;
            }

            if (mouse != null)
            {
                // Mouse delta already represents movement this frame; do not scale by deltaTime.
                Vector2 look = mouse.delta.ReadValue() * mouseSensitivity;
                yaw = Mathf.Repeat(yaw + look.x, 360f);
                pitch = Mathf.Clamp(pitch - look.y, 10f, 70f);
            }

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void LateUpdate()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * targetHeight;
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                SetCursorLocked(false);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
