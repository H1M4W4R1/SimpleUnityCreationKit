using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Wallets;

namespace Systems.SimpleEconomy.Data.Context
{
    /// <summary>
    ///     Readonly struct containing context for currency adding
    /// </summary>
    public readonly ref struct CurrencyAddContext
    {
        /// <summary>
        ///     Currency to add
        /// </summary>
        public readonly CurrencyBase currency;
        
        /// <summary>
        ///     Wallet to which currency is added
        /// </summary>
        public readonly CurrencyWalletBase wallet;
        
        /// <summary>
        ///     Amount of currency that was expected to be added
        /// </summary>
        public readonly long amount;
        
        public CurrencyAddContext(CurrencyBase currency, CurrencyWalletBase wallet, long amount)
        {
            this.currency = currency;
            this.wallet = wallet;
            this.amount = amount;
        }
    }
}