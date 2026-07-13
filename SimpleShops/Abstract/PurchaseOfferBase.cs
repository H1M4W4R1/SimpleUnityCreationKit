using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Data.Context;

namespace Systems.SimpleShops.Abstract
{
    public abstract class PurchaseOfferBase : ShopOfferBase
    {
        protected internal virtual OperationResult CanPurchase(in ShopTransactionContext context)
            => CanTransact(in context);

        protected internal virtual void OnPurchased(
            in ShopTransactionContext context,
            in OperationResult result)
            => OnTransactionCompleted(in context, in result);

        protected internal virtual void OnPurchaseFailed(
            in ShopTransactionContext context,
            in OperationResult result)
            => OnTransactionFailed(in context, in result);
    }
}
