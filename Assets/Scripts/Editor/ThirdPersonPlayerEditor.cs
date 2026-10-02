using UnityEditor;
using UnityEngine;

namespace RamakienWorld.Editor
{
    [CustomEditor(typeof(ThirdPersonPlayer))]
    public sealed class ThirdPersonPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Movement (Read Only)", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect movement after the player's Update.", MessageType.Info);
                return;
            }

            var player = (ThirdPersonPlayer)target;
            if (!player.isActiveAndEnabled)
                EditorGUILayout.HelpBox("The player is inactive or disabled; values are not updating.", MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("State", player.State);
                EditorGUILayout.FloatField("Horizontal Speed", player.HorizontalSpeed);
                EditorGUILayout.FloatField("Normalized Movement Speed", player.NormalizedMovementSpeed);
                EditorGUILayout.FloatField("Vertical Velocity", player.VerticalVelocity);
                EditorGUILayout.Toggle("Grounded", player.IsGrounded);
                EditorGUILayout.Toggle("Moving", player.IsMoving);
                EditorGUILayout.Toggle("Running", player.IsRunning);
                EditorGUILayout.Toggle("Jumping", player.IsJumping);
                EditorGUILayout.Toggle("Falling", player.IsFalling);
                EditorGUILayout.Toggle("Just Landed", player.JustLanded);
                EditorGUILayout.Toggle("Dashing", player.IsDashing);
                EditorGUILayout.FloatField("Dash Cooldown Remaining", player.DashCooldownRemaining);
            }
        }

        public override bool RequiresConstantRepaint() => Application.isPlaying;
    }
}
