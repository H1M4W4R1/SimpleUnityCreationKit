# SimpleEntities

A flexible entity system for Unity that provides foundational classes for in-game objects with support for health, damage, healing, resistance calculations, and status effects.

## Description

SimpleEntities is a component-based system designed to represent game objects with stats and interactive systems. It offers a base framework for creating entities like players, enemies, NPCs, and other dynamic game objects. The system provides built-in support for health mechanics, affinity-based damage and healing, resistance modifiers, and stackable status effects with per-tick updates.

## Requirements

This package depends on the following:

- **Unity 2022.3+** (for C# 10 and modern language features)
- **Unity.Addressables** (for dynamic asset loading)
- **Unity.Burst** (for performance optimization)
- **Unity.Collections** (for high-performance collections)
- **Unity.Mathematics** (for mathematical operations)
- **Unity.ResourceManager** (for addressable resource management)
- **SimpleStats** (custom package for stat modifiers and value calculations)
- **SimpleCore** (custom package for core utilities, timing, and operations)

## Usage Examples

### Creating a Basic Entity

Extend `AliveEntityBase` to create an entity with health and damage capabilities:

```csharp
using Systems.SimpleEntities.Components;

public class MyGameEntity : AliveEntityBase
{
    [SerializeField] private long baseHealthAmount = 100;
    
    protected override void OnInitialized()
    {
        MaxHealth = baseHealthAmount;
        base.OnInitialized(); // resets CurrentHealth to MaxHealth
    }
}
```

### Dealing Damage

Apply damage to an entity using an affinity type:

```csharp
// Deal fire damage to an entity (100 damage before resistance calculations)
entity.Damage<FireAffinity>(damageSource, 100);
```

### Healing Entities

Restore health to an entity:

```csharp
// Heal fire affinity (healing amplified by fire resistance)
entity.Heal<FireAffinity>(healSource, 50);
```

### Working with Affinities

Define custom damage/healing types by extending `AffinityType`:

```csharp
using Systems.SimpleEntities.Data.Affinity;

public sealed class PoisonAffinity : AffinityType
{
    // Override hooks for custom behavior
    protected internal override void OnDamageReceived(
        in DamageContext context,
        in OperationResult result,
        long healthLost)
    {
        // Implement poison-specific logic
    }
}
```

### Applying Status Effects

Create custom status effects by extending `StatusBase`:

```csharp
using Systems.SimpleEntities.Data.Status.Abstract;

public sealed class StunStatusExample : StatusBase
{
    protected internal override void OnStatusApplied(
        in StatusContext context,
        in OperationResult result,
        int currentStacks)
    {
        // Prevent movement or actions
    }
    
    protected internal override void OnStatusTick(
        in StatusContext context,
        float deltaTime)
    {
        // Handle per-frame status logic
    }
}
```

Then apply the status to an entity:

```csharp
// Apply stun status (1 stack, external action source)
entity.ApplyStatus<StunStatusExample>(stackCount: 1);

// Remove stun status
entity.RemoveStatus<StunStatusExample>(stackCount: 1);

// Check if entity has status
if (entity.HasStatus<StunStatusExample>())
{
    // Entity is stunned
}
```

`StatusModificationFlags.IgnoreConditions` bypasses `CanApply` / `CanRemove` validation for trusted internal flows.
`StatusModificationFlags.IgnoreStackLimit` bypasses the configured `MaxStack` limit when applying stacks, and clamps
over-removal to the stacks currently available instead of creating negative stack counts.
Status application and removal require a positive stack count; zero or negative counts return
`StatusOperations.InvalidStackCount()` without changing the entity.

### Implementing Resistance Modifiers

Add resistance to damage by using stat modifiers:

```csharp
public override void RefreshModifiersIfNecessary()
{
    statModifiers.Clear();
    // Add 0.5 (50%) fire resistance
    statModifiers.Add(new FlatAddModifier<EntityFireResistance>(0.5f));
}
```

Get an entity's resistance for a given affinity:

```csharp
float fireResistance = entity.GetResistance<FireAffinity>();
```

### Tick-Based Entity Updates

Entities can subscribe to the global tick system for per-frame updates:

```csharp
public class TickingEntity : AliveEntityBase
{
    private float _activeDuration = 0f;
    
    protected override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime); // handles timed modifiers and status ticks
        _activeDuration += deltaTime;
    }
}
```

### Entity Lifecycle Hooks

Override lifecycle methods to respond to entity events:

```csharp
protected override void AssignComponents()
{
    // Called first in Awake — get component references here
}

protected override void OnInitialized()
{
    // Called in Awake after AssignComponents
}

protected override void OnEntitySetupComplete()
{
    // Called during Start
}

protected override void OnEntityActivated()
{
    // Called on OnEnable
}

protected override void OnEntityDeactivated()
{
    // Called on OnDisable
}

protected override void OnTeardown()
{
    // Called on OnDestroy
}
```

### Death and Survival

Handle entity death with optional death-save mechanics:

```csharp
// Override to implement death-save logic (e.g., revive mechanics)
protected override DeathSaveContext CanSaveFromDeath(in DamageContext context)
{
    // Return (shouldBeSaved: true, healthToSet: 1) to prevent death
    return new DeathSaveContext(shouldBeSaved: true, healthToSet: 1);
}

protected override void OnDeath(in DamageContext context, in OperationResult result, long healthLost)
{
    // Cleanup logic when entity dies
    gameObject.SetActive(false);
}
```

## Core Components

- **EntityBase**: Abstract base for all game entities with lifecycle hooks
- **TickingEntityBase**: Extends EntityBase with automatic tick system registration
- **AliveEntityBase**: Full-featured entity with health, damage, healing, resistances, and status effects
- **AffinityType**: Base for damage/healing types (Fire, Cold, Poison, etc.)
- **StatusBase**: Base for stackable status effects with per-tick updates
- **DamageContext / HealContext**: Contextual data for damage and healing operations

## File Locations

- Core entity components: `Assets/Systems/SimpleEntities/Components/`
- Data structures and contexts: `Assets/Systems/SimpleEntities/Data/`
- Status and affinity systems: `Assets/Systems/SimpleEntities/Data/Status/` and `Assets/Systems/SimpleEntities/Data/Affinity/`
- Example implementations: `Assets/Systems/SimpleEntities/Examples/`, including runtime Unity UI for burning status and elemental damage cases.
- EditMode tests: `Assets/Systems/SimpleEntities/Tests/EditMode/` (`SimpleEntities.Tests`, Editor-only)
