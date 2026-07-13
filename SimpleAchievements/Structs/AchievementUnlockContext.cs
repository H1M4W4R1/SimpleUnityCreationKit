using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Utility;

namespace Systems.SimpleAchievements.Structs
{
    /// <summary>
    ///     Context passed to <see cref="AchievementAPI.Unlock"/> to identify and configure an unlock request.
    /// </summary>
    public readonly ref struct AchievementUnlockContext
    {
        /// <summary>
        ///     Achievement to unlock.
        /// </summary>
        [CanBeNull] public readonly AchievementData Achievement;

        /// <summary>
        ///     When <c>true</c>, bypasses the <see cref="AchievementData.IsConditional"/> guard and
        ///     unlocks a conditional achievement immediately regardless of whether its condition is met.
        ///     Non-conditional achievements do not require this flag.
        /// </summary>
        public readonly bool ForceUnlock;

        /// <param name="achievement">Achievement to unlock.</param>
        /// <param name="forceUnlock">Bypass conditional guard if <c>true</c>.</param>
        public AchievementUnlockContext([CanBeNull] AchievementData achievement, bool forceUnlock = false)
        {
            Achievement = achievement;
            ForceUnlock = forceUnlock;
        }
    }
}
