using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Data.Enums;
using Systems.SimpleShops.Operations;
using Systems.SimpleShops.Utility;
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
            return ShopAPI.CanPurchase(in context);
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
            return ShopAPI.TryPurchase(in context);
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
            return ShopAPI.CanSell(in context);
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
            return ShopAPI.TrySell(in context);
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
