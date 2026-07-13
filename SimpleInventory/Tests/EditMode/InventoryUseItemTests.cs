using NUnit.Framework;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryUseItemTests : SimpleInventoryTestBase
    {
        [Test]
        public void UseItem_InvokesItemAndInventorySuccessCallbacks()
        {
            TestInventory inventory = CreateInventory();
            TestUsableItem item = CreateItem<TestUsableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.UsedSuccessfully(), inventory.UseItem(0));

            Assert.AreEqual(1, item.UsedCount);
            Assert.AreEqual(1, inventory.UsedCount);
            Assert.AreEqual(0, item.UseFailedCount);
            Assert.AreEqual(0, inventory.UseFailedCount);
        }

        [Test]
        public void UseItem_WhenItemRejectsUse_InvokesFailureCallbacks()
        {
            TestInventory inventory = CreateInventory();
            TestUsableItem item = CreateItem<TestUsableItem>();
            item.RejectUse = true;
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidAmount(), inventory.UseItem(0));

            Assert.AreEqual(0, item.UsedCount);
            Assert.AreEqual(1, item.UseFailedCount);
            Assert.AreEqual(0, inventory.UsedCount);
            Assert.AreEqual(1, inventory.UseFailedCount);
        }

        [Test]
        public void UseItem_FailureAlwaysInvokesCallbacks()
        {
            TestInventory inventory = CreateInventory();
            TestUsableItem item = CreateItem<TestUsableItem>();
            item.RejectUse = true;
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidAmount(), inventory.UseItem(0));

            Assert.AreEqual(1, item.UseFailedCount);
            Assert.AreEqual(1, inventory.UseFailedCount);
        }

        [Test]
        public void UseItem_ReturnsExpectedErrorsForInvalidEmptyAndNonUsableSlots()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidSlotIndex(), inventory.UseItem(-1));
            AssertSimilar(InventoryOperations.ItemNotUsable(), inventory.UseItem(0));
            AssertSimilar(InventoryOperations.SlotIsEmpty(), inventory.UseItem(1));
        }

        [Test]
        public void UseAnyAndBestItem_SelectExpectedUsableItems()
        {
            TestInventory inventory = CreateInventory(3);
            TestUsableItem item = CreateItem<TestUsableItem>();
            WorldItem worseWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem betterWorldItem = item.GenerateWorldItem(new ComparableItemData(9));
            inventory.TryAdd(worseWorldItem, 1, out _);
            inventory.TryAdd(betterWorldItem, 1, out _);

            AssertSimilar(InventoryOperations.UsedSuccessfully(), inventory.UseAnyItem<TestUsableItem>());
            AssertSimilar(InventoryOperations.UsedSuccessfully(), inventory.UseBestItem<TestUsableItem>());

            Assert.AreEqual(2, item.UsedCount);
            Assert.AreEqual(2, inventory.UsedCount);
        }
    }
}
