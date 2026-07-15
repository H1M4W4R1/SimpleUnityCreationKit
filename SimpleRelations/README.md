# SimpleRelations

SimpleRelations stores independent, numeric, one-way relationships between relatable Unity objects. Use it for trust, affinity, fear, friendship, rivalry, hostility, or any other score that should be owned by one actor and directed at another.

## Requirements

- Unity 6000.5+
- SimpleCore assembly

The runtime assembly is `SimpleRelations`. Edit-mode tests are in `SimpleRelations.Tests`.

## Relation types

Create a sealed `RelationTypeBase` subclass for each independently tracked score. The base class automatically creates an asset in `Assets/Generated/Relations/` and registers it with the `SimpleRelations.Types` Addressables label. Override `GetInitialValue` when a relation should begin at a non-zero value or that value depends on its source or target.

```csharp
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;

public sealed class TrustRelation : RelationTypeBase
{
    protected override int GetInitialValue(in RelationInitialValueContext context)
    {
        return 25;
    }
}
```

Relation types intentionally do not define levels or thresholds. Interpret values in game code, or connect them to SimpleProgression when a relationship needs progression-style level calculations.

## Relatables and API

Implement `IRelatable` on each Unity object that owns outgoing relations. The only per-actor state is a serialized `List<RelationEntry>` backing field. Its explicit interface member is protected by the interface, so callers interact through the relation operations rather than the mutable entry list.

```csharp
using System.Collections.Generic;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

public sealed class ActorRelations : MonoBehaviour, IRelatable
{
    [SerializeField] private List<RelationEntry> _relationEntries = new();

    List<RelationEntry> IRelatable.RelationEntries => _relationEntries;
}
```

Use the open generic `RelationAPI` methods directly. `Change` creates an entry at the type's initial value when necessary, then applies the amount. `Set` writes an exact value. Both return `OperationResult`. When the relation type is only known at runtime, pass its `RelationTypeBase` asset to the non-generic overloads.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Utility;

public static class RelationExample
{
    public static OperationResult RewardTrust(ActorRelations source, ActorRelations target)
    {
        return RelationAPI.Change<TrustRelation>(source, target, 10);
    }
}
```

Relations are unidirectional. Changing `source`'s trust in `target` does not create or modify `target`'s trust in `source`. A source cannot create a relation to itself. One component stores at most one entry for each `(relation type, target)` pair.

`IRelatable` supplies the relation handling, lookup, and mutation implementation through default interface methods. `RelationEntry` serializes each target as a Unity object that implements `IRelatable`.

`IRelatable` offers `TryGetRelation` and `TryGetRelationValue` in both forms: pass a `RelationTypeBase` asset directly, or use the generic overload when the type asset is available through `RelationTypeDatabase`. The `Try` methods return `false` for an untracked relation instead of returning the type's initial value.

## Callbacks

Override `CanChangeRelation`, `OnRelationChanged`, or `OnRelationChangeFailed` on a relation type to apply type-specific rules and reactions. `RelationChangeContext` provides the source, target, previous value, and new value. Callbacks are virtual methods rather than events.

## Example scene

Open `Examples/Scene - Relations.unity` and enter Play Mode. The runtime panel changes Source's trust and fear toward Target, displays the serialized entry count, and shows that Target's reverse trust remains independent.
