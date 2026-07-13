using Systems.SimpleAchievements.Abstract.Platforms;
using UnityEngine;

namespace Systems.SimpleAchievements.Examples.Platforms
{
    /// <summary>
    ///     Mocked Epic Games Store platform integration.
    ///     Replace the bodies with Epic Online Services (EOS) SDK calls once
    ///     the relevant package is imported.
    /// </summary>
    public sealed class EpicAchievementPlatform : AchievementPlatformBase
    {
        /// <inheritdoc />
        public override string PlatformName => "Epic Games Store";

        /// <inheritdoc />
        public override void Initialise()
        {
            // Initialize EOS SDK platform handle.
            // OS_Platform_Create with your product credentials.
        }

        /// <inheritdoc />
        public override void UnlockAchievement(string platformId)
        {
            // EOS_Achievements_UnlockAchievements with the local user product ID.
            Debug.Log($"[Epic] Unlock achievement: {platformId} (mocked)");
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            // EOS_Platform_Release - release the platform handle before exit.
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                "Configure your Epic Product ID, Sandbox ID and Client ID here.\n" +
                "Requires the Epic Online Services SDK package to be imported.",
                UnityEditor.MessageType.Info);

            // Add [SerializeField] fields for Product ID, Sandbox ID, Client ID, etc.,
            //       then draw them here using EditorGUILayout.PropertyField.
        }
#endif
    }
}
