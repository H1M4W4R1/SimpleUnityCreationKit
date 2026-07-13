using Systems.SimpleAchievements.Abstract.Platforms;
using UnityEngine;

namespace Systems.SimpleAchievements.Examples.Platforms
{
    /// <summary>
    ///     Mocked Steam platform integration.
    ///     Replace the TODO bodies with Steamworks.NET or Facepunch.Steamworks SDK calls once
    ///     the relevant package is imported.
    /// </summary>
    [CreateAssetMenu(
        menuName = "SimpleAchievements/Platforms/Steam",
        fileName = "SteamAchievementPlatform")]
    public sealed class SteamAchievementPlatform : AchievementPlatformBase
    {
        /// <inheritdoc />
        public override string PlatformName => "Steam (Steamworks)";

        /// <inheritdoc />
        public override void Initialise()
        {
            // TODO: SteamAPI.Init() - ensure called before any other Steam call.
        }

        /// <inheritdoc />
        public override void UnlockAchievement(string platformId)
        {
            // TODO: SteamUserStats.SetAchievement(platformId);
            //       SteamUserStats.StoreStats();
            Debug.Log($"[Steam] Unlock achievement: {platformId} (mocked)");
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            // TODO: SteamAPI.Shutdown()
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                "Configure your Steamworks App ID here.\n" +
                "Requires Steamworks.NET or Facepunch.Steamworks to be imported.",
                UnityEditor.MessageType.Info);

            // TODO: Add [SerializeField] fields for App ID and other Steam configuration,
            //       then draw them here using EditorGUILayout.PropertyField.
        }
#endif
    }
}
