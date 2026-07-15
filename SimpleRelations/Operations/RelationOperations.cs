using Systems.SimpleCore.Operations;

namespace Systems.SimpleRelations.Operations
{
    /// <summary>Operation results returned by SimpleRelations.</summary>
    public static class RelationOperations
    {
        /// <summary>System code reserved for SimpleRelations.</summary>
        public const ushort SYSTEM_RELATIONS = 0x0015;

        public const ushort SUCCESS_RELATION_CHANGED = 1;
        public const ushort SUCCESS_RELATION_SET = 2;

        public const ushort ERROR_RELATION_TYPE_NOT_FOUND = 1;
        public const ushort ERROR_INVALID_TARGET = 2;
        public const ushort ERROR_INVALID_AMOUNT = 3;
        public const ushort ERROR_VALUE_OVERFLOW = 4;

        /// <summary>Generic successful validation result for relation callbacks.</summary>
        public static OperationResult Permitted()
        {
            return OperationResult.Success(SYSTEM_RELATIONS, OperationResult.SUCCESS_PERMITTED);
        }

        /// <summary>Returned after a relation value is increased or decreased.</summary>
        public static OperationResult RelationChanged()
        {
            return OperationResult.Success(SYSTEM_RELATIONS, SUCCESS_RELATION_CHANGED);
        }

        /// <summary>Returned after a relation value is explicitly assigned.</summary>
        public static OperationResult RelationSet()
        {
            return OperationResult.Success(SYSTEM_RELATIONS, SUCCESS_RELATION_SET);
        }

        /// <summary>Returned when the requested relation type asset is not loaded.</summary>
        public static OperationResult RelationTypeNotFound()
        {
            return OperationResult.Error(SYSTEM_RELATIONS, ERROR_RELATION_TYPE_NOT_FOUND);
        }

        /// <summary>Returned for null, destroyed, or self relation components.</summary>
        public static OperationResult InvalidTarget()
        {
            return OperationResult.Error(SYSTEM_RELATIONS, ERROR_INVALID_TARGET);
        }

        /// <summary>Returned when a change amount is zero.</summary>
        public static OperationResult InvalidAmount()
        {
            return OperationResult.Error(SYSTEM_RELATIONS, ERROR_INVALID_AMOUNT);
        }

        /// <summary>Returned when applying a change would exceed the <see cref="int"/> range.</summary>
        public static OperationResult ValueOverflow()
        {
            return OperationResult.Error(SYSTEM_RELATIONS, ERROR_VALUE_OVERFLOW);
        }
    }
}
