using Systems.SimpleCore.Operations;

namespace Systems.SimpleProgression.Operations
{
    /// <summary>
    ///     Operation results returned by SimpleProgression.
    /// </summary>
    public static class ProgressionOperations
    {
        public const ushort SYSTEM_PROGRESSION = 0x0013;

        public const ushort SUCCESS_EXPERIENCE_ADDED = 1;
        public const ushort SUCCESS_EXPERIENCE_TAKEN = 2;
        public const ushort SUCCESS_LEVEL_INCREASED = 3;

        public const ushort ERROR_INVALID_GAME_OBJECT = 1;
        public const ushort ERROR_EXPERIENCE_CONTROLLER_NOT_FOUND = 2;
        public const ushort ERROR_LEVEL_CONTROLLER_NOT_FOUND = 3;
        public const ushort ERROR_INVALID_EXPERIENCE_AMOUNT = 4;
        public const ushort ERROR_NOT_ENOUGH_EXPERIENCE = 5;
        public const ushort ERROR_EXPERIENCE_OVERFLOW = 6;
        public const ushort ERROR_INVALID_LEVEL_AMOUNT = 7;
        public const ushort ERROR_MAX_LEVEL_REACHED = 8;
        public const ushort ERROR_INVALID_LEVEL_CURVE = 9;

        public static OperationResult ExperienceAdded()
            => OperationResult.Success(SYSTEM_PROGRESSION, SUCCESS_EXPERIENCE_ADDED);

        public static OperationResult ExperienceTaken()
            => OperationResult.Success(SYSTEM_PROGRESSION, SUCCESS_EXPERIENCE_TAKEN);

        public static OperationResult LevelIncreased()
            => OperationResult.Success(SYSTEM_PROGRESSION, SUCCESS_LEVEL_INCREASED);

        public static OperationResult InvalidGameObject()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_INVALID_GAME_OBJECT);

        public static OperationResult ExperienceControllerNotFound()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_EXPERIENCE_CONTROLLER_NOT_FOUND);

        public static OperationResult LevelControllerNotFound()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_LEVEL_CONTROLLER_NOT_FOUND);

        public static OperationResult InvalidExperienceAmount()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_INVALID_EXPERIENCE_AMOUNT);

        public static OperationResult NotEnoughExperience()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_NOT_ENOUGH_EXPERIENCE);

        public static OperationResult ExperienceOverflow()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_EXPERIENCE_OVERFLOW);

        public static OperationResult InvalidLevelAmount()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_INVALID_LEVEL_AMOUNT);

        public static OperationResult MaxLevelReached()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_MAX_LEVEL_REACHED);

        public static OperationResult InvalidLevelCurve()
            => OperationResult.Error(SYSTEM_PROGRESSION, ERROR_INVALID_LEVEL_CURVE);
    }
}
