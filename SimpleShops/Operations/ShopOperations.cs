using Systems.SimpleCore.Operations;

namespace Systems.SimpleShops.Operations
{
    public static class ShopOperations
    {
        public const ushort SYSTEM_SHOPS = 0x0011;

        public const ushort SUCCESS_TRANSACTION_COMPLETED = 0x0001;

        public const ushort ERROR_SHOP_IS_NULL = 0x0001;
        public const ushort ERROR_OFFER_IS_NULL = 0x0002;
        public const ushort ERROR_INVALID_OFFER_TYPE = 0x0003;
        public const ushort ERROR_CUSTOMER_IS_NULL = 0x0004;
        public const ushort ERROR_OFFER_NOT_AVAILABLE = 0x0005;
        public const ushort ERROR_TRANSACTION_COST_UNAVAILABLE = 0x0006;
        public const ushort ERROR_TRANSACTION_RETURN_UNAVAILABLE = 0x0007;
        public const ushort ERROR_TRANSACTION_COST_PAYMENT_FAILED = 0x0008;
        public const ushort ERROR_TRANSACTION_COST_REFUND_FAILED = 0x0009;
        public const ushort ERROR_TRANSACTION_RETURN_GRANT_FAILED = 0x000A;
        public const ushort ERROR_TRANSACTION_RETURN_ROLLBACK_FAILED = 0x000B;
        public const ushort ERROR_REVERT_FAILED = 0x000C;

        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_SHOPS, OperationResult.SUCCESS_PERMITTED);

        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_SHOPS, OperationResult.ERROR_DENIED);

        public static OperationResult TransactionCompleted()
            => OperationResult.Success(SYSTEM_SHOPS, SUCCESS_TRANSACTION_COMPLETED);

        public static OperationResult ShopIsNull()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_SHOP_IS_NULL);

        public static OperationResult CustomerIsNull()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_CUSTOMER_IS_NULL);

        public static OperationResult OfferIsNull()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_OFFER_IS_NULL);

        public static OperationResult OfferNotAvailable()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_OFFER_NOT_AVAILABLE);

        public static OperationResult InvalidOfferType()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_INVALID_OFFER_TYPE);

        public static OperationResult TransactionCostUnavailable()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_COST_UNAVAILABLE);

        public static OperationResult TransactionReturnUnavailable()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_RETURN_UNAVAILABLE);

        public static OperationResult TransactionCostPaymentFailed()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_COST_PAYMENT_FAILED);

        public static OperationResult TransactionCostRefundFailed()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_COST_REFUND_FAILED);

        public static OperationResult TransactionReturnGrantFailed()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_RETURN_GRANT_FAILED);

        public static OperationResult TransactionReturnRollbackFailed()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_TRANSACTION_RETURN_ROLLBACK_FAILED);

        public static OperationResult RevertFailed()
            => OperationResult.Error(SYSTEM_SHOPS, ERROR_REVERT_FAILED);
    }
}
