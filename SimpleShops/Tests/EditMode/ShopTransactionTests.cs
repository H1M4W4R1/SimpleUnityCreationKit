using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleShops.Abstract;
using Systems.SimpleShops.Data.Enums;
using Systems.SimpleShops.Operations;

namespace Systems.SimpleShops.Tests
{
    public sealed class ShopTransactionTests : SimpleShopsTestBase
    {
        [Test]
        public void TryPurchase_WhenOfferBelongsToShop_PaysCostsGrantsReturnsAndCallbacks()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });

            OperationResult result = shop.TryPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.TransactionCompleted(), result);
            Assert.AreEqual(1, shop.PayCostsCount);
            Assert.AreEqual(1, shop.GrantReturnsCount);
            Assert.AreEqual(0, shop.RefundCostsCount);
            Assert.AreEqual(0, shop.RollbackReturnsCount);
            Assert.AreEqual(1, shop.PurchaseCount);
            Assert.AreEqual(1, offer.PurchasedCount);
            Assert.AreEqual(0, shop.PurchaseFailedCount);
        }

        [Test]
        public void TrySell_WhenOfferBelongsToShop_PaysCostsGrantsReturnsAndCallbacks()
        {
            TestSellOffer offer = CreateSellOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });

            OperationResult result = shop.TrySell(offer, new TestCustomer());

            AssertSimilar(ShopOperations.TransactionCompleted(), result);
            Assert.AreEqual(1, shop.PayCostsCount);
            Assert.AreEqual(1, shop.GrantReturnsCount);
            Assert.AreEqual(1, shop.SellCount);
            Assert.AreEqual(1, offer.SoldCount);
        }

        [Test]
        public void CanPurchase_WhenOfferIsNotInShop_ReturnsOfferNotAvailable()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase>());

            OperationResult result = shop.CanPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.OfferNotAvailable(), result);
        }

        [Test]
        public void CanPurchase_WhenCustomerIsNull_ReturnsCustomerIsNull()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });

            OperationResult result = shop.CanPurchase(offer);

            AssertSimilar(ShopOperations.CustomerIsNull(), result);
        }

        [Test]
        public void TryPurchase_WhenOfferRejects_FailsWithoutSideEffectsAndFiresFailureCallbacks()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            offer.RejectPurchase = true;
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });

            OperationResult result = shop.TryPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.Denied(), result);
            Assert.AreEqual(0, shop.PayCostsCount);
            Assert.AreEqual(0, shop.GrantReturnsCount);
            Assert.AreEqual(1, shop.PurchaseFailedCount);
            Assert.AreEqual(1, offer.PurchaseFailedCount);
            Assert.AreEqual(0, offer.PurchasedCount);
        }

        [Test]
        public void TryPurchase_WithInternalAction_SuppressesCallbacks()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });

            OperationResult result = shop.TryPurchase(
                offer,
                new TestCustomer(),
                actionSource: ActionSource.Internal);

            AssertSimilar(ShopOperations.TransactionCompleted(), result);
            Assert.AreEqual(0, shop.PurchaseCount);
            Assert.AreEqual(0, offer.PurchasedCount);
        }

        [Test]
        public void CanPurchase_WhenShopCannotPayCosts_ReturnsTransactionCostUnavailable()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });
            shop.RejectCanPayCosts = true;

            OperationResult result = shop.CanPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.TransactionCostUnavailable(), result);
            Assert.AreEqual(1, shop.CanPayCostsCount);
            Assert.AreEqual(0, shop.CanGrantReturnsCount);
        }

        [Test]
        public void CanPurchase_WithIgnoreTransactionConditions_BypassesStageChecks()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });
            shop.RejectCanPayCosts = true;
            shop.RejectCanGrantReturns = true;

            OperationResult result = shop.CanPurchase(
                offer,
                new TestCustomer(),
                ShopTransactionFlags.IgnoreTransactionConditions);

            AssertSimilar(ShopOperations.Permitted(), result);
            Assert.AreEqual(0, shop.CanPayCostsCount);
            Assert.AreEqual(0, shop.CanGrantReturnsCount);
        }

        [Test]
        public void TryPurchase_WhenGrantReturnsFails_RollsBackReturnsAndRefundsCosts()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });
            shop.FailGrantReturns = true;

            OperationResult result = shop.TryPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.TransactionReturnGrantFailed(), result);
            Assert.AreEqual(1, shop.PayCostsCount);
            Assert.AreEqual(1, shop.GrantReturnsCount);
            Assert.AreEqual(1, shop.RollbackReturnsCount);
            Assert.AreEqual(1, shop.RefundCostsCount);
            Assert.AreEqual(1, shop.PurchaseFailedCount);
        }

        [Test]
        public void TryPurchase_WhenRefundCostsFails_ReturnsRevertFailed()
        {
            TestPurchaseOffer offer = CreatePurchaseOffer();
            TestShop shop = CreateShop(new List<ShopOfferBase> { offer });
            shop.FailGrantReturns = true;
            shop.FailRefundCosts = true;

            OperationResult result = shop.TryPurchase(offer, new TestCustomer());

            AssertSimilar(ShopOperations.RevertFailed(), result);
            Assert.AreEqual(1, shop.RollbackReturnsCount);
            Assert.AreEqual(1, shop.RefundCostsCount);
            Assert.AreEqual(1, shop.PurchaseFailedCount);
        }
    }
}
