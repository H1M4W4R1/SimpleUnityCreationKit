using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Operations;
using UnityEngine;

namespace Systems.SimpleEconomy.Currencies
{
    /// <summary>
    ///     Base class for in-game currency or usable resource such as mana
    /// </summary>
    [AutoCreate("Currencies", CurrencyDatabase.LABEL)] public abstract class CurrencyBase : ScriptableObject
    {
        /// <summary>
        ///     Checks if the specified amount of currency can be added.
        /// </summary>
        protected internal virtual OperationResult CanBeAdded(in CurrencyAddContext context) => EconomyOperations.Permitted();

        /// <summary>
        ///     Check if specified amount of currency can be taken.
        /// </summary>
        protected internal virtual OperationResult CanBeTaken(in CurrencyTakeContext context) => EconomyOperations.Permitted();

        /// <summary>
        ///     Event that is called when currency is taken.
        /// </summary>
        protected internal virtual void OnCurrencyTaken(
            in CurrencyTakeContext context,
            in OperationResult result,
            long amountLeft)
        {
        }

        /// <summary>
        ///     Event that is called when currency take fails.
        /// </summary>
        protected internal virtual void OnCurrencyTakeFailed(
            in CurrencyTakeContext context,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Event that is called when currency is added.
        /// </summary>
        protected internal virtual void OnCurrencyAdded(
            in CurrencyAddContext context,
            in OperationResult result,
            long amountLeft)
        {
        }

        /// <summary>
        ///     Event that is called when currency addition fails.
        /// </summary>
        protected internal virtual void OnCurrencyAddFailed(
            in CurrencyAddContext context,
            in OperationResult result)
        {
        }
    }
}