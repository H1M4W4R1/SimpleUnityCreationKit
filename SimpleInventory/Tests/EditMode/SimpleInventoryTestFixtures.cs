using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Abstract.Data;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Components.Items.Pickup;
using Systems.SimpleInventory.Data.Context;
using Systems.SimpleInventory.Data.Inventory;
using Systems.SimpleInventory.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleInventory.Tests
{
    public abstract class SimpleInventoryTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                Object createdObject = _createdObjects[i];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestInventory CreateInventory(int inventorySize = 8)
        {
            GameObject gameObject = Track(new GameObject("Test Inventory"));
            gameObject.SetActive(false);
            TestInventory inventory = gameObject.AddComponent<TestInventory>();
            SetInt(inventory, "<InventorySize>k__BackingField", inventorySize);
            inventory.InitializeSlotsForTests();
            return inventory;
        }

        protected TestEquipment CreateEquipment()
        {
            GameObject gameObject = Track(new GameObject("Test Equipment"));
            TestEquipment equipment = gameObject.AddComponent<TestEquipment>();
            equipment.InitializeSlotsForTests();
            return equipment;
        }

        protected TItem CreateItem<TItem>(int maxStack = 1, bool createDroppedPrefab = false)
            where TItem : ItemBase
        {
            TItem item = Track(ScriptableObject.CreateInstance<TItem>());
            item.name = typeof(TItem).Name;
            SetInt(item, "<MaxStack>k__BackingField", maxStack);

            if (createDroppedPrefab)
            {
                GameObject droppedPrefab = Track(new GameObject(typeof(TItem).Name + " Drop Prefab"));
                SetObject(item, "<DroppedItemPrefab>k__BackingField", droppedPrefab);
            }

            return item;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, propertyName);
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.IsNotNull(property, propertyName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected TestPickupItem FindSinglePickup()
        {
            TestPickupItem[] pickups = Object.FindObjectsByType<TestPickupItem>(FindObjectsInactive.Include);
            Assert.AreEqual(1, pickups.Length);
            Track(pickups[0].gameObject);
            return pickups[0];
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }

        protected static void AssertSimilar(OperationResult expected, TestPickupItem pickup)
        {
            Assert.AreEqual(expected.systemCode, pickup.LastSystemCode);
            Assert.AreEqual(expected.resultCode, pickup.LastResultCode);
        }
    }

    public sealed class TestInventory : InventoryBase
    {
        private bool _initializedSlotsForTests;

        public bool RejectPickup { get; set; }
        public int AddedCount { get; private set; }
        public int AddFailedCount { get; private set; }
        public int TakenCount { get; private set; }
        public int TakeFailedCount { get; private set; }
        public int TransferCount { get; private set; }
        public int TransferFailedCount { get; private set; }
        public int PickupCount { get; private set; }
        public int PickupFailedCount { get; private set; }
        public int DroppedCount { get; private set; }
        public int DropFailedCount { get; private set; }
        public int UsedCount { get; private set; }
        public int UseFailedCount { get; private set; }

        public InventorySlot SlotAt(int slotIndex)
        {
            return GetSlotAt(slotIndex);
        }

        public void InitializeSlotsForTests()
        {
            if (_initializedSlotsForTests) return;
            Awake();
            _initializedSlotsForTests = true;
        }

        protected override OperationResult CanPickupItem(PickupItemContext checkContext)
        {
            if (RejectPickup) return InventoryOperations.InvalidAmount();
            return base.CanPickupItem(checkContext);
        }

        protected override void OnItemAdded(in AddItemContext context, in OperationResult result, int amountLeft)
        {
            AddedCount++;
            base.OnItemAdded(in context, result, amountLeft);
        }

        protected override void OnItemAddFailed(in AddItemContext context, in OperationResult result)
        {
            AddFailedCount++;
            base.OnItemAddFailed(in context, result);
        }

        protected override void OnItemTaken(in TakeItemContext context, in OperationResult result, int amountLeft)
        {
            TakenCount++;
            base.OnItemTaken(in context, result, amountLeft);
        }

        protected override void OnItemTakeFailed(in TakeItemContext context, in OperationResult result)
        {
            TakeFailedCount++;
            base.OnItemTakeFailed(in context, result);
        }

        protected override void OnItemTransferred(in TransferItemContext context, in OperationResult result)
        {
            TransferCount++;
            base.OnItemTransferred(in context, result);
        }

        protected override void OnItemTransferFailed(in TransferItemContext context, in OperationResult result)
        {
            TransferFailedCount++;
            base.OnItemTransferFailed(in context, result);
        }

        protected override void OnItemPickedUp(
            in PickupItemContext context,
            in OperationResult result,
            int amountLeft)
        {
            PickupCount++;
            base.OnItemPickedUp(in context, result, amountLeft);
        }

        protected override void OnItemPickupFailed(in PickupItemContext context, in OperationResult result)
        {
            PickupFailedCount++;
            base.OnItemPickupFailed(in context, result);
        }

        protected override void OnItemDropped(in DropItemContext context, in OperationResult result)
        {
            DroppedCount++;
            base.OnItemDropped(in context, result);
        }

        protected override void OnItemDropFailed(in DropItemContext context, in OperationResult resultAmountExpected)
        {
            DropFailedCount++;
            base.OnItemDropFailed(in context, resultAmountExpected);
        }

        protected override void OnItemUsed(in UseItemContext context, in OperationResult result)
        {
            UsedCount++;
            base.OnItemUsed(in context, result);
        }

        protected override void OnItemUseFailed(in UseItemContext context, in OperationResult result)
        {
            UseFailedCount++;
            base.OnItemUseFailed(in context, result);
        }
    }

    public sealed class TestEquipment : EquipmentBase
    {
        private bool _initializedSlotsForTests;

        public void InitializeSlotsForTests()
        {
            if (_initializedSlotsForTests) return;
            BuildEquipmentSlots();
            _initializedSlotsForTests = true;
        }

        protected override void BuildEquipmentSlots()
        {
            AddEquipmentSlotFor<TestEquippableItem>();
        }
    }

    public sealed class TestItem : ItemBase
    {
    }

    public sealed class TestOtherItem : ItemBase
    {
    }

    public sealed class TestUsableItem : UsableItemBase, System.IComparable<TestUsableItem>
    {
        public bool RejectUse { get; set; }
        public int UsedCount { get; private set; }
        public int UseFailedCount { get; private set; }

        protected override OperationResult CanUse(in UseItemContext context)
        {
            if (RejectUse) return InventoryOperations.InvalidAmount();
            return base.CanUse(in context);
        }

        protected override void OnUse(in UseItemContext context, OperationResult result)
        {
            UsedCount++;
        }

        protected override void OnUseFailed(in UseItemContext context, OperationResult result)
        {
            UseFailedCount++;
        }

        public int CompareTo(TestUsableItem other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return CompareTo((ItemBase)other);
        }
    }

    public sealed class TestEquippableItem : EquippableItemBase, System.IComparable<TestEquippableItem>
    {
        public bool RejectEquip { get; set; }
        public bool RejectUnequip { get; set; }
        public int EquippedCount { get; private set; }
        public int UnequippedCount { get; private set; }
        public int AlreadyEquippedCount { get; private set; }
        public int AlreadyUnequippedCount { get; private set; }
        public int EquipFailedCount { get; private set; }
        public int UnequipFailedCount { get; private set; }

        protected override OperationResult CanEquip(in EquipItemContext context)
        {
            if (RejectEquip) return InventoryOperations.InvalidAmount();
            return base.CanEquip(in context);
        }

        protected override OperationResult CanUnequip(in UnequipItemContext context)
        {
            if (RejectUnequip) return InventoryOperations.InvalidAmount();
            return base.CanUnequip(in context);
        }

        protected override void OnEquipSuccess(in EquipItemContext context, in OperationResult result)
        {
            EquippedCount++;
        }

        protected override void OnUnequipSuccess(in UnequipItemContext context, in OperationResult result)
        {
            UnequippedCount++;
        }

        protected override void OnEquipWhenAlreadyEquipped(
            in EquipItemContext context,
            in OperationResult result)
        {
            AlreadyEquippedCount++;
        }

        protected override void OnUnequipWhenAlreadyUnequipped(
            in UnequipItemContext context,
            in OperationResult result)
        {
            AlreadyUnequippedCount++;
        }

        protected override void OnEquipWhenCannotBeEquipped(
            in EquipItemContext context,
            in OperationResult result)
        {
            EquipFailedCount++;
        }

        protected override void OnUnequipWhenCannotBeUnequipped(
            in UnequipItemContext context,
            in OperationResult result)
        {
            UnequipFailedCount++;
        }

        public int CompareTo(TestEquippableItem other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return CompareTo((ItemBase)other);
        }
    }

    public sealed class TestOtherEquippableItem : EquippableItemBase
    {
    }

    public sealed class TestPickupItem : PickupItem
    {
        public int CompletionCount { get; private set; }
        public ushort LastSystemCode { get; private set; }
        public ushort LastResultCode { get; private set; }
        public int LastAmountLeft { get; private set; }

        protected override void OnPickupAttemptComplete(in OperationResult result, int amountLeft)
        {
            CompletionCount++;
            LastSystemCode = result.systemCode;
            LastResultCode = result.resultCode;
            LastAmountLeft = amountLeft;
        }
    }

    public sealed class ComparableItemData : ItemData
    {
        private readonly int _score;

        public ComparableItemData(int score)
        {
            _score = score;
        }

        public override int CompareTo(ItemData other)
        {
            if (other is not ComparableItemData comparableItemData) return 0;
            return _score.CompareTo(comparableItemData._score);
        }
    }
}
