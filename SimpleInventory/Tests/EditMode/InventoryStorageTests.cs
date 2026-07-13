using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryStorageTests : SimpleInventoryTestBase
    {
        [Test]
        public void TryAdd_SplitsAmountAcrossStacksAndReportsFreeSpace()
        {
            TestInventory inventory = CreateInventory(3);
            TestItem item = CreateItem<TestItem>(3);
            WorldItem worldItem = item.GenerateWorldItem(null);

            OperationResult result = inventory.TryAdd(worldItem, 8, out int amountLeft);

            AssertSimilar(InventoryOperations.ItemsAdded(), result);
            Assert.AreEqual(0, amountLeft);
            Assert.AreEqual(8, inventory.Count(worldItem));
            Assert.AreEqual(3, inventory.SlotAt(0).Amount);
            Assert.AreEqual(3, inventory.SlotAt(1).Amount);
            Assert.AreEqual(2, inventory.SlotAt(2).Amount);
            Assert.AreEqual(1, inventory.GetFreeSpaceFor(worldItem));
            Assert.AreEqual(1, inventory.AddedCount);
        }

        [Test]
        public void TryAdd_WhenInventoryCannotFitWholeAmount_DoesNotPartiallyAdd()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(2);
            WorldItem worldItem = item.GenerateWorldItem(null);

            OperationResult result = inventory.TryAdd(worldItem, 5, out int amountLeft);

            AssertSimilar(InventoryOperations.NotEnoughSpace(), result);
            Assert.AreEqual(5, amountLeft);
            Assert.AreEqual(0, inventory.Count(worldItem));
            Assert.AreEqual(1, inventory.AddFailedCount);
        }

        [Test]
        public void TryAdd_WithInvalidAmount_ReturnsInvalidAmountWithoutCallback()
        {
            TestInventory inventory = CreateInventory();
            TestItem item = CreateItem<TestItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);

            OperationResult result = inventory.TryAdd(worldItem, 0, out int amountLeft);

            AssertSimilar(InventoryOperations.InvalidAmount(), result);
            Assert.AreEqual(0, amountLeft);
            Assert.AreEqual(0, inventory.AddedCount);
            Assert.AreEqual(0, inventory.AddFailedCount);
        }

        [Test]
        public void TryTake_RemovesAcrossMultipleSlotsAndClearsEmptySlots()
        {
            TestInventory inventory = CreateInventory(3);
            TestItem item = CreateItem<TestItem>(3);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 7, out _);

            OperationResult result = inventory.TryTake(worldItem, 5, out int amountLeft);

            AssertSimilar(InventoryOperations.ItemsTaken(), result);
            Assert.AreEqual(0, amountLeft);
            Assert.AreEqual(2, inventory.Count(worldItem));
            Assert.IsNull(inventory.SlotAt(0).Item);
            Assert.AreEqual(0, inventory.SlotAt(0).Amount);
            Assert.AreSame(worldItem, inventory.SlotAt(1).Item);
            Assert.AreEqual(1, inventory.SlotAt(1).Amount);
            Assert.AreEqual(1, inventory.TakenCount);
        }

        [Test]
        public void TryTake_WhenNotEnoughItems_FailsWithoutMutatingInventory()
        {
            TestInventory inventory = CreateInventory();
            TestItem item = CreateItem<TestItem>(3);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 2, out _);

            OperationResult result = inventory.TryTake(worldItem, 3, out int amountLeft);

            AssertSimilar(InventoryOperations.NotEnoughItems(), result);
            Assert.AreEqual(3, amountLeft);
            Assert.AreEqual(2, inventory.Count(worldItem));
            Assert.AreEqual(1, inventory.TakeFailedCount);
        }

        [Test]
        public void TryTake_WithInternalAction_SuppressesCallbacks()
        {
            TestInventory inventory = CreateInventory();
            TestItem item = CreateItem<TestItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _, ActionSource.Internal);

            OperationResult result = inventory.TryTake(worldItem, 1, out int amountLeft, ActionSource.Internal);

            AssertSimilar(InventoryOperations.ItemsTaken(), result);
            Assert.AreEqual(0, amountLeft);
            Assert.AreEqual(0, inventory.AddedCount);
            Assert.AreEqual(0, inventory.TakenCount);
        }

        [Test]
        public void TryTake_WorldItemUsesSameComparableIdentityAsCount()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(2);
            WorldItem storedItem = item.GenerateWorldItem(new ComparableItemData(10));
            WorldItem equivalentItem = item.GenerateWorldItem(new ComparableItemData(10));
            inventory.TryAdd(storedItem, 2, out _);

            OperationResult result = inventory.TryTake(equivalentItem, 1, out int amountLeft);

            AssertSimilar(InventoryOperations.ItemsTaken(), result);
            Assert.AreEqual(0, amountLeft);
            Assert.AreEqual(1, inventory.Count(storedItem));
        }

        [Test]
        public void ItemAccess_ReturnsFirstAllAndBestMatchingItems()
        {
            TestInventory inventory = CreateInventory(4);
            TestItem item = CreateItem<TestItem>(1);
            TestOtherItem otherItem = CreateItem<TestOtherItem>(1);
            WorldItem worseWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem betterWorldItem = item.GenerateWorldItem(new ComparableItemData(5));
            WorldItem otherWorldItem = otherItem.GenerateWorldItem(null);
            inventory.TryAdd(otherWorldItem, 1, out _);
            inventory.TryAdd(worseWorldItem, 1, out _);
            inventory.TryAdd(betterWorldItem, 1, out _);

            InventoryItemReference firstItem = inventory.GetFirstItemOfType<TestItem>();
            IReadOnlyList<InventoryItemReference> allItems = inventory.GetAllItemsOfType<TestItem>();
            InventoryItemReference? bestItem = inventory.GetBestItem<TestItem>();

            Assert.AreEqual(1, firstItem.slotIndex);
            Assert.AreSame(worseWorldItem, firstItem.item);
            Assert.AreEqual(2, allItems.Count);
            Assert.IsTrue(bestItem.HasValue);
            Assert.AreSame(betterWorldItem, bestItem.Value.item);
        }

        [Test]
        public void InventorySlotSwap_ExchangesItemsAndAmounts()
        {
            TestItem item = CreateItem<TestItem>();
            TestOtherItem otherItem = CreateItem<TestOtherItem>();
            WorldItem firstWorldItem = item.GenerateWorldItem(null);
            WorldItem secondWorldItem = otherItem.GenerateWorldItem(null);
            TestInventory inventory = CreateInventory(2);
            inventory.TryAdd(firstWorldItem, 1, out _);
            inventory.TryAdd(secondWorldItem, 1, out _);
            InventorySlot firstSlot = inventory.SlotAt(0);
            InventorySlot secondSlot = inventory.SlotAt(1);

            InventorySlot.Swap(firstSlot, secondSlot);

            Assert.AreSame(secondWorldItem, firstSlot.Item);
            Assert.AreSame(firstWorldItem, secondSlot.Item);
        }
    }
}
