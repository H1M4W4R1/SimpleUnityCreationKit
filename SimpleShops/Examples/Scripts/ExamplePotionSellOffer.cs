using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Data.Context;
using UnityEngine;

namespace Systems.SimpleShops.Examples.Scripts
{
    public sealed class ExamplePotionSellOffer : SellOfferBase
    {
        protected internal override void OnSold(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            Debug.Log("[SimpleShops] Potion sell completed: " + result);
        }
    }
}
