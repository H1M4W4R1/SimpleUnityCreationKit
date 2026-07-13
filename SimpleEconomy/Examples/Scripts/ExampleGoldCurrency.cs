using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Currencies;
using Systems.SimpleEconomy.Data.Context;
using UnityEngine;

namespace Systems.SimpleEconomy.Examples.Scripts
{
    public sealed class ExampleGoldCurrency : CurrencyBase
    {
        protected internal override void OnCurrencyAdded(
            in CurrencyAddContext context,
            in OperationResult result,
            long amountLeft)
        {
            Debug.Log("[SimpleEconomy] Gold added: " + context.amount + ", left: " + amountLeft);
        }

        protected internal override void OnCurrencyTaken(
            in CurrencyTakeContext context,
            in OperationResult result,
            long amountLeft)
        {
            Debug.Log("[SimpleEconomy] Gold taken: " + context.amountExpected + ", left: " + amountLeft);
        }
    }
}
