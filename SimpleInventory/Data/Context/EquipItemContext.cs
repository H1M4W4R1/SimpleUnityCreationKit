using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context.Internal;
using Systems.SimpleInventory.Data.Enums;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for equipping item
    /// </summary>
    public readonly ref struct EquipItemContext
    {
        /// <summary>
        ///     Slot that contains item to equip
        /// </summary>
        public readonly InventorySlotContext slot;

        /// <summary>
        ///     Item being equipped
        /// </summary>
        public readonly WorldItem item;

        /// <summary>
        ///     Reference to item base for easier handling
        /// </summary>
        public readonly EquippableItemBase itemBase;

        /// <summary>
        ///     Equipment where item is being equipped
        /// </summary>
        public readonly EquipmentBase equipment;

        /// <summary>
        ///     Flags for modifying action
        /// </summary>
        public readonly EquipmentModificationFlags flags;

        public EquipItemContext(
            [NotNull] EquipmentBase equipment,
            [NotNull] InventoryBase inventory,
            int slotIndex,
            EquipmentModificationFlags flags)
        {
            this.equipment = equipment;
            slot = new InventorySlotContext(inventory, slotIndex);
            item = slot.Item;
            if (ReferenceEquals(item, null))
                throw new InvalidOperationException("Slot is empty, cannot equip from an empty slot");
            itemBase = item.Item as EquippableItemBase;
            this.flags = flags;
            if (itemBase is null)
                throw new InvalidOperationException("Item is not equippable");
        }
    }
}