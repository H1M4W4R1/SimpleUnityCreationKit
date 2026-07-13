using NUnit.Framework;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryPickupAndDropTests : SimpleInventoryTestBase
    {
        [Test]
        public void TryDropItemAs_TakesItemsAndSpawnsConfiguredPickup()
        {
            TestInventory inventory = CreateInventory();
            TestItem item = CreateItem<TestItem>(5, true);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 4, out _);

            AssertSimilar(InventoryOperations.ItemsDropped(), inventory.TryDropItemAs<TestPickupItem>(0, 2));

            TestPickupItem pickup = FindSinglePickup();
            Assert.AreSame(worldItem, pickup.ItemInstance);
            Assert.AreEqual(2, pickup.Amount);
            Assert.AreEqual(2, inventory.Count(worldItem));
            Assert.AreSame(inventory.transform, pickup.transform.parent);
            Assert.AreEqual(1, inventory.DroppedCount);
        }

        [Test]
        public void TryDropItemAs_ReturnsExpectedErrorsForInvalidEmptyAndUnavailableAmounts()
        {
            TestInventory inventory = CreateInventory(2);
            TestItem item = CreateItem<TestItem>(5, true);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidSlotIndex(), inventory.TryDropItemAs<TestPickupItem>(-1, 1));
            AssertSimilar(InventoryOperations.SlotIsEmpty(), inventory.TryDropItemAs<TestPickupItem>(1, 1));
            AssertSimilar(InventoryOperations.NotEnoughItems(), inventory.TryDropItemAs<TestPickupItem>(0, 2));
            Assert.AreEqual(1, inventory.DropFailedCount);
        }

        [Test]
        public void Pickup_AddsItemsToInventoryAndUpdatesPickupAmount()
        {
            TestInventory sourceInventory = CreateInventory();
            TestInventory targetInventory = CreateInventory();
            TestItem item = CreateItem<TestItem>(5, true);
            WorldItem worldItem = item.GenerateWorldItem(null);
            sourceInventory.TryAdd(worldItem, 2, out _);
            sourceInventory.TryDropItemAs<TestPickupItem>(0, 2);
            TestPickupItem pickup = FindSinglePickup();

            pickup.Pickup(targetInventory);

            AssertSimilar(InventoryOperations.ItemsPickedUp(), pickup);
            Assert.AreEqual(1, pickup.CompletionCount);
            Assert.AreEqual(0, pickup.Amount);
            Assert.AreEqual(0, pickup.LastAmountLeft);
            Assert.AreEqual(2, targetInventory.Count(worldItem));
            Assert.AreEqual(1, targetInventory.PickupCount);
        }

        [Test]
        public void Pickup_UsesTryPickupValidationBeforeAddingItems()
        {
            TestInventory sourceInventory = CreateInventory();
            TestInventory targetInventory = CreateInventory();
            targetInventory.RejectPickup = true;
            TestItem item = CreateItem<TestItem>(5, true);
            WorldItem worldItem = item.GenerateWorldItem(null);
            sourceInventory.TryAdd(worldItem, 1, out _);
            sourceInventory.TryDropItemAs<TestPickupItem>(0, 1);
            TestPickupItem pickup = FindSinglePickup();

            pickup.Pickup(targetInventory);

            AssertSimilar(InventoryOperations.InvalidAmount(), pickup);
            Assert.AreEqual(1, pickup.CompletionCount);
            Assert.AreEqual(1, pickup.Amount);
            Assert.AreEqual(1, pickup.LastAmountLeft);
            Assert.AreEqual(0, targetInventory.Count(worldItem));
            Assert.AreEqual(1, targetInventory.PickupFailedCount);
        }

        [Test]
        public void TryPickupItem_WhenInventoryCannotStoreAmount_LeavesPickupUntouched()
        {
            TestInventory sourceInventory = CreateInventory();
            TestInventory targetInventory = CreateInventory(1);
            TestItem item = CreateItem<TestItem>(1, true);
            WorldItem worldItem = item.GenerateWorldItem(null);
            WorldItem blockingWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            targetInventory.TryAdd(blockingWorldItem, 1, out _);
            sourceInventory.TryAdd(worldItem, 1, out _);
            sourceInventory.TryDropItemAs<TestPickupItem>(0, 1);
            TestPickupItem pickup = FindSinglePickup();

            AssertSimilar(
                InventoryOperations.NotEnoughSpace(),
                targetInventory.TryPickupItem(pickup, pickup.Amount, out int amountLeft));

            Assert.AreEqual(1, amountLeft);
            Assert.AreEqual(0, targetInventory.Count(worldItem));
            Assert.AreEqual(1, pickup.Amount);
            Assert.AreEqual(1, targetInventory.PickupFailedCount);
        }

        [Test]
        public void TryAddOrDrop_WhenInventoryIsFull_SpawnsLeftoverWithoutRemovingStoredItems()
        {
            TestInventory inventory = CreateInventory(1);
            TestItem item = CreateItem<TestItem>(1, true);
            WorldItem storedWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem leftoverWorldItem = item.GenerateWorldItem(new ComparableItemData(2));
            inventory.TryAdd(storedWorldItem, 1, out _);

            inventory.TryAddOrDrop(leftoverWorldItem, 1, out int amountLeft);

            PickupItemWithDestroy[] pickups = Object.FindObjectsByType<PickupItemWithDestroy>(
                FindObjectsInactive.Include);
            Assert.AreEqual(1, pickups.Length);
            Track(pickups[0].gameObject);
            Assert.AreEqual(1, amountLeft);
            Assert.AreEqual(1, inventory.Count(storedWorldItem));
            Assert.AreEqual(0, inventory.Count(leftoverWorldItem));
            Assert.AreSame(leftoverWorldItem, pickups[0].ItemInstance);
            Assert.AreEqual(1, pickups[0].Amount);
            Assert.AreEqual(1, inventory.DroppedCount);
        }
    }
}
