using NUnit.Framework;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryTransferTests : SimpleInventoryTestBase
    {
        [Test]
        public void TryTransferItem_ToEmptySlotMovesStackWithinInventory()
        {
            TestInventory inventory = CreateInventory(3);
            TestItem item = CreateItem<TestItem>(5);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 3, out _);

            AssertSimilar(
                InventoryOperations.ItemsTransferred(),
                inventory.TryTransferItem(0, inventory, 2));

            Assert.IsNull(inventory.SlotAt(0).Item);
            Assert.AreSame(worldItem, inventory.SlotAt(2).Item);
            Assert.AreEqual(3, inventory.SlotAt(2).Amount);
            Assert.AreEqual(2, inventory.TransferCount);
        }

        [Test]
        public void TryTransferItem_SameItemWithoutPartialFlagRejectsOverflow()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(5);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 8, out _);
            inventory.TryTake(worldItem, 2, out _);

            AssertSimilar(
                InventoryOperations.NotEnoughSpace(),
                inventory.TryTransferItem(1, inventory, 0));

            Assert.AreEqual(3, inventory.SlotAt(0).Amount);
            Assert.AreEqual(3, inventory.SlotAt(1).Amount);
            Assert.AreEqual(1, inventory.TransferFailedCount);
        }

        [Test]
        public void TryTransferItem_WithPartialFlagCombinesUntilTargetIsFull()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(5);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 8, out _);
            inventory.TryTake(worldItem, 2, out _);

            AssertSimilar(
                InventoryOperations.ItemsTransferred(),
                inventory.TryTransferItem(1, inventory, 0, ItemTransferFlags.AllowPartialTransfer));

            Assert.AreEqual(5, inventory.SlotAt(0).Amount);
            Assert.AreEqual(1, inventory.SlotAt(1).Amount);
        }

        [Test]
        public void TryTransferItem_WithSwapIfOccupiedBySameSwapsSameItemStacks()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(5);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 8, out _);

            AssertSimilar(
                InventoryOperations.ItemsTransferred(),
                inventory.TryTransferItem(1, inventory, 0, ItemTransferFlags.SwapIfOccupiedBySame));

            Assert.AreEqual(3, inventory.SlotAt(0).Amount);
            Assert.AreEqual(5, inventory.SlotAt(1).Amount);
        }

        [Test]
        public void TryTransferItem_DifferentItemsSwapBetweenInventories()
        {
            TestInventory sourceInventory = CreateInventory(1);
            TestInventory targetInventory = CreateInventory(1);
            TestItem item = CreateItem<TestItem>();
            TestOtherItem otherItem = CreateItem<TestOtherItem>();
            WorldItem sourceWorldItem = item.GenerateWorldItem(null);
            WorldItem targetWorldItem = otherItem.GenerateWorldItem(null);
            sourceInventory.TryAdd(sourceWorldItem, 1, out _);
            targetInventory.TryAdd(targetWorldItem, 1, out _);

            AssertSimilar(
                InventoryOperations.ItemsTransferred(),
                sourceInventory.TryTransferItem(0, targetInventory, 0));

            Assert.AreSame(targetWorldItem, sourceInventory.SlotAt(0).Item);
            Assert.AreSame(sourceWorldItem, targetInventory.SlotAt(0).Item);
            Assert.AreEqual(1, sourceInventory.TransferCount);
            Assert.AreEqual(1, targetInventory.TransferCount);
        }

        [Test]
        public void TryTransferItems_MovesRequestedAmountBetweenInventories()
        {
            TestInventory sourceInventory = CreateInventory(3);
            TestInventory targetInventory = CreateInventory(3);
            TestItem item = CreateItem<TestItem>(5);
            WorldItem worldItem = item.GenerateWorldItem(null);
            sourceInventory.TryAdd(worldItem, 5, out _);

            AssertSimilar(
                InventoryOperations.ItemsTransferred(),
                sourceInventory.TryTransferItems(targetInventory, worldItem, 3));

            Assert.AreEqual(2, sourceInventory.Count(worldItem));
            Assert.AreEqual(3, targetInventory.Count(worldItem));
            Assert.AreEqual(1, sourceInventory.TransferCount);
            Assert.AreEqual(1, targetInventory.TransferCount);
        }

        [Test]
        public void TryTransferItems_WhenTargetCannotStoreAmount_RollsBackSource()
        {
            TestInventory sourceInventory = CreateInventory(2);
            TestInventory targetInventory = CreateInventory(1);
            TestItem item = CreateItem<TestItem>(2);
            WorldItem worldItem = item.GenerateWorldItem(null);
            sourceInventory.TryAdd(worldItem, 4, out _);
            targetInventory.TryAdd(worldItem, 2, out _);

            AssertSimilar(
                InventoryOperations.NotEnoughSpace(),
                sourceInventory.TryTransferItems(targetInventory, worldItem, 1));

            Assert.AreEqual(4, sourceInventory.Count(worldItem));
            Assert.AreEqual(2, targetInventory.Count(worldItem));
        }

        [Test]
        public void TryTransferItem_InvalidSlotsReturnInvalidSlotIndex()
        {
            TestInventory inventory = CreateInventory(1);

            AssertSimilar(
                InventoryOperations.InvalidSlotIndex(),
                inventory.TryTransferItem(-1, inventory, 0));
            AssertSimilar(
                InventoryOperations.InvalidSlotIndex(),
                inventory.TryTransferItem(0, inventory, 4));
        }
    }
}
