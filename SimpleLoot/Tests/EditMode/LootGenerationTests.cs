using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Utility;
using UnityEngine;
using UnityEngine.TestTools;

namespace Systems.SimpleLoot.Tests
{
    public sealed class LootGenerationTests : SimpleLootTestBase
    {
        [Test]
        public void LootAPI_WhenGeneratorIsMissing_ReturnsEmptyListAndLogsError()
        {
            TestLootItem item = CreateItem("Only Item", 1f);
            TestLootTable table = CreateTable((item, null));

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("TestWeightedGenerator.*not found"));

            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 3);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void EqualGenerator_WhenOnlyOneItemAllowed_ReturnsThatItemForEachBudgetRoll()
        {
            TestEqualGenerator generator = CreateRegisteredGenerator<TestEqualGenerator>();
            TestLootItem blocked = CreateItem("Blocked", 1f, false);
            TestLootItem allowed = CreateItem("Allowed", 1f);
            TestLootTable table = CreateTable((blocked, null), (allowed, null));

            Random.InitState(17);
            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestEqualGenerator, TestLootItem>(table, 5);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(5, drops.Count);
            for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
            {
                Assert.AreSame(allowed, drops[dropIndex]);
            }

            Assert.AreEqual(1, generator.GeneratedCount);
            Assert.AreEqual(0, generator.FailedCount);
            Assert.AreEqual(5, generator.LastLootCount);
            Assert.AreEqual(5, generator.LastBudget);
        }

        [Test]
        public void EqualGenerator_WhenIgnoreConditionsIsSet_CanReturnBlockedEntries()
        {
            TestEqualGenerator generator = CreateRegisteredGenerator<TestEqualGenerator>();
            TestLootItem blocked = CreateItem("Blocked", 1f, false);
            TestLootTable table = CreateTable((blocked, null));

            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestEqualGenerator, TestLootItem>(
                    table, 2, LootGenerationFlags.IgnoreConditions);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(2, drops.Count);
            Assert.AreSame(blocked, drops[0]);
            Assert.AreSame(blocked, drops[1]);
            Assert.AreEqual(LootGenerationFlags.IgnoreConditions, generator.LastFlags);
        }

        [Test]
        public void EqualGenerator_WhenTableEmptyBudgetInvalidOrAllBlocked_FailsWithoutDrops()
        {
            TestEqualGenerator generator = CreateRegisteredGenerator<TestEqualGenerator>();
            TestLootItem blocked = CreateItem("Blocked", 1f, false);
            TestLootTable emptyTable = CreateTable();
            TestLootTable blockedTable = CreateTable((blocked, null));

            List<TestLootItem> emptyDrops =
                CopyAndRelease(LootAPI.GenerateLoot<TestEqualGenerator, TestLootItem>(emptyTable, 1));
            List<TestLootItem> zeroBudgetDrops =
                CopyAndRelease(LootAPI.GenerateLoot<TestEqualGenerator, TestLootItem>(blockedTable, 0));
            List<TestLootItem> blockedDrops =
                CopyAndRelease(LootAPI.GenerateLoot<TestEqualGenerator, TestLootItem>(blockedTable, 1));

            Assert.AreEqual(0, emptyDrops.Count);
            Assert.AreEqual(0, zeroBudgetDrops.Count);
            Assert.AreEqual(0, blockedDrops.Count);
            Assert.AreEqual(3, generator.FailedCount);
            Assert.AreEqual(0, generator.GeneratedCount);
        }

        [Test]
        public void WeightedGenerator_UsesRarityOverrideBeforeItemChance()
        {
            TestWeightedGenerator generator = CreateRegisteredGenerator<TestWeightedGenerator>();
            TestLootItem overrideItem = CreateItem("Override Item", 0f);
            TestRarity rarity = CreateRarity(100f);
            TestLootTable table = CreateTable((overrideItem, rarity));

            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 4);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(4, drops.Count);
            for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
            {
                Assert.AreSame(overrideItem, drops[dropIndex]);
            }

            Assert.AreEqual(1, generator.GeneratedCount);
            Assert.AreEqual(4, generator.LastLootCount);
        }

        [Test]
        public void WeightedGenerator_UsesItemRarityWhenNoDirectChanceExists()
        {
            TestWeightedGenerator generator = CreateRegisteredGenerator<TestWeightedGenerator>();
            TestLootItem item = CreateItem("Rarity Item", 0f);
            item.RarityValue = CreateRarity(50f);
            TestLootTable table = CreateTable((item, null));

            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 3);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(3, drops.Count);
            for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
            {
                Assert.AreSame(item, drops[dropIndex]);
            }
        }

        [Test]
        public void WeightedGenerator_WhenTotalWeightIsZero_FallsBackToEqualWeights()
        {
            CreateRegisteredGenerator<TestWeightedGenerator>();
            TestLootItem first = CreateItem("First", 0f);
            TestLootItem second = CreateItem("Second", 0f);
            TestLootTable table = CreateTable((first, null), (second, null));

            Random.InitState(42);
            ROListAccess<TestLootItem> access =
                LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 20);
            List<TestLootItem> drops = CopyAndRelease(access);

            Assert.AreEqual(20, drops.Count);
            for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
            {
                bool isKnownItem = ReferenceEquals(drops[dropIndex], first) ||
                                   ReferenceEquals(drops[dropIndex], second);
                Assert.IsTrue(isKnownItem);
            }
        }

        [Test]
        public void WeightedGenerator_WhenAllEntriesBlocked_FailsUnlessConditionsAreIgnored()
        {
            TestWeightedGenerator generator = CreateRegisteredGenerator<TestWeightedGenerator>();
            TestLootItem blocked = CreateItem("Blocked", 10f, false);
            TestLootTable table = CreateTable((blocked, null));

            List<TestLootItem> normalDrops =
                CopyAndRelease(LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 1));
            List<TestLootItem> ignoredDrops =
                CopyAndRelease(LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(
                    table, 1, LootGenerationFlags.IgnoreConditions));

            Assert.AreEqual(0, normalDrops.Count);
            Assert.AreEqual(1, ignoredDrops.Count);
            Assert.AreSame(blocked, ignoredDrops[0]);
            Assert.AreEqual(1, generator.FailedCount);
            Assert.AreEqual(1, generator.GeneratedCount);
        }

        [Test]
        public void WeightedGenerator_IgnoresInvalidWeights()
        {
            TestWeightedGenerator generator = CreateRegisteredGenerator<TestWeightedGenerator>();
            TestLootItem negative = CreateItem("Negative", -1f);
            TestLootItem infinite = CreateItem("Infinite", float.PositiveInfinity);
            TestLootItem nan = CreateItem("NaN", float.NaN);
            TestLootItem valid = CreateItem("Valid", 1f);
            TestLootTable table = CreateTable((negative, null), (infinite, null), (nan, null), (valid, null));

            List<TestLootItem> drops = CopyAndRelease(
                LootAPI.GenerateLoot<TestWeightedGenerator, TestLootItem>(table, 5));

            Assert.AreEqual(5, drops.Count);
            for (int dropIndex = 0; dropIndex < drops.Count; dropIndex++)
                Assert.AreSame(valid, drops[dropIndex]);
            Assert.AreEqual(1, generator.GeneratedCount);
        }
    }
}
