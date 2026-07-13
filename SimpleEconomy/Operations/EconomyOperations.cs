using Systems.SimpleCore.Operations;

namespace Systems.SimpleEconomy.Operations
{
    public static class EconomyOperations
    {
        public const ushort SYSTEM_ECONOMY = 0x0001;


        public const ushort ERROR_NOT_ENOUGH_CURRENCY = 1;
        public const ushort ERROR_INVALID_CURRENCY_AMOUNT = 2;
        public const ushort ERROR_CURRENCY_NOT_FOUND = 3;
        public const ushort ERROR_OVERFLOW = 4;

        public const ushort SUCCESS_CURRENCY_ADDED = 1;
        public const ushort SUCCESS_CURRENCY_TAKEN = 2;
        public const ushort SUCCESS_CURRENCY_TAKEN_PARTIAL = 3;

        public static OperationResult NotEnoughCurrency()
            => OperationResult.Error(SYSTEM_ECONOMY, ERROR_NOT_ENOUGH_CURRENCY);

        public static OperationResult InvalidCurrencyAmount()
            => OperationResult.Error(SYSTEM_ECONOMY, ERROR_INVALID_CURRENCY_AMOUNT);

        public static OperationResult CurrencyNotFound()
            => OperationResult.Error(SYSTEM_ECONOMY, ERROR_CURRENCY_NOT_FOUND);

        public static OperationResult Overflow()
            => OperationResult.Error(SYSTEM_ECONOMY, ERROR_OVERFLOW);

        public static OperationResult CurrencyAdded() => OperationResult.Success(SYSTEM_ECONOMY, SUCCESS_CURRENCY_ADDED);
        public static OperationResult CurrencyTaken() => OperationResult.Success(SYSTEM_ECONOMY, SUCCESS_CURRENCY_TAKEN);
        public static OperationResult CurrencyTakenPartial() => OperationResult.Success(SYSTEM_ECONOMY, SUCCESS_CURRENCY_TAKEN_PARTIAL);

        public static OperationResult Permitted() => OperationResult.Success(SYSTEM_ECONOMY, OperationResult.SUCCESS_PERMITTED);
    }
}