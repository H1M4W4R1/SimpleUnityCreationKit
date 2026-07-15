# SimpleFactions

SimpleFactions provides typed faction configuration, component-based membership, and faction-owned SimpleRelations. It deliberately has no rank or reputation subsystem: reputation is a directional relation from a faction to an actor.

## Requirements

- Unity 6000.5+
- SimpleCore
- SimpleRelations
- SimpleSaving
- Unity Addressables

## Membership

Create a faction and a membership component.

```csharp
using Systems.SimpleFactions.Abstract;
using UnityEngine;

public sealed class EmpireFaction : FactionBase<PlayerController> { }

public sealed class PlayerFactionMembership : FactionMembershipBase<PlayerController> { }
```

Concrete faction types are auto-created as addressable assets under `Assets/Generated/Factions/`.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Utility;

public static class MembershipActions
{
    public static OperationResult JoinEmpire(PlayerFactionMembership membership)
    {
        return FactionAPI.Join<EmpireFaction, PlayerController>(membership);
    }
}
```

Override `CanJoin`, `CanLeave`, `OnJoined`, or `OnLeft` in `FactionBase<TFactionObject>` for faction rules. `FactionMembershipBase<THolder>` has matching membership-component hooks.

## Reputation is a relation

Every `FactionBase` implements `IRelatable`. Define a relation type for the meaning your game needs—reputation, hostility, trust, or influence—then set it from the faction to the actor. Relations are directional.

```csharp
using Systems.SimpleRelations.Abstract;

public sealed class ReputationRelation : RelationTypeBase { }
```

The target must also be an `IRelatable` Unity object. Implement `IIdentifiable<Snowflake128>` when the relation must survive saving.

```csharp
using System.Collections.Generic;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using UnityEngine;

public sealed class PlayerRelations : MonoBehaviour, IRelatable, IIdentifiable<Snowflake128>
{
    [SerializeField] private Snowflake128 _identifier;
    [SerializeField] private List<RelationEntry> _relationEntries = new List<RelationEntry>();

    public Snowflake128 Identifier => _identifier;

    List<RelationEntry> IRelatable.RelationEntries => _relationEntries;
}
```

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Utility;

public static class ReputationActions
{
    public static OperationResult RewardService(EmpireFaction empire, PlayerRelations player)
    {
        return FactionAPI.ChangeRelation<ReputationRelation>(empire, player, 25);
    }
}
```

## Saving relations

`FactionAPI.SaveToMemory` saves all outgoing faction relations. Faction targets are resolved using their concrete type hash. Runtime targets are saved only when they implement `IIdentifiable<Snowflake128>` and expose a created identifier.

Before loading, register every live runtime target that may receive a relation in `RelatableObjectDatabase`. Register it after the target restores its identifier and unregister it when the target is removed. The database is owned by SimpleRelations, so other systems can resolve the same identified relatable objects.

```csharp
using Systems.SimpleRelations.Data;
using UnityEngine;

public sealed class PlayerRelationsBootstrap : MonoBehaviour
{
    [SerializeField] private PlayerRelations _playerRelations;

    private void OnEnable()
    {
        RelatableObjectDatabase.Register(_playerRelations);
    }

    private void OnDisable()
    {
        RelatableObjectDatabase.Unregister(_playerRelations);
    }
}
```

`FactionAPI.Load` clears current faction-owned relations and restores entries whose faction/relation assets and runtime targets can be resolved. Missing or unregistered runtime targets are skipped safely.

## Public API

| Method | Description |
| --- | --- |
| `Join` / `Leave` | Changes faction membership. |
| `ChangeRelation` / `SetRelation` | Changes or assigns a one-way relation owned by a faction. |
| `GetRelationValue` | Reads a relation value, including its relation type's initial value. |
| `SaveToMemory` / `Load` | Saves and restores faction-to-faction and faction-to-runtime-object relations. |
