# SimpleRelations

SimpleRelations stores independent, numeric, one-way relationships between relatable Unity objects. Use it for trust, affinity, fear, friendship, rivalry, hostility, or any other score that should be owned by one actor and directed at another.

## Requirements

- Unity 6000.5+
- SimpleCore assembly

The runtime assembly is `SimpleRelations`. Edit-mode tests are in `SimpleRelations.Tests`.

## Relation types

Create a sealed `RelationTypeBase` subclass for each independently tracked score. The base class automatically creates an asset in `Assets/Generated/Relations/` and registers it with the `SimpleRelations.Types` Addressables label. Override `InitialValue` only when a relation should begin at a non-zero score.

```csharp
using Systems.SimpleRelations.Abstract;

public sealed class TrustRelation : RelationTypeBase
{
    protected internal override int InitialValue => 25;
}
```

Relation types intentionally do not define levels or thresholds. Interpret values in game code, or connect them to SimpleProgression when a relationship needs progression-style level calculations.

## Components and API

Derive a component from `RelationComponentBase` and add it to each actor that owns outgoing relations. The component serializes a `RelationStorage` collection of `RelationEntry` instances. Each entry contains only the relation type asset, a strongly typed target relation component, and current value.

```csharp
using Systems.SimpleRelations.Components;

public sealed class ActorRelations : RelationComponentBase { }
```

Use `RelationAPI` with a typed context. `Change` creates an entry at the type's initial value when necessary, then applies the amount. `Set` writes an exact value. Both return `OperationResult`.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Utility;

public static class RelationExample
{
    public static OperationResult RewardTrust(ActorRelations source, ActorRelations target)
    {
        RelationChangeContext<TrustRelation> context = new(source, target, 10);
        return RelationAPI.Change<TrustRelation>(in context);
    }
}
```

Relations are unidirectional. Changing `source`'s trust in `target` does not create or modify `target`'s trust in `source`. A source cannot create a relation to itself. One component stores at most one entry for each `(relation type, target)` pair.

`IRelatable` is a storage-agnostic domain contract used by the API. Serialization is an implementation detail of `RelationEntry`; the contract does not expose Unity objects or relation components.

`RelationComponentBase` additionally offers `TryGetRelation` and `TryGetRelationValue` in both forms: pass a `RelationTypeBase` asset directly, or use the generic overload when the type asset is available through `RelationTypeDatabase`. The `Try` methods return `false` for an untracked relation instead of returning the type's initial value.

## Callbacks

Override `CanChangeRelation`, `OnRelationChanged`, or `OnRelationChangeFailed` on a relation component to apply actor-specific rules and reactions. `RelationChangeContext` provides the type, target, previous value, and new value. Callbacks are virtual methods rather than events.

## Example scene

Open `Examples/Scene - Relations.unity` and enter Play Mode. The runtime panel changes Source's trust and fear toward Target, displays the serialized entry count, and shows that Target's reverse trust remains independent.
