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

        #endregion

        #region Success codes

        /// <summary>The object successfully joined the faction.</summary>
        public const ushort SUCCESS_JOINED = 1;

        /// <summary>The object successfully left the faction.</summary>
        public const ushort SUCCESS_LEFT = 2;

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

        /// <summary>Returns a successful join result.</summary>
        public static OperationResult Joined()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_JOINED);

        /// <summary>Returns a successful leave result.</summary>
        public static OperationResult Left()
            => OperationResult.Success(SYSTEM_FACTION, SUCCESS_LEFT);

        #endregion
    }
}
