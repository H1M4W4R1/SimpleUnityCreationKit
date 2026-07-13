using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Data.Context;

namespace Systems.SimpleShops.Abstract
{
    public abstract class SellOfferBase : ShopOfferBase
    {
        protected internal virtual OperationResult CanSell(in ShopTransactionContext context)
            => CanTransact(in context);

        protected internal virtual void OnSold(
            in ShopTransactionContext context,
            in OperationResult result)
            => OnTransactionCompleted(in context, in result);

        protected internal virtual void OnSellFailed(
            in ShopTransactionContext context,
            in OperationResult result)
            => OnTransactionFailed(in context, in result);
    }
}
