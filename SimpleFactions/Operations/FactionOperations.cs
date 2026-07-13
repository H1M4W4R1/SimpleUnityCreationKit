using Systems.SimpleCore.Operations;

namespace Systems.SimpleFactions.Operations
{
    /// <summary>
    ///     Centralised result-code registry and <see cref="OperationResult"/> factory for the
    ///     SimpleFactions system.
    /// </summary>
    public static class FactionOperations
    {
        /// <summary>Unique system identifier for SimpleFactions.</summary>
        public const ushort SYSTEM_FACTION = 0x000B;

        #region Error codes

        /// <summary>The requested faction was not found in <c>FactionDatabase</c>.</summary>
        public const ushort ERROR_FACTION_NOT_FOUND = 1;

        /// <summary>Join was attempted on an object that is already a member.</summary>
        public const ushort ERROR_ALREADY_MEMBER = 2;

        /// <summary>An operation requiring membership was attempted on a non-member.</summary>
        public const ushort ERROR_NOT_A_MEMBER = 3;

        /// <summary>A zero-amount reputation change was attempted.</summary>
        public const ushort ERROR_INVALID_REPUTATION = 4;

        /// <summary>The supplied <c>ReputationLevelBase</c> is not in the faction's level list.</summary>
        public const ushort ERROR_LEVEL_NOT_IN_FACTION = 5;

        /// <summary>An automatic or check-gated promotion was denied.</summary>
        public const ushort ERROR_PROMOTION_DENIED = 6;

        /// <summary>An automatic or check-gated demotion was denied.</summary>
        public const ushort ERROR_DEMOTION_DENIED = 7;

        #endregion

        #region Success codes

        /// <summary>The object successfully joined the faction.</summary>
        public const ushort SUCCESS_JOINED = 1;

        /// <summary>The object successfully left the faction.</summary>
        public const ushort SUCCESS_LEFT = 2;

        /// <summary>Reputation was modified successfully.</summary>
        public const ushort SUCCESS_REPUTATION_CHANGED = 3;

        /// <summary>A reputation level was assigned successfully.</summary>
        public const ushort SUCCESS_LEVEL_ASSIGNED = 4;

        /// <summary>The active reputation level was cleared (set to none).</summary>
        public const ushort SUCCESS_LEVEL_CLEARED = 5;

        #endregion

        #region Factories

        /// <summary>Returns a generic check-passed result.</summary>
        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_FACTION, OperationResult.SUCCESS_PERMITTED);

        /// <summary>Returns a generic check-denied result.</summary>
        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_FACTION, OperationResult.ERROR_DENIED);

        /// <summary>Returns a faction-not-found error.</summary>
        public static OperationResult FactionNotFound()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_FACTION_NOT_FOUND);

        /// <summary>Returns an already-member error.</summary>
        public static OperationResult AlreadyMember()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_ALREADY_MEMBER);

        /// <summary>Returns a not-a-member error.</summary>
        public static OperationResult NotAMember()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_NOT_A_MEMBER);

        /// <summary>Returns an invalid-reputation-amount error.</summary>
        public static OperationResult InvalidReputation()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_INVALID_REPUTATION);

        /// <summary>Returns a level-not-in-faction error.</summary>
        public static OperationResult LevelNotInFaction()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_LEVEL_NOT_IN_FACTION);

        /// <summary>Returns a promotion-denied error.</summary>
        public static OperationResult PromotionDenied()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_PROMOTION_DENIED);

        /// <summary>Returns a demotion-denied error.</summary>
        public static OperationResult DemotionDenied()
            => OperationResult.Error(SYSTEM_FACTION, ERROR_DEMOTION_DENIED);

        /// <summary>Returns a successful join result.</summary>
        public static OperationResult Joined()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_JOINED);

        /// <summary>Returns a successful leave result.</summary>
        public static OperationResult Left()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_LEFT);

        /// <summary>Returns a successful reputation-changed result.</summary>
        public static OperationResult ReputationChanged()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_REPUTATION_CHANGED);

        /// <summary>Returns a successful level-assigned result.</summary>
        public static OperationResult LevelAssigned()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_LEVEL_ASSIGNED);

        /// <summary>Returns a successful level-cleared result.</summary>
        public static OperationResult LevelCleared()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_LEVEL_CLEARED);

        #endregion
    }
}
