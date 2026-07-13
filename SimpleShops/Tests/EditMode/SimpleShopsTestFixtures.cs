using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Components;
using Systems.SimpleShops.Data.Context;
using Systems.SimpleShops.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleShops.Tests
{
    public abstract class SimpleShopsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                Object createdObject = _createdObjects[i];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestShop CreateShop(IReadOnlyList<ShopOfferBase> offers)
        {
            GameObject gameObject = Track(new GameObject(nameof(TestShop)));
            gameObject.SetActive(false);
            TestShop shop = gameObject.AddComponent<TestShop>();
            shop.ConfigureOffersForTests(offers);
            return shop;
        }

        protected TestPurchaseOffer CreatePurchaseOffer()
        {
            return Track(ScriptableObject.CreateInstance<TestPurchaseOffer>());
        }

        protected TestSellOffer CreateSellOffer()
        {
            return Track(ScriptableObject.CreateInstance<TestSellOffer>());
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }
    }

    public sealed class TestCustomer : IShopCustomer
    {
    }

    public sealed class TestPurchaseOffer : PurchaseOfferBase
    {
        public bool RejectPurchase { get; set; }
        public int PurchasedCount { get; private set; }
        public int PurchaseFailedCount { get; private set; }

        protected internal override OperationResult CanPurchase(in ShopTransactionContext context)
        {
            if (RejectPurchase) return ShopOperations.Denied();
            return base.CanPurchase(in context);
        }

        protected internal override void OnPurchased(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            PurchasedCount++;
        }

        protected internal override void OnPurchaseFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            PurchaseFailedCount++;
        }
    }

    public sealed class TestSellOffer : SellOfferBase
    {
        public bool RejectSell { get; set; }
        public int SoldCount { get; private set; }
        public int SellFailedCount { get; private set; }

        protected internal override OperationResult CanSell(in ShopTransactionContext context)
        {
            if (RejectSell) return ShopOperations.Denied();
            return base.CanSell(in context);
        }

        protected internal override void OnSold(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            SoldCount++;
        }

        protected internal override void OnSellFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            SellFailedCount++;
        }
    }

    public sealed class TestShop : ShopBase
    {
        public bool RejectPurchase { get; set; }
        public bool RejectSell { get; set; }
        public bool RejectCanPayCosts { get; set; }
        public bool RejectCanGrantReturns { get; set; }
        public bool FailPayCosts { get; set; }
        public bool FailRefundCosts { get; set; }
        public bool FailGrantReturns { get; set; }
        public bool FailRollbackReturns { get; set; }
        public int CanPayCostsCount { get; private set; }
        public int PayCostsCount { get; private set; }
        public int RefundCostsCount { get; private set; }
        public int CanGrantReturnsCount { get; private set; }
        public int GrantReturnsCount { get; private set; }
        public int RollbackReturnsCount { get; private set; }
        public int PurchaseCount { get; private set; }
        public int PurchaseFailedCount { get; private set; }
        public int SellCount { get; private set; }
        public int SellFailedCount { get; private set; }

        protected internal override OperationResult CanPurchaseOffer(in ShopTransactionContext context)
        {
            if (RejectPurchase) return ShopOperations.Denied();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult CanSellOffer(in ShopTransactionContext context)
        {
            if (RejectSell) return ShopOperations.Denied();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult CanPayTransactionCosts(in ShopTransactionContext context)
        {
            CanPayCostsCount++;
            if (RejectCanPayCosts) return ShopOperations.TransactionCostUnavailable();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult PayTransactionCosts(in ShopTransactionContext context)
        {
            PayCostsCount++;
            if (FailPayCosts) return ShopOperations.TransactionCostPaymentFailed();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult RefundTransactionCosts(in ShopTransactionContext context)
        {
            RefundCostsCount++;
            if (FailRefundCosts) return ShopOperations.TransactionCostRefundFailed();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult CanGrantTransactionReturns(in ShopTransactionContext context)
        {
            CanGrantReturnsCount++;
            if (RejectCanGrantReturns) return ShopOperations.TransactionReturnUnavailable();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult GrantTransactionReturns(in ShopTransactionContext context)
        {
            GrantReturnsCount++;
            if (FailGrantReturns) return ShopOperations.TransactionReturnGrantFailed();
            return ShopOperations.Permitted();
        }

        protected internal override OperationResult RollbackTransactionReturns(in ShopTransactionContext context)
        {
            RollbackReturnsCount++;
            if (FailRollbackReturns) return ShopOperations.TransactionReturnRollbackFailed();
            return ShopOperations.Permitted();
        }

        protected internal override void OnPurchased(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            PurchaseCount++;
            base.OnPurchased(in context, in result);
        }

        protected internal override void OnPurchaseFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            PurchaseFailedCount++;
            base.OnPurchaseFailed(in context, in result);
        }

        protected internal override void OnSold(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            SellCount++;
            base.OnSold(in context, in result);
        }

        protected internal override void OnSellFailed(
            in ShopTransactionContext context,
            in OperationResult result)
        {
            SellFailedCount++;
            base.OnSellFailed(in context, in result);
        }
    }
}
