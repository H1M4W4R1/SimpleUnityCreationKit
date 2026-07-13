using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Wallets;

namespace Systems.SimpleEconomy.Data.Context
{
    /// <summary>
    ///     Readonly struct containing context for currency taking
    /// </summary>
    public readonly ref struct CurrencyTakeContext
    {
        /// <summary>
        ///     Currency to take
        /// </summary>
        public readonly CurrencyBase currency;
        
        /// <summary>
        ///     Wallet from which currency is taken
        /// </summary>
        public readonly CurrencyWalletBase wallet;
        
        /// <summary>
        ///     Amount of currency that was expected to be taken
        /// </summary>
        public readonly long amountExpected;

        public CurrencyTakeContext(CurrencyBase currency, CurrencyWalletBase wallet, long amountExpected)
        {
            this.currency = currency;
            this.wallet = wallet;
            this.amountExpected = amountExpected;
        }
    }
}