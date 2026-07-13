# SimpleInventory

A flexible, performance-optimized inventory and equipment management system for Unity games. SimpleInventory provides a robust foundation for managing items, handling equipment slots, and supporting item interactions like pickup, use, and transfer operations.

## About

SimpleInventory is a complete inventory solution designed for RPG-style games and similar projects. It features modular item management, stackable items with configurable max stacks, equipment systems with typed slots, and extensible event callbacks for custom item behavior. The system is built on top of SimpleCore and uses addressable assets for efficient item database management.

## Requirements

- **Unity 2022.2 LTS or later**
- **SimpleCore** assembly
- **Unity Addressables** package
- **Unity Burst** (optional, for performance optimization)
- **Unity Collections** (for high-performance data structures)
- **Unity Mathematics** (for numeric operations)

## Features

- **Flexible Item System**: Create custom items by extending `ItemBase`, `EquippableItemBase`, or `UsableItemBase`
- **Equipment Slots**: Type-safe equipment slots with support for multiple item types
- **Stackable Items**: Automatic stack management with configurable max stack sizes
- **Item Pickup System**: Drop and pick up items with customizable pickup components
- **Event Callbacks**: Comprehensive event system for add, remove, equip, unequip, and use operations
- **Items Database**: Centralized addressable-based database for managing all game items

## Quick Start

### Basic Inventory Setup

```csharp
using Systems.SimpleInventory.Components.Inventory;
using Systems.SimpleInventory.Abstract.Items;

public class MyInventory : InventoryBase
{
    // Inheriting from InventoryBase provides all inventory functionality
    // Configure InventorySize via inspector (default 2048 items)
}
```

### Creating a Simple Item

```csharp
using Systems.SimpleInventory.Abstract.Items;
using UnityEngine;

public class MyItem : ItemBase
{
    // Identifier and MaxStack are configurable in inspector
    // MaxStack of 1 = non-stackable item
}
```

### Working with Items

```csharp
// Add item to inventory
inventory.TryAdd<MyItem>(1, out int amountLeft);

// Get item by type
var itemRef = inventory.GetFirstItemOfType<MyItem>();
if (itemRef.IsValid)
{
    var worldItem = itemRef.item;
}

// Transfer items between inventories
inventory.TryTransferItem(sourceSlotIndex, targetInventory, targetSlotIndex);

// Drop item from a slot as a world pickup
inventory.TryDropItemAs<PickupItemWithDestroy>(slotIndex, 1);
```

### Creating Equippable Items

```csharp
using Systems.SimpleInventory.Abstract.Items;

public class SteelHelmet : EquippableItemBase
{
    protected internal override OperationResult CanEquip(in EquipItemContext context)
    {
        // Add custom validation logic
        return InventoryOperations.Permitted();
    }

    protected internal override void OnEquipSuccess(in EquipItemContext context, in OperationResult result)
    {
        // Handle successful equip (e.g., apply stats)
    }
}
```

### Setting Up Equipment

```csharp
using Systems.SimpleInventory.Components.Equipment;
using Systems.SimpleInventory.Abstract.Items;

public class CharacterEquipment : EquipmentBase
{
    protected override void BuildEquipmentSlots()
    {
        AddEquipmentSlotFor<HelmetItemBase>();
        AddEquipmentSlotFor<ChestplateItemBase>();
        AddEquipmentSlotFor<LeggingsItemBase>();
        AddEquipmentSlotFor<BootsItemBase>();
    }
}
```

### Creating Usable Items

```csharp
using Systems.SimpleInventory.Abstract.Items;

public class HealthPotion : UsableItemBase
{
    [SerializeField] private int healAmount = 20;

    protected internal override OperationResult CanUse(in UseItemContext context)
    {
        return InventoryOperations.Permitted();
    }

    protected internal override void OnUse(in UseItemContext context, OperationResult result)
    {
        // Apply healing or other consumable effects
        Debug.Log($"Used {name} to heal {healAmount}");
    }
}
```

### Using Items from Inventory

```csharp
// Use item by slot
inventory.UseItem(slotIndex);

// Use first item of type
inventory.UseAnyItem<HealthPotion>();

// Use best item of type (requires HealthPotion to implement IComparable<HealthPotion>)
inventory.UseBestItem<HealthPotion>();
```

### Equipping Items

```csharp
// Equip from inventory slot
var result = inventory.EquipItem(slotIndex, equipment);

// Equip the first available item of a type from inventory
inventory.EquipAnyItem<HelmetItemBase>(equipment);

// Unequip a specific item
inventory.UnequipItem(worldItem, equipment);

// Unequip the first equipped item of a type
inventory.UnequipAnyItem<HelmetItemBase>(equipment);
```

### Handling Inventory Events

```csharp
public class MyInventory : InventoryBase
{
    protected override void OnItemAdded(in AddItemContext context, in OperationResult result, int amountLeft)
    {
        base.OnItemAdded(in context, result, amountLeft);
        Debug.Log($"Added {context.itemInstance.Item.name}");
    }

    protected override void OnItemTaken(in TakeItemContext context, in OperationResult result, int amountLeft)
    {
        base.OnItemTaken(in context, result, amountLeft);
        Debug.Log($"Took {context.itemInstance.name}");
    }

    protected override void OnItemAddFailed(in AddItemContext context, in OperationResult result)
    {
        base.OnItemAddFailed(in context, result);
        // Handle inventory full or other failures
    }
}
```

### Dropping Items

```csharp
// Drop or create item with custom pickup component
ItemBase.DropItem<PickupItemWithDestroy>(
    worldItem,
    amount,
    position,
    rotation,
    parent: null
);
```

## Architecture

- **Abstract Layer**: Base classes (`ItemBase`, `EquippableItemBase`, `UsableItemBase`) define item behavior
- **Components**: MonoBehaviour-based systems (`InventoryBase`, `EquipmentBase`) manage game state
- **Data Layer**: Context objects and enums provide type-safe operation parameters
- **Operations**: Utility methods for validation and result handling
- **Examples**: Runtime Unity UI and reference implementations showing common use cases (armor, food, weapons)

## Examples included

- `Scene - Inventory.unity`: exposes runtime Unity UI for first-food use, best-food use, leather/steel equipment, unequip, and equippable database inspection.
- `ExampleInventory`: scene driver with runtime buttons and context menu actions for replaying inventory and equipment examples.
- Example food and armor item types: configured typed items used by the scene.
