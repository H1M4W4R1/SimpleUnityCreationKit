using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Abstract.Features;
using UnityEngine;

namespace Systems.SimpleIntegration.Examples.Platforms
{
    /// <summary>Mock Epic Games Store integration that exposes the achievement platform contract.</summary>
    public sealed class EpicPlatform : IntegratedPlatformBase, IAchievementPlatform
    {
        /// <inheritdoc />
        public override string PlatformName => "Epic Games Store";

        /// <inheritdoc />
        public override void Initialize()
        {
            // Initialize the EOS SDK platform handle with the project credentials.
            base.Initialize();
        }

        /// <inheritdoc />
        public void UnlockAchievement(string achievementId)
        {
            // EOS_Achievements_UnlockAchievements with the local user product ID.
            Debug.Log($"[Epic] Unlock achievement: {achievementId} (mocked)");
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            // EOS_Platform_Release();
            base.Shutdown();
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                "Configure your Epic Product ID, Sandbox ID and Client ID here.\n" +
                "Requires the Epic Online Services SDK package to be imported.",
                UnityEditor.MessageType.Info);
        }
#endif
    }
}
