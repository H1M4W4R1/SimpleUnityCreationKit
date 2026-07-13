using NUnit.Framework;

namespace Systems.SimpleEconomy.Tests
{
    public sealed class GlobalCurrencyWalletTests : SimpleEconomyTestBase
    {
        [Test]
        public void Instance_WhenWalletExistsInScene_ReturnsExistingWallet()
        {
            TestGlobalWallet wallet = CreateWallet<TestGlobalWallet>();

            TestGlobalWallet instance = TestGlobalWallet.Instance;

            Assert.AreSame(wallet, instance);
        }

        [Test]
        public void Instance_WhenWalletDoesNotExist_CreatesWallet()
        {
            TestGlobalWallet instance = TestGlobalWallet.Instance;
            Track(instance.gameObject);

            Assert.IsFalse(ReferenceEquals(instance, null));
            Assert.IsTrue(instance);
            Assert.IsTrue(instance.gameObject.name.Contains(nameof(TestCurrency)));
        }
    }
}
