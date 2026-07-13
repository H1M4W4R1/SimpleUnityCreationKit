using Systems.SimpleAchievements.Data.Databases;
using Systems.SimpleCore.Automation.Attributes;
using UnityEngine;

namespace Systems.SimpleAchievements.Abstract.Platforms
{
    /// <summary>
    ///     Abstract base for external achievement platform integrations (Steam, Epic, etc.).
    ///     Subclass to implement SDK calls for a specific platform.
    /// </summary>
    /// <remarks>
    ///     Concrete subclasses inherit the <see cref="AutoCreateAttribute"/> and are automatically
    ///     registered in <see cref="AchievementPlatformDatabase"/> via the Addressables label
    ///     <see cref="AchievementPlatformDatabase.LABEL"/>.
    ///     Each concrete platform is auto-created as a single configuration asset in
    ///     <c>Assets/Generated/AchievementPlatforms/</c>.
    /// </remarks>
    [AutoCreate("AchievementPlatforms", AchievementPlatformDatabase.LABEL)]
    public abstract class AchievementPlatformBase : ScriptableObject
    {
        /// <summary>Display name for this platform, shown in the Project Settings window.</summary>
        public abstract string PlatformName { get; }

        /// <summary>
        ///     Called once at application startup (registry <c>Awake</c>).
        ///     Initialize the platform SDK here.
        /// </summary>
        public virtual void Initialise() { }

        /// <summary>
        ///     Called once when the registry is destroyed (application quit or domain reload).
        ///     Shut down the platform SDK here.
        /// </summary>
        public virtual void Shutdown() { }

        /// <summary>
        ///     Propagates an unlock notification to the external platform SDK.
        ///     Called for every achievement that transitions to the unlocked state.
        /// </summary>
        /// <param name="platformId">
        ///     The platform-specific achievement identifier matching
        ///     <see cref="AchievementData.PlatformId"/>.
        /// </param>
        public abstract void UnlockAchievement(string platformId);

#if UNITY_EDITOR
        /// <summary>
        ///     Renders this platform's configuration fields inside the
        ///     <c>Project Settings &gt; Achievements</c> window.
        /// </summary>
        /// <param name="serializedObject">
        ///     <see cref="UnityEditor.SerializedObject"/> wrapping this platform asset.
        ///     Call <c>Update()</c> before and <c>ApplyModifiedProperties()</c> after drawing.
        /// </param>
        public abstract void DrawSettings(UnityEditor.SerializedObject serializedObject);
#endif
    }
}
