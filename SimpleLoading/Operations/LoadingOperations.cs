using Systems.SimpleCore.Operations;

namespace Systems.SimpleLoading.Operations
{
    /// <summary>Operation results returned by SimpleLoading.</summary>
    public static class LoadingOperations
    {
        public const ushort SYSTEM_LOADING = 0x0020;

        public const ushort SUCCESS_STARTED = 1;
        public const ushort SUCCESS_COMPLETED = 2;
        public const ushort SUCCESS_CANCELLED = 3;
        public const ushort SUCCESS_RELEASED = 4;
        public const ushort SUCCESS_WORLD_PART_LOADED = 5;
        public const ushort SUCCESS_WORLD_PART_UNLOADED = 6;

        public const ushort ERROR_SEQUENCE_IS_NULL = 1;
        public const ushort ERROR_STAGE_MISSING = 2;
        public const ushort ERROR_STAGE_OPERATION_MISSING = 3;
        public const ushort ERROR_HANDLE_INVALID = 4;
        public const ushort ERROR_HANDLE_NOT_RUNNING = 5;
        public const ushort ERROR_WORLD_PART_TARGET_MISSING = 6;
        public const ushort ERROR_WORLD_PART_ROOT_MISSING = 7;
        public const ushort ERROR_WORLD_PART_DISTANCE_INVALID = 8;
        public const ushort ERROR_ADDRESSABLE_DATABASE_MISSING = 9;
        public const ushort ERROR_ADDRESSABLE_DATABASE_LOADING_FAILED = 10;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_LOADING, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Started() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_STARTED);
        public static OperationResult Completed() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_COMPLETED);
        public static OperationResult Cancelled() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_CANCELLED);
        public static OperationResult Released() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_RELEASED);
        public static OperationResult WorldPartLoaded() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_WORLD_PART_LOADED);
        public static OperationResult WorldPartUnloaded() => OperationResult.Success(SYSTEM_LOADING, SUCCESS_WORLD_PART_UNLOADED);
        public static OperationResult SequenceIsNull() => OperationResult.Error(SYSTEM_LOADING, ERROR_SEQUENCE_IS_NULL);
        public static OperationResult StageMissing() => OperationResult.Error(SYSTEM_LOADING, ERROR_STAGE_MISSING);
        public static OperationResult StageOperationMissing() => OperationResult.Error(SYSTEM_LOADING, ERROR_STAGE_OPERATION_MISSING);
        public static OperationResult HandleInvalid() => OperationResult.Error(SYSTEM_LOADING, ERROR_HANDLE_INVALID);
        public static OperationResult HandleNotRunning() => OperationResult.Error(SYSTEM_LOADING, ERROR_HANDLE_NOT_RUNNING);
        public static OperationResult WorldPartTargetMissing() => OperationResult.Error(SYSTEM_LOADING, ERROR_WORLD_PART_TARGET_MISSING);
        public static OperationResult WorldPartRootMissing() => OperationResult.Error(SYSTEM_LOADING, ERROR_WORLD_PART_ROOT_MISSING);
        public static OperationResult WorldPartDistanceInvalid() => OperationResult.Error(SYSTEM_LOADING, ERROR_WORLD_PART_DISTANCE_INVALID);
        public static OperationResult AddressableDatabaseMissing() => OperationResult.Error(SYSTEM_LOADING, ERROR_ADDRESSABLE_DATABASE_MISSING);
        public static OperationResult AddressableDatabaseLoadingFailed() => OperationResult.Error(SYSTEM_LOADING, ERROR_ADDRESSABLE_DATABASE_LOADING_FAILED);
    }
}
