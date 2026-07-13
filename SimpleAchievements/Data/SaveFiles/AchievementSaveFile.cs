using System;
using JetBrains.Annotations;
using Systems.SimpleCore.Saving.Abstract;

namespace Systems.SimpleAchievements.Data.SaveFiles
{
    /// <summary>
    ///     Serializable snapshot of all unlocked achievement platform IDs.
    /// </summary>
    [Serializable]
    public sealed class AchievementSaveFile : SaveFileBase
    {
        /// <summary>
        ///     Platform IDs of all unlocked achievements at the time this file was built.
        ///     Each entry must match the <see cref="Abstract.AchievementData.PlatformId"/> of the achievement.
        /// </summary>
        [NotNull] public string[] UnlockedPlatformIds = Array.Empty<string>();
    }
}
