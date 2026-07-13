using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Data.Enums;
using Systems.SimpleShops.Operations;
using UnityEngine;

namespace Systems.SimpleShops.Components
{
    public abstract class ShopBase : MonoBehaviour
    {
        [SerializeField] private List<ShopOfferBase> _offers = new List<ShopOfferBase>();

        public IReadOnlyList<ShopOfferBase> Offers => _offers;

        public OperationResult CanPurchase(
            [CanBeNull] PurchaseOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                this,
                offer,
                customer,
                ShopTransactionKind.Purchase,
                flags);
            return CanPurchase(in context);
        }

        public OperationResult TryPurchase(
            [CanBeNull] PurchaseOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                this,
                offer,
                customer,
                ShopTransactionKind.Purchase,
                flags);
            return TryPurchase(in context);
        }

        public OperationResult CanSell(
            [CanBeNull] SellOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                this,
                offer,
                customer,
                ShopTransactionKind.Sell,
                flags);
            return CanSell(in context);
        }

        public OperationResult TrySell(
            [CanBeNull] SellOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                this,
                offer,
                customer,
                ShopTransactionKind.Sell,
                flags);
            return TrySell(in context);
        }

        private OperationResult CanPurchase(in ShopTransactionContext context)
        {
            OperationResult result = ValidateContext(in context, ShopTransactionKind.Purchase);
            if (!result) return result;

            PurchaseOfferBase purchaseOffer = (PurchaseOfferBase)context.offer;
            if ((context.flags & ShopTransactionFlags.IgnoreShopConditions) == 0)
            {
                result = CanPurchaseOffer(in context);
                if (!result) return result;
            }

            if ((context.flags & ShopTransactionFlags.IgnoreOfferConditions) == 0)
            {
                result = purchaseOffer.CanPurchase(in context);
                if (!result) return result;
            }

            return ValidateTransactionStages(in context);
        }

        private OperationResult TryPurchase(in ShopTransactionContext context)
        {
            OperationResult result = CanPurchase(in context);
            if (!result)
            {
                OnPurchaseFailed(in context, in result);
                return result;
            }

            result = ApplyTransaction(in context);
            if (!result)
            {
                OnPurchaseFailed(in context, in result);
                return result;
            }

            OperationResult completed = ShopOperations.TransactionCompleted();
            OnPurchased(in context, in completed);
            return completed;
        }

        private OperationResult CanSell(in ShopTransactionContext context)
        {
            OperationResult result = ValidateContext(in context, ShopTransactionKind.Sell);
            if (!result) return result;

            SellOfferBase sellOffer = (SellOfferBase)context.offer;
            if ((context.flags & ShopTransactionFlags.IgnoreShopConditions) == 0)
            {
                result = CanSellOffer(in context);
                if (!result) return result;
            }

            if ((context.flags & ShopTransactionFlags.IgnoreOfferConditions) == 0)
            {
                result = sellOffer.CanSell(in context);
                if (!result) return result;
            }

            return ValidateTransactionStages(in context);
        }

        private OperationResult TrySell(in ShopTransactionContext context)
        {
            OperationResult result = CanSell(in context);
            if (!result)
            {
                OnSellFailed(in context, in result);
                return result;
            }

            result = ApplyTransaction(in context);
            if (!result)
            {
                OnSellFailed(in context, in result);
                return result;
            }

            OperationResult completed = ShopOperations.TransactionCompleted();
            OnSold(in context, in completed);
            return completed;
        }

        private OperationResult ValidateContext(
            in ShopTransactionContext context,
            ShopTransactionKind expectedKind)
        {
            if (ReferenceEquals(context.offer, null)) return ShopOperations.OfferIsNull();
            if (!context.offer) return ShopOperations.OfferIsNull();
            if (ReferenceEquals(context.customer, null)) return ShopOperations.CustomerIsNull();
            if (context.transactionKind != expectedKind) return ShopOperations.InvalidOfferType();

            if (expectedKind == ShopTransactionKind.Purchase &&
                context.offer is not PurchaseOfferBase)
                return ShopOperations.InvalidOfferType();

            if (expectedKind == ShopTransactionKind.Sell &&
                context.offer is not SellOfferBase)
                return ShopOperations.InvalidOfferType();

            if (!HasOffer(context.offer)) return ShopOperations.OfferNotAvailable();
            return ShopOperations.Permitted();
        }

        private bool HasOffer([NotNull] ShopOfferBase offer)
        {
            for (int offerIndex = 0; offerIndex < _offers.Count; offerIndex++)
            {
                if (ReferenceEquals(_offers[offerIndex], offer)) return true;
            }

            return false;
        }

        private OperationResult ValidateTransactionStages(in ShopTransactionContext context)
        {
            if ((context.flags & ShopTransactionFlags.IgnoreTransactionConditions) != 0)
                return ShopOperations.Permitted();

            OperationResult result = CanPayTransactionCosts(in context);
            if (!result) return result;

            return CanGrantTransactionReturns(in context);
        }

        private OperationResult ApplyTransaction(in ShopTransactionContext context)
        {
            OperationResult result = PayTransactionCosts(in context);
            if (!result) return result;

            result = GrantTransactionReturns(in context);
            if (!result) return RevertTransaction(in context, in result);

            return ShopOperations.TransactionCompleted();
        }

        private OperationResult RevertTransaction(
            in ShopTransactionContext context,
            in OperationResult failure)
        {
            OperationResult rollbackResult = RollbackTransactionReturns(in context);
            if (!rollbackResult) return ShopOperations.RevertFailed();

            OperationResult refundResult = RefundTransactionCosts(in context);
            if (!refundResult) return ShopOperations.RevertFailed();

            return failure;
        }

        protected internal virtual OperationResult CanPurchaseOffer(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult CanSellOffer(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult CanPayTransactionCosts(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult PayTransactionCosts(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult RefundTransactionCosts(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult CanGrantTransactionReturns(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult GrantTransactionReturns(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual OperationResult RollbackTransactionReturns(in ShopTransactionContext context)
            => ShopOperations.Permitted();

        protected internal virtual void OnPurchased(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (context.offer is PurchaseOfferBase purchaseOffer)
            {
                purchaseOffer.OnPurchased(in context, in result);
            }
        }

        protected internal virtual void OnPurchaseFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (context.offer is PurchaseOfferBase purchaseOffer)
            {
                purchaseOffer.OnPurchaseFailed(in context, in result);
            }
        }

        protected internal virtual void OnSold(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (context.offer is SellOfferBase sellOffer)
            {
                sellOffer.OnSold(in context, in result);
            }
        }

        protected internal virtual void OnSellFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (context.offer is SellOfferBase sellOffer)
            {
                sellOffer.OnSellFailed(in context, in result);
            }
        }

#if UNITY_INCLUDE_TESTS
        internal void ConfigureOffersForTests([CanBeNull] IReadOnlyList<ShopOfferBase> offers)
        {
            _offers.Clear();
            if (ReferenceEquals(offers, null)) return;

            for (int i = 0; i < offers.Count; i++)
            {
                _offers.Add(offers[i]);
            }
        }
#endif
    }
}
