using UnityEditor;
using UnityEngine;

namespace RamakienWorld.Editor
{
    [CustomEditor(typeof(PlayerAnimationDriver))]
    public sealed class PlayerAnimationDriverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Animation (Read Only)", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect animation state after movement Update.", MessageType.Info);
                return;
            }

            var driver = (PlayerAnimationDriver)target;
            EditorGUILayout.HelpBox("Samples require an active driver and player. Otherwise values retain their last sample.", MessageType.Info);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("State", driver.State);
                EditorGUILayout.FloatField("Normalized Movement Speed", driver.NormalizedMovementSpeed);
                EditorGUILayout.Toggle("Grounded", driver.IsGrounded);
                EditorGUILayout.FloatField("Vertical Velocity", driver.VerticalVelocity);
                EditorGUILayout.Toggle("Walking", driver.IsWalking);
                EditorGUILayout.Toggle("Running", driver.IsRunning);
                EditorGUILayout.Toggle("Jumping", driver.IsJumping);
                EditorGUILayout.Toggle("Falling", driver.IsFalling);
                EditorGUILayout.Toggle("Landing", driver.IsLanding);
                EditorGUILayout.Toggle("Dashing", driver.IsDashing);
                EditorGUILayout.Toggle("Just Landed", driver.JustLanded);
            }
        }

        public override bool RequiresConstantRepaint() => Application.isPlaying;
    }
}
