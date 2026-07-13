using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data.Context;
using Systems.SimpleEconomy.Wallets;
using UnityEngine;

namespace Systems.SimpleEconomy.Examples.Scripts
{
    public sealed class ExampleGoldWallet : CurrencyWalletBase<ExampleGoldCurrency>
    {
        public void ResetBalance()
        {
            Balance = 0L;
            Debug.Log("[SimpleEconomy] Wallet balance reset.");
        }

        protected override void OnCurrencyAdded(
            in CurrencyAddContext context,
            in OperationResult result,
            long amountLeft)
        {
            base.OnCurrencyAdded(in context, in result, amountLeft);
            Debug.Log("[SimpleEconomy] Wallet balance after add: " + Balance);
        }

        protected override void OnCurrencyTaken(
            in CurrencyTakeContext context,
            in OperationResult result,
            long amountLeft)
        {
            base.OnCurrencyTaken(in context, in result, amountLeft);
            Debug.Log("[SimpleEconomy] Wallet balance after take: " + Balance);
        }
    }
}
