using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleEconomy.Data;
using Systems.SimpleEconomy.Operations;

namespace Systems.SimpleEconomy.Tests
{
    public sealed class EconomyOperationResultTests : SimpleEconomyTestBase
    {
        [Test]
        public void EconomyFactories_UseEconomySystemCodes()
        {
            OperationResult added = EconomyOperations.CurrencyAdded();
            OperationResult notEnough = EconomyOperations.NotEnoughCurrency();
            OperationResult permitted = EconomyOperations.Permitted();

            Assert.IsTrue(OperationResult.IsSuccess(added));
            Assert.AreEqual(EconomyOperations.SYSTEM_ECONOMY, added.systemCode);
            Assert.AreEqual(EconomyOperations.SUCCESS_CURRENCY_ADDED, added.resultCode);

            Assert.IsTrue(OperationResult.IsError(notEnough));
            Assert.IsTrue(OperationResult.IsFromSystem(notEnough, EconomyOperations.SYSTEM_ECONOMY));
            Assert.AreEqual(EconomyOperations.ERROR_NOT_ENOUGH_CURRENCY, notEnough.resultCode);

            Assert.IsTrue(OperationResult.IsSuccess(permitted));
            Assert.AreEqual(OperationResult.SUCCESS_PERMITTED, permitted.resultCode);
        }

        [Test]
        public void CurrencyDatabase_ReturnsRegisteredCurrenciesByExactType()
        {
            TestCurrency currency = CreateRegisteredCurrency<TestCurrency>();
            OtherTestCurrency otherCurrency = CreateRegisteredCurrency<OtherTestCurrency>();

            TestCurrency foundCurrency = CurrencyDatabase.GetExact<TestCurrency>();
            OtherTestCurrency foundOtherCurrency = CurrencyDatabase.GetExact<OtherTestCurrency>();

            Assert.AreSame(currency, foundCurrency);
            Assert.AreSame(otherCurrency, foundOtherCurrency);
            Assert.AreEqual(2, CurrencyDatabase.Count);
        }

        [Test]
        public void CurrencyDatabase_ClearForTests_RemovesRegisteredCurrencies()
        {
            CreateRegisteredCurrency<TestCurrency>();

            CurrencyDatabase.ClearForTests();

            Assert.IsTrue(ReferenceEquals(CurrencyDatabase.GetExact<TestCurrency>(), null));
            Assert.AreEqual(0, CurrencyDatabase.Count);
        }
    }
}
