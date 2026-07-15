using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Abstract.Features;
using UnityEngine;

namespace Systems.SimpleIntegration.Examples.Platforms
{
    /// <summary>Mock Steam integration that exposes the achievement platform contract.</summary>
    public sealed class SteamPlatform : IntegratedPlatformBase, IAchievementPlatform
    {
        /// <inheritdoc />
        public override string PlatformName => "Steam (Steamworks)";

        /// <inheritdoc />
        public override void Initialize()
        {
            // SteamAPI.Init() - ensure this succeeds before Steam API calls.
            base.Initialize();
        }

        /// <inheritdoc />
        public void UnlockAchievement(string achievementId)
        {
            // SteamUserStats.SetAchievement(achievementId);
            // SteamUserStats.StoreStats();
            Debug.Log($"[Steam] Unlock achievement: {achievementId} (mocked)");
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            // SteamAPI.Shutdown();
            base.Shutdown();
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                "Configure your Steamworks App ID here.\n" +
                "Requires Steamworks.NET or Facepunch.Steamworks to be imported.",
                UnityEditor.MessageType.Info);
        }
#endif
    }
}
