using Systems.SimpleCore.Operations;

namespace Systems.SimplePermissions.Operations
{
    /// <summary>
    ///     Operation results emitted by SimplePermissions.
    /// </summary>
    public static class PermissionOperations
    {
        public const ushort SYSTEM_PERMISSIONS = 0x002A;

        public const ushort SUCCESS_GRANTED = 1;
        public const ushort SUCCESS_DENIED = 2;
        public const ushort SUCCESS_REVOKED = 3;
        public const ushort SUCCESS_ALREADY_GRANTED = 4;
        public const ushort SUCCESS_ALREADY_DENIED = 5;
        public const ushort SUCCESS_ALREADY_REVOKED = 6;

        public const ushort ERROR_PERMISSION_NOT_CONFIGURED = 1;
        public const ushort ERROR_PERMISSION_REJECTED = 2;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_PERMISSIONS, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Granted()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_GRANTED);

        public static OperationResult Denied()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_DENIED);

        public static OperationResult Revoked()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_REVOKED);

        public static OperationResult AlreadyGranted()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_ALREADY_GRANTED);

        public static OperationResult AlreadyDenied()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_ALREADY_DENIED);

        public static OperationResult AlreadyRevoked()
            => OperationResult.Success(SYSTEM_PERMISSIONS, SUCCESS_ALREADY_REVOKED);

        public static OperationResult PermissionNotConfigured()
            => OperationResult.Error(SYSTEM_PERMISSIONS, ERROR_PERMISSION_NOT_CONFIGURED);

        public static OperationResult PermissionRejected()
            => OperationResult.Error(SYSTEM_PERMISSIONS, ERROR_PERMISSION_REJECTED);
    }
}
