using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Components;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Data.Enums;
using Systems.SimpleShops.Operations;
using UnityEngine;

namespace Systems.SimpleShops.Utility
{
    public static class ShopAPI
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() { }

        public static OperationResult CanPurchase(
            [CanBeNull] ShopBase shop,
            [CanBeNull] PurchaseOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                shop,
                offer,
                customer,
                ShopTransactionKind.Purchase,
                flags);
            return CanPurchase(in context);
        }

        public static OperationResult CanPurchase(in ShopTransactionContext context)
        {
            OperationResult result = ValidateContext(in context, ShopTransactionKind.Purchase);
            if (!result) return result;

            PurchaseOfferBase purchaseOffer = (PurchaseOfferBase)context.offer;
            if ((context.flags & ShopTransactionFlags.IgnoreShopConditions) == 0)
            {
                result = context.shop!.CanPurchaseOffer(in context);
                if (!result) return result;
            }

            if ((context.flags & ShopTransactionFlags.IgnoreOfferConditions) == 0)
            {
                result = purchaseOffer!.CanPurchase(in context);
                if (!result) return result;
            }

            return ValidateTransactionStages(in context);
        }

        public static OperationResult TryPurchase(
            [CanBeNull] ShopBase shop,
            [CanBeNull] PurchaseOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                shop,
                offer,
                customer,
                ShopTransactionKind.Purchase,
                flags);
            return TryPurchase(in context);
        }

        public static OperationResult TryPurchase(in ShopTransactionContext context)
        {
            OperationResult result = CanPurchase(in context);
            if (!result)
            {
                NotifyPurchaseFailed(in context, in result);
                return result;
            }

            result = ApplyTransaction(in context);
            if (!result)
            {
                NotifyPurchaseFailed(in context, in result);
                return result;
            }

            OperationResult completed = ShopOperations.TransactionCompleted();
            context.shop!.OnPurchased(in context, in completed);
            return completed;
        }

        public static OperationResult CanSell(
            [CanBeNull] ShopBase shop,
            [CanBeNull] SellOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                shop,
                offer,
                customer,
                ShopTransactionKind.Sell,
                flags);
            return CanSell(in context);
        }

        public static OperationResult CanSell(in ShopTransactionContext context)
        {
            OperationResult result = ValidateContext(in context, ShopTransactionKind.Sell);
            if (!result) return result;

            SellOfferBase sellOffer = (SellOfferBase)context.offer;
            if ((context.flags & ShopTransactionFlags.IgnoreShopConditions) == 0)
            {
                result = context.shop!.CanSellOffer(in context);
                if (!result) return result;
            }

            if ((context.flags & ShopTransactionFlags.IgnoreOfferConditions) == 0)
            {
                result = sellOffer!.CanSell(in context);
                if (!result) return result;
            }

            return ValidateTransactionStages(in context);
        }

        public static OperationResult TrySell(
            [CanBeNull] ShopBase shop,
            [CanBeNull] SellOfferBase offer,
            [CanBeNull] IShopCustomer customer = null,
            ShopTransactionFlags flags = ShopTransactionFlags.None)
        {
            ShopTransactionContext context = new ShopTransactionContext(
                shop,
                offer,
                customer,
                ShopTransactionKind.Sell,
                flags);
            return TrySell(in context);
        }

        public static OperationResult TrySell(in ShopTransactionContext context)
        {
            OperationResult result = CanSell(in context);
            if (!result)
            {
                NotifySellFailed(in context, in result);
                return result;
            }

            result = ApplyTransaction(in context);
            if (!result)
            {
                NotifySellFailed(in context, in result);
                return result;
            }

            OperationResult completed = ShopOperations.TransactionCompleted();
            context.shop!.OnSold(in context, in completed);
            return completed;
        }

        private static OperationResult ValidateContext(
            in ShopTransactionContext context,
            ShopTransactionKind expectedKind)
        {
            if (ReferenceEquals(context.shop, null)) return ShopOperations.ShopIsNull();
            if (!context.shop) return ShopOperations.ShopIsNull();
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

            if (!ShopHasOffer(context.shop, context.offer)) return ShopOperations.OfferNotAvailable();
            return ShopOperations.Permitted();
        }

        private static bool ShopHasOffer([NotNull] ShopBase shop, [NotNull] ShopOfferBase offer)
        {
            IReadOnlyList<ShopOfferBase> offers = shop.Offers;
            for (int i = 0; i < offers.Count; i++)
            {
                if (ReferenceEquals(offers[i], offer)) return true;
            }

            return false;
        }

        private static OperationResult ValidateTransactionStages(in ShopTransactionContext context)
        {
            if ((context.flags & ShopTransactionFlags.IgnoreTransactionConditions) != 0)
                return ShopOperations.Permitted();

            OperationResult result = context.shop!.CanPayTransactionCosts(in context);
            if (!result) return result;

            return context.shop.CanGrantTransactionReturns(in context);
        }

        private static OperationResult ApplyTransaction(in ShopTransactionContext context)
        {
            OperationResult result = context.shop!.PayTransactionCosts(in context);
            if (!result) return result;

            result = context.shop.GrantTransactionReturns(in context);
            if (!result) return RevertTransaction(in context, in result);

            return ShopOperations.TransactionCompleted();
        }

        private static OperationResult RevertTransaction(
            in ShopTransactionContext context,
            in OperationResult failure)
        {
            OperationResult rollbackResult = context.shop!.RollbackTransactionReturns(in context);
            if (!rollbackResult) return ShopOperations.RevertFailed();

            OperationResult refundResult = context.shop.RefundTransactionCosts(in context);
            if (!refundResult) return ShopOperations.RevertFailed();

            return failure;
        }

        private static void NotifyPurchaseFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.shop, null)) return;
            if (!context.shop) return;
            context.shop.OnPurchaseFailed(in context, in result);
        }

        private static void NotifySellFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.shop, null)) return;
            if (!context.shop) return;
            context.shop.OnSellFailed(in context, in result);
        }
    }
}
