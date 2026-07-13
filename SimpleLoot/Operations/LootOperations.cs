using Systems.SimpleCore.Operations;

namespace Systems.SimpleLoot.Operations
{
    public static class LootOperations
    {
        public const ushort SYSTEM_LOOT = 0x000A;

        public const ushort SUCCESS_LOOT_GENERATED = 1;

        public const ushort ERROR_GENERATOR_NOT_FOUND  = 1;
        public const ushort ERROR_NO_VALID_ITEMS        = 2;
        public const ushort ERROR_ITEM_CONDITION_FAILED = 3;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_LOOT, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_LOOT, OperationResult.ERROR_DENIED);

        public static OperationResult ItemConditionFailed()
            => OperationResult.Error(SYSTEM_LOOT, ERROR_ITEM_CONDITION_FAILED);

        public static OperationResult GeneratorNotFound()
            => OperationResult.Error(SYSTEM_LOOT, ERROR_GENERATOR_NOT_FOUND);

        public static OperationResult NoValidItems()
            => OperationResult.Error(SYSTEM_LOOT, ERROR_NO_VALID_ITEMS);

        public static OperationResult LootGenerated()
            => OperationResult.Success(SYSTEM_LOOT, SUCCESS_LOOT_GENERATED);
    }
}
