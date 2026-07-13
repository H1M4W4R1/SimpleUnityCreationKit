using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Data;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Data.Enums;
using Systems.SimpleEconomy.Operations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleEconomy.Wallets
{
    /// <summary>
    ///     Wallet for a single currency type.
    /// </summary>
    /// <typeparam name="TCurrencyType">
    ///     The new() constraint is required by AddressableDatabase.GetExact and limits TCurrencyType
    ///     to concrete (non-abstract) types. ScriptableObject-derived currencies must still be created
    ///     via ScriptableObject.CreateInstance, not new(). Do not use new TCurrencyType() directly.
    /// </typeparam>
    public abstract class CurrencyWalletBase<TCurrencyType> : CurrencyWalletBase
        where TCurrencyType : CurrencyBase, new()
    {
        /// <summary>
        ///     Adds the specified amount of currency to the wallet.
        ///     Includes overflow protection by default.
        /// </summary>
        /// <param name="currencyAmount">Amount of currency to add</param>
        /// <param name="allowOverflow">When true, disables overflow protection</param>
        /// <returns>Remaining amount of currency that could not be added, or -1 if overflow would occur</returns>
        protected virtual long Add(long currencyAmount, bool allowOverflow = false)
        {
            if (!allowOverflow && Balance > 0 && currencyAmount > long.MaxValue - Balance)
                return -1;

            Balance += currencyAmount;
            return 0;
        }

        /// <summary>
        ///     Attempts to add the specified amount of currency to the wallet.
        ///     If the currency cannot be added, a failed operation result is returned.
        /// </summary>
        /// <param name="currencyAmount">Amount of currency to add</param>
        /// <param name="flags">Flags to modify wallet behavior</param>
        /// <returns>Operation result of the add attempt with remaining amount of currency</returns>
        public sealed override OperationResult TryAdd(
            long currencyAmount,
            ModifyWalletCurrencyFlags flags = ModifyWalletCurrencyFlags.None)
        {
            if (currencyAmount <= 0) return EconomyOperations.InvalidCurrencyAmount();

            // Get currency from database
            TCurrencyType currency = CurrencyDatabase.GetExact<TCurrencyType>();
            Assert.IsFalse(ReferenceEquals(currency, null), "Currency was not found in database.");

            if (ReferenceEquals(currency, null)) return EconomyOperations.CurrencyNotFound();

            // Create context
            CurrencyAddContext context = new CurrencyAddContext(currency, this, currencyAmount);

            // Ensure that currency can be added
            OperationResult canAddCurrency = CanAddCurrency(context);
            if (!canAddCurrency && (flags & ModifyWalletCurrencyFlags.IgnoreConditions) == 0)
            {
                // Invoke event
                OnCurrencyAddFailed(context, canAddCurrency);
                return canAddCurrency;
            }

            bool allowOverflow = (flags & ModifyWalletCurrencyFlags.IgnoreBalanceLimits) != 0;

            long remainder;
            lock (_balanceLock)
            {
                remainder = Add(currencyAmount, allowOverflow);
            }

            if (remainder == -1)
            {
                OperationResult overflowResult = EconomyOperations.Overflow();
                OnCurrencyAddFailed(context, overflowResult);
                return overflowResult;
            }

            // Invoke event
            OperationResult currencyAddResult = EconomyOperations.CurrencyAdded();

            OnCurrencyAdded(context, currencyAddResult, remainder);
            return currencyAddResult;
        }


        /// <summary>
        ///     Attempts to take the specified amount of currency from the wallet.
        ///     If the currency cannot be taken, a failed operation result is returned.
        /// </summary>
        /// <param name="currencyAmount">Amount of currency to take</param>
        /// <param name="flags">Flags to modify wallet behavior</param>
        /// <returns>Operation result of the take attempt with remaining amount of currency</returns>
        public sealed override OperationResult TryTake(
            long currencyAmount,
            ModifyWalletCurrencyFlags flags = ModifyWalletCurrencyFlags.None
        )
        {
            if (currencyAmount <= 0) return EconomyOperations.InvalidCurrencyAmount();

            // Get currency from database
            TCurrencyType currency = CurrencyDatabase.GetExact<TCurrencyType>();
            Assert.IsFalse(ReferenceEquals(currency, null), "Currency was not found in database.");

            if (ReferenceEquals(currency, null)) return EconomyOperations.CurrencyNotFound();

            // Create context
            CurrencyTakeContext context = new CurrencyTakeContext(currency, this, currencyAmount);

            // Check balance unless IgnoreBalanceLimits is set
            if ((flags & ModifyWalletCurrencyFlags.IgnoreBalanceLimits) == 0 &&
                (flags & ModifyWalletCurrencyFlags.IgnoreConditions) == 0 &&
                Balance < context.amountExpected)
            {
                OperationResult notEnough = EconomyOperations.NotEnoughCurrency();
                OnCurrencyTakeFailed(context, notEnough);
                return notEnough;
            }

            // Ensure that currency can be taken (custom currency conditions)
            OperationResult canTakeCurrency = CanTakeCurrency(context);
            if (!canTakeCurrency && (flags & ModifyWalletCurrencyFlags.IgnoreConditions) == 0)
            {
                // Invoke event
                OnCurrencyTakeFailed(context, canTakeCurrency);
                return canTakeCurrency;
            }

            bool underflowDetected = false;
            long currencyLeftToTake = currencyAmount;

            lock (_balanceLock)
            {
                long currencyTaken;
                if ((flags & ModifyWalletCurrencyFlags.IgnoreBalanceLimits) != 0)
                {
                    // Underflow protection: ensure Balance - currencyAmount won't wrap
                    if (Balance < 0 && currencyAmount > Balance - long.MinValue)
                    {
                        underflowDetected = true;
                        currencyTaken = 0L;
                    }
                    else
                    {
                        currencyTaken = currencyAmount;
                    }
                }
                else
                {
                    currencyTaken = Balance <= 0 ? 0L : math.min(Balance, currencyAmount);
                }

                currencyLeftToTake = currencyAmount - currencyTaken;

                // Take currency
                if (!underflowDetected) Balance -= currencyTaken;
            }

            if (underflowDetected)
            {
                OperationResult overflowResult = EconomyOperations.Overflow();
                OnCurrencyTakeFailed(context, overflowResult);
                return overflowResult;
            }

            OperationResult currencyTakeResult = currencyLeftToTake > 0
                ? EconomyOperations.CurrencyTakenPartial()
                : EconomyOperations.CurrencyTaken();

            // Invoke event
            OnCurrencyTaken(context, currencyTakeResult, currencyLeftToTake);
            return currencyTakeResult;
        }

        /// <summary>
        ///     Event that is called when currency is taken from the wallet
        /// </summary>
        protected virtual void OnCurrencyTaken(in CurrencyTakeContext context, in OperationResult result, long amountLeft)
            => context.currency.OnCurrencyTaken(context, result, amountLeft);

        /// <summary>
        ///     Event that is called when currency is added to the wallet
        /// </summary>
        protected virtual void OnCurrencyAdded(in CurrencyAddContext context, in OperationResult result, long amountLeft)
            => context.currency.OnCurrencyAdded(context, result, amountLeft);

        /// <summary>
        ///     Event that is called when currency take fails
        /// </summary>
        protected virtual void OnCurrencyTakeFailed(
            in CurrencyTakeContext context,
            in OperationResult result)
            => context.currency.OnCurrencyTakeFailed(context, result);

        /// <summary>
        ///     Event that is called when currency addition fails
        /// </summary>
        protected virtual void OnCurrencyAddFailed(
            in CurrencyAddContext context,
            in OperationResult result)
            => context.currency.OnCurrencyAddFailed(context, result);
    }

    public abstract class CurrencyWalletBase : MonoBehaviour
    {
        protected readonly object _balanceLock = new object();

        /// <summary>
        ///     Balance of the wallet
        /// </summary>
        public long Balance { get; protected set; }

        /// <summary>
        ///     Checks if the wallet has the specified amount of currency.
        ///     For non-positive values, always returns true.
        /// </summary>
        /// <param name="currencyAmount">Amount of currency to check</param>
        /// <returns>True if the wallet has the specified amount of currency, false otherwise</returns>
        public virtual bool Has(long currencyAmount) => currencyAmount <= 0 || Balance >= currencyAmount;

        public abstract OperationResult TryTake(
            long currencyAmount,
            ModifyWalletCurrencyFlags flags = ModifyWalletCurrencyFlags.None);

        public abstract OperationResult TryAdd(
            long currencyAmount,
            ModifyWalletCurrencyFlags flags = ModifyWalletCurrencyFlags.None);

        /// <summary>
        ///     Checks if the specified amount of currency can be taken from the wallet.
        ///     Balance sufficiency is checked separately in TryTake to respect IgnoreBalanceLimits.
        /// </summary>
        protected virtual OperationResult CanTakeCurrency(in CurrencyTakeContext context) =>
            context.currency.CanBeTaken(context);

        /// <summary>
        ///     Checks if the specified amount of currency can be added to the wallet
        /// </summary>
        protected virtual OperationResult CanAddCurrency(in CurrencyAddContext context) =>
            context.currency.CanBeAdded(context);
    }
}
