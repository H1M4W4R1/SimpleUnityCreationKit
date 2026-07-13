using System;

namespace Systems.SimpleEconomy.Data.Enums
{
    [Flags]
    public enum ModifyWalletCurrencyFlags
    {
        None = 0,

        /// <summary>
        ///     Currency will be added/taken without checking conditions.
        ///     For TryTake, the balance is still clamped to zero unless <see cref="IgnoreBalanceLimits"/> is also set.
        ///     For TryAdd, overflow protection still applies unless <see cref="IgnoreBalanceLimits"/> is also set.
        /// </summary>
        IgnoreConditions = 1 << 0,

        /// <summary>
        ///     When set on TryTake, allows the balance to go below zero instead of clamping.
        /// </summary>
        IgnoreBalanceLimits = 1 << 1
    }
}