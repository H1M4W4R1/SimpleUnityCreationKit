# SimpleRelations

SimpleRelations stores independent numeric, one-way relationships between Unity objects. Use it for trust, hostility, fear, affinity, suspicion, friendship, diplomacy, or any other value an actor owns about another actor.

## Requirements

- Unity 6000.5+
- SimpleCore
- Unity Addressables

The runtime assembly is `SimpleRelations`. Edit-mode tests are in `SimpleRelations.Tests`.

## Features

- Typed, addressable relation definitions
- One-way values: `A -> B` is independent from `B -> A`
- Any `MonoBehaviour` or `ScriptableObject` can be a source or target when it implements `IRelatable`
- Initial values, validation, success callbacks, and failure callbacks per relation type
- Exact assignment and checked additive changes with overflow protection
- Stable type hashes for systems that persist relation snapshots, including SimpleFactions

## Quick start

### Create a relation type

Create one sealed `RelationTypeBase` subclass for each independently tracked score. The base class creates its asset under `Assets/Generated/Relations/` and registers it with the `SimpleRelations.Types` Addressables label.

```csharp
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;

public sealed class AttitudeRelation : RelationTypeBase
{
    protected override int GetInitialValue(in RelationInitialValueContext context)
    {
        return 0;
    }
}
```

### Make an actor relatable

Implement `IRelatable` on any Unity object that owns outgoing relations. The serialized list belongs to the actor; outside code uses `RelationAPI`, not the list.

```csharp
using System.Collections.Generic;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

public sealed class MonsterRelations : MonoBehaviour, IRelatable
{
    [SerializeField] private List<RelationEntry> _relationEntries = new List<RelationEntry>();

    List<RelationEntry> IRelatable.RelationEntries => _relationEntries;
}
```

### Change and query a value

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Utility;

public static class MonsterRelationActions
{
    public static OperationResult AngerMonster(MonsterRelations monster, MonsterRelations player)
    {
        return RelationAPI.Change<AttitudeRelation>(monster, player, -25);
    }

    public static int GetAttitude(MonsterRelations monster, MonsterRelations player)
    {
        return RelationAPI.GetValue<AttitudeRelation>(monster, player);
    }
}
```

`Change` creates a tracked entry at the relation type's initial value, then applies the amount. `Set` writes an exact value. `GetValue` returns the initial value when no entry exists. `IRelatable.TryGetRelationValue` answers whether a serialized entry actually exists.

## Relation behavior

Relations are directional. A source cannot relate to itself, and it stores no more than one entry for each `(relation type, target)` pair.

| Operation | Result |
|---|---|
| `RelationAPI.Change` | Adds a non-zero amount after validation; rejects integer overflow. |
| `RelationAPI.Set` | Writes an exact value after validation. |
| `RelationAPI.GetValue` | Returns the stored value or the type's initial value. |
| `IRelatable.TryGetRelation` | Locates a tracked `RelationEntry` without creating one. |
| `IRelatable.TryGetRelationValue` | Gets only a tracked value; returns `false` when no entry exists. |

Pass the generated relation asset to the non-generic overloads when the type is selected at runtime.

```csharp
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Utility;

public static class RelationRuntimeSelection
{
    public static int GetValue(
        MonsterRelations source,
        MonsterRelations target,
        RelationTypeBase relationType)
    {
        return RelationAPI.GetValue(source, target, relationType);
    }
}
```

## Custom rules and callbacks

Override callbacks on a relation type to apply rules or react to completed operations. `RelationChangeContext` and `RelationSetContext` contain the source, target, previous value, and requested or final value.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Operations;

public sealed class BoundedAttitudeRelation : RelationTypeBase
{
    protected override OperationResult CanChangeRelation(in RelationChangeContext context)
    {
        if (context.newValue < -100 || context.newValue > 100)
            return RelationOperations.InvalidAmount();

        return RelationOperations.Permitted();
    }

    protected override void OnRelationChanged(in RelationChangeContext context)
    {
        // React to the completed write.
    }
}
```

Use `CanChangeRelation` and `CanSetRelation` for validation. Use `OnRelationChanged`, `OnRelationSet`, `OnRelationChangeFailed`, and `OnRelationSetFailed` for reactions. These protected virtual methods keep behavior with the relation type instead of exposing public events.

## Persistence

`RelationEntry` stores Unity object references, which is appropriate for authored scene and asset data but insufficient by itself for a save file that survives a new session. The owning gameplay system must map runtime actors to stable IDs. SimpleFactions provides `FactionAPI.SaveToMemory` and `FactionAPI.Load` through SimpleSaving for faction-to-faction relations; see its README for the exact scope.

For a non-faction source, save `HashIdentifier.New(relationType.GetType()).Value`, your target's stable game ID, and the numeric value returned by each `RelationEntry`. On load, resolve the relation type and target, then call `RelationAPI.Set` to preserve its validation and callbacks. Type hash values are stable only while the type and assembly names remain unchanged.

## Architecture

- **Abstract**: `RelationTypeBase` and `IRelatable` define relation ownership and behavior.
- **Data**: `RelationEntry` serializes a type, target reference, and value; contexts describe operations.
- **Utility**: `RelationAPI` is the static entry point for changes, assignments, and queries.
- **Operations**: `RelationOperations` defines standard `OperationResult` codes.
- **Examples**: the included scene demonstrates independent trust and fear values.

## Example scene

Open `Examples/Scene - Relations.unity` and enter Play Mode. The runtime panel changes Source's trust and fear toward Target, displays the serialized entry count, and shows that Target's reverse trust remains independent.
