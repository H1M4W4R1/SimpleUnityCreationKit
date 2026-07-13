using NUnit.Framework;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryEquipmentTests : SimpleInventoryTestBase
    {
        [Test]
        public void EquipItem_MovesItemFromInventoryToEquipment()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(EquipmentOperations.Equipped(), inventory.EquipItem(0, equipment));

            Assert.IsTrue(equipment.IsEquipped(worldItem));
            Assert.IsNull(inventory.SlotAt(0).Item);
            Assert.AreEqual(1, item.EquippedCount);
        }

        [Test]
        public void EquipItem_FromStackOnlyRemovesEquippedItem()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>(3);
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 2, out _);

            AssertSimilar(EquipmentOperations.Equipped(), inventory.EquipItem(0, equipment));

            Assert.IsTrue(equipment.IsEquipped(worldItem));
            Assert.AreSame(worldItem, inventory.SlotAt(0).Item);
            Assert.AreEqual(1, inventory.SlotAt(0).Amount);
        }

        [Test]
        public void EquipItem_WhenItemRejectsEquip_FailsAndKeepsInventory()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            item.RejectEquip = true;
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidAmount(), inventory.EquipItem(0, equipment));

            Assert.IsFalse(equipment.IsEquipped(worldItem));
            Assert.AreSame(worldItem, inventory.SlotAt(0).Item);
            Assert.AreEqual(1, item.EquipFailedCount);
        }

        [Test]
        public void EquipItem_WithIgnoreConditions_EquipsRejectedItem()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            item.RejectEquip = true;
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(
                EquipmentOperations.Equipped(),
                inventory.EquipItem(0, equipment, EquipmentModificationFlags.IgnoreConditions));

            Assert.IsTrue(equipment.IsEquipped(worldItem));
            Assert.AreEqual(1, item.EquippedCount);
            Assert.AreEqual(0, item.EquipFailedCount);
        }

        [Test]
        public void EquipItem_WithoutSwapFlagFailsWhenMatchingSlotIsOccupied()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem firstWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem secondWorldItem = item.GenerateWorldItem(new ComparableItemData(2));
            inventory.TryAdd(firstWorldItem, 1, out _);
            inventory.TryAdd(secondWorldItem, 1, out _);
            inventory.EquipItem(0, equipment);

            AssertSimilar(
                EquipmentOperations.NoFreeSlots(),
                inventory.EquipItem(1, equipment, EquipmentModificationFlags.None));

            Assert.IsTrue(equipment.IsEquipped(firstWorldItem));
            Assert.AreSame(secondWorldItem, inventory.SlotAt(1).Item);
            Assert.AreEqual(1, item.EquipFailedCount);
        }

        [Test]
        public void EquipItem_WithSwapFlagReturnsPreviousItemToInventory()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem firstWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem secondWorldItem = item.GenerateWorldItem(new ComparableItemData(2));
            inventory.TryAdd(firstWorldItem, 1, out _);
            inventory.TryAdd(secondWorldItem, 1, out _);
            inventory.EquipItem(0, equipment);

            AssertSimilar(
                EquipmentOperations.Equipped(),
                inventory.EquipItem(1, equipment, EquipmentModificationFlags.AllowItemSwap));

            Assert.IsFalse(equipment.IsEquipped(firstWorldItem));
            Assert.IsTrue(equipment.IsEquipped(secondWorldItem));
            Assert.AreSame(firstWorldItem, inventory.SlotAt(0).Item);
            Assert.IsNull(inventory.SlotAt(1).Item);
            Assert.AreEqual(2, item.EquippedCount);
            Assert.AreEqual(1, item.UnequippedCount);
        }

        [Test]
        public void UnequipItem_ReturnsEquippedItemToInventory()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);
            inventory.EquipItem(0, equipment);

            AssertSimilar(EquipmentOperations.Unequipped(), inventory.UnequipItem(worldItem, equipment));

            Assert.IsFalse(equipment.IsEquipped(worldItem));
            Assert.AreSame(worldItem, inventory.SlotAt(0).Item);
            Assert.AreEqual(1, item.UnequippedCount);
        }

        [Test]
        public void UnequipItem_WhenItemRejectsUnequip_FailsUnlessIgnored()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);
            inventory.EquipItem(0, equipment);
            item.RejectUnequip = true;

            AssertSimilar(InventoryOperations.InvalidAmount(), inventory.UnequipItem(worldItem, equipment));
            Assert.IsTrue(equipment.IsEquipped(worldItem));
            Assert.AreEqual(1, item.UnequipFailedCount);

            AssertSimilar(
                EquipmentOperations.Unequipped(),
                inventory.UnequipItem(worldItem, equipment, EquipmentModificationFlags.IgnoreConditions));
            Assert.IsFalse(equipment.IsEquipped(worldItem));
            Assert.AreEqual(1, item.UnequippedCount);
        }

        [Test]
        public void EquipItem_ReturnsInventoryErrorsForInvalidEmptyAndNonEquippableSlots()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestItem item = CreateItem<TestItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(InventoryOperations.InvalidSlotIndex(), inventory.EquipItem(-1, equipment));
            AssertSimilar(InventoryOperations.ItemNotEquippable(), inventory.EquipItem(0, equipment));
            AssertSimilar(InventoryOperations.SlotIsEmpty(), inventory.EquipItem(1, equipment));
        }

        [Test]
        public void EquipAnyAndUnequipAny_UseFirstMatchingItemType()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(EquipmentOperations.Equipped(), inventory.EquipAnyItem<TestEquippableItem>(equipment));
            Assert.IsTrue(equipment.IsEquipped(worldItem));

            AssertSimilar(EquipmentOperations.Unequipped(), inventory.UnequipAnyItem<TestEquippableItem>(equipment));
            Assert.IsFalse(equipment.IsEquipped(worldItem));
            Assert.AreSame(worldItem, inventory.SlotAt(0).Item);
        }

        [Test]
        public void EquipBestItem_UsesBestComparableWorldItem()
        {
            TestInventory inventory = CreateInventory(2);
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worseWorldItem = item.GenerateWorldItem(new ComparableItemData(1));
            WorldItem betterWorldItem = item.GenerateWorldItem(new ComparableItemData(9));
            inventory.TryAdd(worseWorldItem, 1, out _);
            inventory.TryAdd(betterWorldItem, 1, out _);

            AssertSimilar(EquipmentOperations.Equipped(), inventory.EquipBestItem<TestEquippableItem>(equipment));

            Assert.IsTrue(equipment.IsEquipped(betterWorldItem));
            Assert.AreSame(worseWorldItem, inventory.SlotAt(0).Item);
            Assert.IsNull(inventory.SlotAt(1).Item);
        }

        [Test]
        public void EquipmentAccess_ReturnsEquippedBaseAndWorldItems()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);
            inventory.EquipItem(0, equipment);

            TestEquippableItem equippedBaseItem = equipment.GetFirstEquippedBaseItemFor<TestEquippableItem>();
            WorldItem equippedWorldItem = equipment.GetFirstEquippedItemFor<TestEquippableItem>();

            Assert.AreSame(item, equippedBaseItem);
            Assert.AreSame(worldItem, equippedWorldItem);
            Assert.IsTrue(equipment.IsEquipped(item));
        }

        [Test]
        public void EquipItem_FailureAlwaysInvokesCallbacks()
        {
            TestInventory inventory = CreateInventory();
            TestEquipment equipment = CreateEquipment();
            TestEquippableItem item = CreateItem<TestEquippableItem>();
            item.RejectEquip = true;
            WorldItem worldItem = item.GenerateWorldItem(null);
            inventory.TryAdd(worldItem, 1, out _);

            AssertSimilar(
                InventoryOperations.InvalidAmount(),
                inventory.EquipItem(0, equipment, EquipmentModificationFlags.None));

            Assert.AreEqual(1, item.EquipFailedCount);
        }
    }
}
