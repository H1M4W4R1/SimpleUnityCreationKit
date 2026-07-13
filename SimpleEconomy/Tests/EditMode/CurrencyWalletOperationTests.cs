using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleEconomy.Data.Enums;
using Systems.SimpleEconomy.Operations;

namespace Systems.SimpleEconomy.Tests
{
    public sealed class CurrencyWalletOperationTests : SimpleEconomyTestBase
    {
        [Test]
        public void Has_ReturnsTrueForNonPositiveAndSufficientAmounts()
        {
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(25L);

            Assert.IsTrue(wallet.Has(0L));
            Assert.IsTrue(wallet.Has(-10L));
            Assert.IsTrue(wallet.Has(25L));
            Assert.IsFalse(wallet.Has(26L));
        }

        [Test]
        public void TryAdd_WithInvalidAmount_ReturnsInvalidAmountWithoutCallbacks()
        {
            CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();

            OperationResult result = wallet.TryAdd(0L);

            AssertSimilar(EconomyOperations.InvalidCurrencyAmount(), result);
            Assert.AreEqual(0L, wallet.Balance);
            Assert.AreEqual(0, wallet.AddCheckCount);
            Assert.AreEqual(0, wallet.AddedCount);
            Assert.AreEqual(0, wallet.AddFailedCount);
        }

        [Test]
        public void TryAdd_WhenPermitted_IncreasesBalanceAndFiresWalletAndCurrencyCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();

            OperationResult result = wallet.TryAdd(50L);

            AssertSimilar(EconomyOperations.CurrencyAdded(), result);
            Assert.AreEqual(50L, wallet.Balance);
            Assert.AreEqual(1, wallet.AddCheckCount);
            Assert.AreEqual(1, currency.AddCheckCount);
            Assert.AreEqual(1, wallet.AddedCount);
            Assert.AreEqual(1, currency.AddedCount);
            Assert.AreSame(wallet, currency.LastWallet);
            Assert.AreEqual(50L, wallet.LastAddAmount);
            Assert.AreEqual(50L, currency.LastAddAmount);
            Assert.AreEqual(0L, wallet.LastAmountLeft);
            Assert.AreEqual(EconomyOperations.SUCCESS_CURRENCY_ADDED, currency.LastResultCode);
        }

        [Test]
        public void TryAdd_WhenCurrencyRejects_FailsWithoutChangingBalance()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            currency.RejectAdd = true;
            TestWallet wallet = CreateWallet<TestWallet>();

            OperationResult result = wallet.TryAdd(10L);

            AssertSimilar(EconomyOperations.NotEnoughCurrency(), result);
            Assert.AreEqual(0L, wallet.Balance);
            Assert.AreEqual(1, wallet.AddFailedCount);
            Assert.AreEqual(1, currency.AddFailedCount);
            Assert.AreEqual(0, wallet.AddedCount);
        }

        [Test]
        public void TryAdd_WhenWalletRejects_ShortCircuitsCurrencyCheck()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.RejectAdd = true;

            OperationResult result = wallet.TryAdd(10L);

            AssertSimilar(EconomyOperations.NotEnoughCurrency(), result);
            Assert.AreEqual(0L, wallet.Balance);
            Assert.AreEqual(1, wallet.AddCheckCount);
            Assert.AreEqual(0, currency.AddCheckCount);
            Assert.AreEqual(1, wallet.AddFailedCount);
            Assert.AreEqual(1, currency.AddFailedCount);
        }

        [Test]
        public void TryAdd_WithIgnoreConditions_BypassesRejectedCurrency()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            currency.RejectAdd = true;
            TestWallet wallet = CreateWallet<TestWallet>();

            OperationResult result = wallet.TryAdd(15L, ModifyWalletCurrencyFlags.IgnoreConditions);

            AssertSimilar(EconomyOperations.CurrencyAdded(), result);
            Assert.AreEqual(15L, wallet.Balance);
            Assert.AreEqual(1, currency.AddCheckCount);
            Assert.AreEqual(1, wallet.AddedCount);
            Assert.AreEqual(1, currency.AddedCount);
            Assert.AreEqual(0, wallet.AddFailedCount);
        }

        [Test]
        public void TryAdd_WithInternalAction_MutatesAndSuppressesCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();

            OperationResult result = wallet.TryAdd(12L, actionSource: ActionSource.Internal);

            AssertSimilar(EconomyOperations.CurrencyAdded(), result);
            Assert.AreEqual(12L, wallet.Balance);
            Assert.AreEqual(1, wallet.AddCheckCount);
            Assert.AreEqual(1, currency.AddCheckCount);
            Assert.AreEqual(0, wallet.AddedCount);
            Assert.AreEqual(0, currency.AddedCount);
        }

        [Test]
        public void TryAdd_WhenOverflowWouldOccur_ReturnsOverflowAndFiresFailureCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(long.MaxValue - 2L);

            OperationResult result = wallet.TryAdd(3L);

            AssertSimilar(EconomyOperations.Overflow(), result);
            Assert.AreEqual(long.MaxValue - 2L, wallet.Balance);
            Assert.AreEqual(1, wallet.AddFailedCount);
            Assert.AreEqual(1, currency.AddFailedCount);
            Assert.AreEqual(EconomyOperations.ERROR_OVERFLOW, wallet.LastResultCode);
        }

        [Test]
        public void TryAdd_WithIgnoreBalanceLimits_AllowsOverflowWrap()
        {
            CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(long.MaxValue - 1L);
            long expectedBalance;
            unchecked
            {
                expectedBalance = (long.MaxValue - 1L) + 10L;
            }

            OperationResult result = wallet.TryAdd(10L, ModifyWalletCurrencyFlags.IgnoreBalanceLimits);

            AssertSimilar(EconomyOperations.CurrencyAdded(), result);
            Assert.AreEqual(expectedBalance, wallet.Balance);
        }

        [Test]
        public void TryTake_WithInvalidAmount_ReturnsInvalidAmountWithoutCallbacks()
        {
            CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(10L);

            OperationResult result = wallet.TryTake(-1L);

            AssertSimilar(EconomyOperations.InvalidCurrencyAmount(), result);
            Assert.AreEqual(10L, wallet.Balance);
            Assert.AreEqual(0, wallet.TakeCheckCount);
            Assert.AreEqual(0, wallet.TakenCount);
            Assert.AreEqual(0, wallet.TakeFailedCount);
        }

        [Test]
        public void TryTake_WhenPermitted_DecreasesBalanceAndFiresCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(40L);

            OperationResult result = wallet.TryTake(15L);

            AssertSimilar(EconomyOperations.CurrencyTaken(), result);
            Assert.AreEqual(25L, wallet.Balance);
            Assert.AreEqual(1, wallet.TakeCheckCount);
            Assert.AreEqual(1, currency.TakeCheckCount);
            Assert.AreEqual(1, wallet.TakenCount);
            Assert.AreEqual(1, currency.TakenCount);
            Assert.AreSame(wallet, currency.LastWallet);
            Assert.AreEqual(15L, wallet.LastTakeAmount);
            Assert.AreEqual(15L, currency.LastTakeAmount);
            Assert.AreEqual(0L, wallet.LastAmountLeft);
        }

        [Test]
        public void TryTake_WhenNotEnoughBalance_FailsWithoutChangingBalance()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(5L);

            OperationResult result = wallet.TryTake(6L);

            AssertSimilar(EconomyOperations.NotEnoughCurrency(), result);
            Assert.AreEqual(5L, wallet.Balance);
            Assert.AreEqual(0, wallet.TakeCheckCount);
            Assert.AreEqual(0, currency.TakeCheckCount);
            Assert.AreEqual(1, wallet.TakeFailedCount);
            Assert.AreEqual(1, currency.TakeFailedCount);
        }

        [Test]
        public void TryTake_WhenCurrencyRejects_FailsWithoutChangingBalance()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            currency.RejectTake = true;
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(20L);

            OperationResult result = wallet.TryTake(10L);

            AssertSimilar(EconomyOperations.NotEnoughCurrency(), result);
            Assert.AreEqual(20L, wallet.Balance);
            Assert.AreEqual(1, wallet.TakeFailedCount);
            Assert.AreEqual(1, currency.TakeFailedCount);
            Assert.AreEqual(0, wallet.TakenCount);
        }

        [Test]
        public void TryTake_WithIgnoreConditions_TakesPartialAmountWhenBalanceIsInsufficient()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            currency.RejectTake = true;
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(4L);

            OperationResult result = wallet.TryTake(10L, ModifyWalletCurrencyFlags.IgnoreConditions);

            AssertSimilar(EconomyOperations.CurrencyTakenPartial(), result);
            Assert.AreEqual(0L, wallet.Balance);
            Assert.AreEqual(1, currency.TakeCheckCount);
            Assert.AreEqual(1, wallet.TakenCount);
            Assert.AreEqual(1, currency.TakenCount);
            Assert.AreEqual(6L, wallet.LastAmountLeft);
        }

        [Test]
        public void TryTake_WithIgnoreConditionsAndNegativeBalance_DoesNotIncreaseBalance()
        {
            CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(-5L);

            OperationResult result = wallet.TryTake(10L, ModifyWalletCurrencyFlags.IgnoreConditions);

            AssertSimilar(EconomyOperations.CurrencyTakenPartial(), result);
            Assert.AreEqual(-5L, wallet.Balance);
            Assert.AreEqual(10L, wallet.LastAmountLeft);
        }

        [Test]
        public void TryTake_WithIgnoreBalanceLimits_AllowsNegativeBalance()
        {
            CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(3L);

            OperationResult result = wallet.TryTake(10L, ModifyWalletCurrencyFlags.IgnoreBalanceLimits);

            AssertSimilar(EconomyOperations.CurrencyTaken(), result);
            Assert.AreEqual(-7L, wallet.Balance);
            Assert.AreEqual(0L, wallet.LastAmountLeft);
        }

        [Test]
        public void TryTake_WhenUnderflowWouldOccur_ReturnsOverflowAndFiresFailureCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(long.MinValue + 1L);

            OperationResult result = wallet.TryTake(2L, ModifyWalletCurrencyFlags.IgnoreBalanceLimits);

            AssertSimilar(EconomyOperations.Overflow(), result);
            Assert.AreEqual(long.MinValue + 1L, wallet.Balance);
            Assert.AreEqual(1, wallet.TakeFailedCount);
            Assert.AreEqual(1, currency.TakeFailedCount);
            Assert.AreEqual(EconomyOperations.ERROR_OVERFLOW, wallet.LastResultCode);
        }

        [Test]
        public void TryTake_WithInternalAction_MutatesAndSuppressesCallbacks()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            TestWallet wallet = CreateWallet<TestWallet>();
            wallet.SetBalanceForTests(20L);

            OperationResult result = wallet.TryTake(5L, actionSource: ActionSource.Internal);

            AssertSimilar(EconomyOperations.CurrencyTaken(), result);
            Assert.AreEqual(15L, wallet.Balance);
            Assert.AreEqual(1, wallet.TakeCheckCount);
            Assert.AreEqual(1, currency.TakeCheckCount);
            Assert.AreEqual(0, wallet.TakenCount);
            Assert.AreEqual(0, currency.TakenCount);
        }
    }
}
