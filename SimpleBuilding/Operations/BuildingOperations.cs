using Systems.SimpleCore.Operations;

namespace Systems.SimpleBuilding.Operations
{
    /// <summary>
    ///     Operation results returned by SimpleBuilding.
    /// </summary>
    public static class BuildingOperations
    {
        public const ushort SYSTEM_BUILDING = 0x0014;

        public const ushort SUCCESS_PLACED = 0x0001;
        public const ushort SUCCESS_DEMOLISHED = 0x0002;

        public const ushort ERROR_ENTRY_IS_NULL = 0x0001;
        public const ushort ERROR_PREFAB_MISSING = 0x0002;
        public const ushort ERROR_BUILDING_IS_NULL = 0x0003;
        public const ushort ERROR_BUILDING_ALREADY_DESTROYED = 0x0004;
        public const ushort ERROR_ENTRY_MISSING = 0x0005;
        public const ushort ERROR_SLOT_COUNT_INVALID = 0x0006;
        public const ushort ERROR_SLOT_MISSING = 0x0007;
        public const ushort ERROR_SLOT_OCCUPIED = 0x0008;
        public const ushort ERROR_REFUND_FAILED = 0x0009;
        public const ushort ERROR_RAYCAST_NOT_HIT = 0x000A;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_BUILDING, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_BUILDING, OperationResult.ERROR_DENIED);

        public static OperationResult Placed()
            => OperationResult.Success(SYSTEM_BUILDING, SUCCESS_PLACED);

        public static OperationResult Demolished()
            => OperationResult.Success(SYSTEM_BUILDING, SUCCESS_DEMOLISHED);

        public static OperationResult EntryIsNull()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_ENTRY_IS_NULL);

        public static OperationResult PrefabMissing()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_PREFAB_MISSING);

        public static OperationResult BuildingIsNull()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_BUILDING_IS_NULL);

        public static OperationResult BuildingAlreadyDestroyed()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_BUILDING_ALREADY_DESTROYED);

        public static OperationResult EntryMissing()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_ENTRY_MISSING);

        public static OperationResult SlotCountInvalid()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_SLOT_COUNT_INVALID);

        public static OperationResult SlotMissing()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_SLOT_MISSING);

        public static OperationResult SlotOccupied()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_SLOT_OCCUPIED);

        public static OperationResult RefundFailed()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_REFUND_FAILED);

        public static OperationResult RaycastNotHit()
            => OperationResult.Error(SYSTEM_BUILDING, ERROR_RAYCAST_NOT_HIT);
    }
}
