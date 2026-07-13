using Systems.SimpleCore.Operations;

namespace Systems.SimpleAchievements.Operations
{
    /// <summary>
    ///     Factory methods for <see cref="OperationResult"/> values produced by the SimpleAchievements system.
    /// </summary>
    public static class AchievementOperations
    {
        /// <summary>System code identifying SimpleAchievements in operation results.</summary>
        public const ushort SYSTEM_ACHIEVEMENTS = 0x000C;

        /// <summary>Result code: achievement was successfully unlocked.</summary>
        public const ushort SUCCESS_UNLOCKED = 0x0001;

        /// <summary>Result code: achievement progress was updated but the achievement remains locked.</summary>
        public const ushort SUCCESS_PROGRESS_UPDATED = 0x0002;

        /// <summary>Result code: achievement was already unlocked.</summary>
        public const ushort ERROR_ALREADY_UNLOCKED = 0x0002;

        /// <summary>Result code: achievement reference was null or otherwise invalid.</summary>
        public const ushort ERROR_INVALID = 0x0003;

        /// <summary>Result code: achievement condition was not met.</summary>
        public const ushort ERROR_CONDITION_NOT_MET = 0x0004;

        /// <summary>Result code: achievement does not implement <see cref="Abstract.IProgressibleAchievement"/>.</summary>
        public const ushort ERROR_NOT_PROGRESSIBLE = 0x0005;

        /// <returns>Generic permitted success result.</returns>
        public static OperationResult Permitted() =>
            OperationResult.Success(SYSTEM_ACHIEVEMENTS, OperationResult.SUCCESS_PERMITTED);

        /// <returns>Success result indicating the achievement was unlocked.</returns>
        public static OperationResult Unlocked() =>
            OperationResult.Success(SYSTEM_ACHIEVEMENTS, SUCCESS_UNLOCKED);

        /// <returns>Success result indicating progress was updated without unlocking the achievement.</returns>
        public static OperationResult ProgressUpdated() =>
            OperationResult.Success(SYSTEM_ACHIEVEMENTS, SUCCESS_PROGRESS_UPDATED);

        /// <returns>Error result indicating the achievement was already unlocked.</returns>
        public static OperationResult AlreadyUnlocked() =>
            OperationResult.Error(SYSTEM_ACHIEVEMENTS, ERROR_ALREADY_UNLOCKED);

        /// <returns>Error result indicating the achievement reference is invalid.</returns>
        public static OperationResult InvalidAchievement() =>
            OperationResult.Error(SYSTEM_ACHIEVEMENTS, ERROR_INVALID);

        /// <returns>Error result indicating the achievement condition is not currently met.</returns>
        public static OperationResult ConditionNotMet() =>
            OperationResult.Error(SYSTEM_ACHIEVEMENTS, ERROR_CONDITION_NOT_MET);

        /// <returns>Error result indicating the achievement cannot receive progress notifications.</returns>
        public static OperationResult NotProgressible() =>
            OperationResult.Error(SYSTEM_ACHIEVEMENTS, ERROR_NOT_PROGRESSIBLE);
    }
}
