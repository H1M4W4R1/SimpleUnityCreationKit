using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Operations;
using UnityEngine;

namespace Systems.SimpleShops.Abstract
{
    public abstract class ShopOfferBase : ScriptableObject
    {
        protected internal virtual OperationResult CanTransact(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual void OnTransactionCompleted(
            in ShopTransactionContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnTransactionFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
        }
    }
}
