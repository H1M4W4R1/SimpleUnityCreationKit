using NUnit.Framework;
using Systems.SimpleLoot.Operations;

namespace Systems.SimpleLoot.Tests
{
    public sealed class LootOperationResultTests : SimpleLootTestBase
    {
        [Test]
        public void Factories_ReturnExpectedSystemAndResultCodes()
        {
            AssertSimilar(LootOperations.Permitted(), LootOperations.Permitted());
            Assert.AreEqual(LootOperations.SYSTEM_LOOT, LootOperations.LootGenerated().systemCode);
            Assert.AreEqual(LootOperations.SUCCESS_LOOT_GENERATED, LootOperations.LootGenerated().resultCode);
            Assert.AreEqual(LootOperations.ERROR_GENERATOR_NOT_FOUND, LootOperations.GeneratorNotFound().resultCode);
            Assert.AreEqual(LootOperations.ERROR_NO_VALID_ITEMS, LootOperations.NoValidItems().resultCode);
            Assert.AreEqual(
                LootOperations.ERROR_ITEM_CONDITION_FAILED,
                LootOperations.ItemConditionFailed().resultCode);
            Assert.IsFalse(LootOperations.Denied());
        }
    }
}
