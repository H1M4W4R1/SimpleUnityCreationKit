using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Components;
using Systems.SimpleAchievements.Data.SaveFiles;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Saving.Abstract;
using Systems.SimpleCore.Saving.Utility;
using UnityEngine;

namespace Systems.SimpleAchievements.Utility
{
    /// <summary>
    ///     Static facade for all achievement operations.
    ///     Use this class to unlock achievements, query unlock state, and manage persistence.
    /// </summary>
    public static class AchievementAPI
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() { }

        // ------------------------------------------------------------------ //
        //  Unlock                                                              //
        // ------------------------------------------------------------------ //

        /// <summary>
        ///     Attempts to unlock an achievement.
        ///     For non-conditional achievements this is the primary unlock path.
        ///     For conditional achievements, prefer letting the registry poll automatically;
        ///     use <see cref="AchievementUnlockContext.ForceUnlock"/> to bypass the guard if needed.
        /// </summary>
        /// <param name="context">Unlock context identifying the target achievement.</param>
        /// <returns>
        ///     <see cref="AchievementOperations.Unlocked"/> on success,
        ///     <see cref="AchievementOperations.AlreadyUnlocked"/> if already unlocked, or
        ///     <see cref="AchievementOperations.InvalidAchievement"/> if the context is invalid.
        /// </returns>
        public static OperationResult Unlock(in AchievementUnlockContext context)
            => AchievementRegistry.Instance.Unlock(in context);

        // ------------------------------------------------------------------ //
        //  Query                                                               //
        // ------------------------------------------------------------------ //

        /// <summary>
        ///     Returns <c>true</c> if the given achievement has been unlocked.
        /// </summary>
        public static bool IsUnlocked([CanBeNull] AchievementData achievement)
        {
            if (ReferenceEquals(achievement, null)) return false;
            return AchievementRegistry.Instance.IsUnlocked(achievement.PlatformId);
        }

        /// <summary>
        ///     Returns <c>true</c> if the achievement with the given platform ID has been unlocked.
        /// </summary>
        public static bool IsUnlocked([CanBeNull] string platformId)
            => AchievementRegistry.Instance.IsUnlocked(platformId);

        // ------------------------------------------------------------------ //
        //  Automatic disk persistence                                          //
        //  Writes to Application.persistentDataPath/<saveFileName> using JSON. //
        // ------------------------------------------------------------------ //

        /// <summary>
        ///     Writes current unlock state to <c>Application.persistentDataPath/&lt;saveFileName&gt;</c>
        ///     as JSON. Called automatically on each unlock when
        ///     <see cref="Data.Settings.AchievementsSettings.AutoSaveOnUnlock"/> is <c>true</c>.
        /// </summary>
        public static void Save() => AchievementRegistry.Instance.PersistToDisk();

        /// <summary>
        ///     Reads and restores unlock state from <c>Application.persistentDataPath/&lt;saveFileName&gt;</c>.
        ///     Called automatically during registry <c>Awake</c>; call again to force a reload.
        /// </summary>
        public static void Load() => AchievementRegistry.Instance.LoadFromDisk();

        // ------------------------------------------------------------------ //
        //  Host save-system integration (optional)                             //
        //  Use when embedding achievement data inside a larger save file.      //
        // ------------------------------------------------------------------ //

        /// <summary>
        ///     Serializes current unlock state into an in-memory <see cref="AchievementSaveFile"/>
        ///     via <see cref="SaveAPI"/>. The caller is responsible for persisting the returned
        ///     object to disk as part of a larger save.
        /// </summary>
        /// <returns>In-memory save file, or <c>null</c> if serialization failed.</returns>
        [CanBeNull] public static SaveFileBase SaveToMemory()
            => SaveAPI.Save(AchievementRegistry.Instance);

        /// <summary>
        ///     Restores unlock state from a <see cref="SaveFileBase"/> previously produced by
        ///     <see cref="SaveToMemory"/>. Use when loading from a host-managed save system.
        /// </summary>
        /// <param name="saveFile">Save file to restore from.</param>
        public static void Load([NotNull] SaveFileBase saveFile)
            => SaveAPI.Load(AchievementRegistry.Instance, saveFile);
    }
}
