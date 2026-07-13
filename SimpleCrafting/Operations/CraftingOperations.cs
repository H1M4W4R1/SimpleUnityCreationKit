using Systems.SimpleCore.Operations;

namespace Systems.SimpleCrafting.Operations
{
    public static class CraftingOperations
    {
        public const ushort SYSTEM_CRAFTING = 0x000D;

        public const ushort SUCCESS_STARTED = 0x0001;
        public const ushort SUCCESS_COMPLETED = 0x0002;
        public const ushort SUCCESS_CANCELLED = 0x0003;
        public const ushort SUCCESS_READY = 0x0004;
        public const ushort SUCCESS_PROGRESS_UPDATED = 0x0005;

        public const ushort ERROR_RECIPE_IS_NULL = 0x0001;
        public const ushort ERROR_STATION_MISSING = 0x0002;
        public const ushort ERROR_REFUND_FAILED = 0x0003;
        public const ushort ERROR_INSTANCE_IS_NULL = 0x0004;
        public const ushort ERROR_INSTANCE_NOT_READY = 0x0005;
        public const ushort ERROR_INSTANCE_ALREADY_FINISHED = 0x0006;
        public const ushort ERROR_INSTANCE_CANCELLED = 0x0007;
        public const ushort ERROR_INVALID_DELTA_TIME = 0x0008;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_CRAFTING, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_CRAFTING, OperationResult.ERROR_DENIED);

        public static OperationResult Started()
            => OperationResult.Success(SYSTEM_CRAFTING, SUCCESS_STARTED);

        public static OperationResult Completed()
            => OperationResult.Success(SYSTEM_CRAFTING, SUCCESS_COMPLETED);

        public static OperationResult Cancelled()
            => OperationResult.Success(SYSTEM_CRAFTING, SUCCESS_CANCELLED);

        public static OperationResult Ready()
            => OperationResult.Success(SYSTEM_CRAFTING, SUCCESS_READY);

        public static OperationResult ProgressUpdated()
            => OperationResult.Success(SYSTEM_CRAFTING, SUCCESS_PROGRESS_UPDATED);

        public static OperationResult RecipeIsNull()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_RECIPE_IS_NULL);

        public static OperationResult StationMissing()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_STATION_MISSING);

        public static OperationResult RefundFailed()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_REFUND_FAILED);

        public static OperationResult InstanceIsNull()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_INSTANCE_IS_NULL);

        public static OperationResult InstanceNotReady()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_INSTANCE_NOT_READY);

        public static OperationResult InstanceAlreadyFinished()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_INSTANCE_ALREADY_FINISHED);

        public static OperationResult InstanceCancelled()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_INSTANCE_CANCELLED);

        public static OperationResult InvalidDeltaTime()
            => OperationResult.Error(SYSTEM_CRAFTING, ERROR_INVALID_DELTA_TIME);
    }
}
