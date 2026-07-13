using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Data.Context;
using UnityEngine;

namespace Systems.SimpleShops.Examples.Scripts
{
    public sealed class ExamplePotionPurchaseOffer : PurchaseOfferBase
    {
        protected internal override void OnPurchased(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            Debug.Log("[SimpleShops] Potion purchase completed: " + result);
        }
    }
}
