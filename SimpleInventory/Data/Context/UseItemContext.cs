using System;
using JetBrains.Annotations;
using Systems.SimpleInventory.Abstract.Items;
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Data.Context.Internal;
using Systems.SimpleInventory.Data.Inventory;

namespace Systems.SimpleInventory.Data.Context
{
    /// <summary>
    ///     Context for using item
    /// </summary>
    public readonly ref struct UseItemContext
    {
        /// <summary>
        ///     Inventory slot where item is being used
        /// </summary>
        public readonly InventorySlotContext slot;

        /// <summary>
        ///     Item being used
        /// </summary>
        public readonly WorldItem itemInstance;

        /// <summary>
        ///     Item base that is being used
        /// </summary>
        public readonly UsableItemBase itemBase;

        public UseItemContext([NotNull] InventoryBase inventory, int slotIndex)
        {
            slot = new InventorySlotContext(inventory, slotIndex);
            itemInstance = slot.Item;
            if (ReferenceEquals(itemInstance, null))
                throw new InvalidOperationException("Slot is empty, cannot use item from an empty slot");
            itemBase = itemInstance.Item as UsableItemBase;
            if (itemBase is null)
                throw new InvalidOperationException("Item is not usable");
        }
    }
}