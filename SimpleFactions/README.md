# SimpleFactions

Reputation and allegiance tracking system for SimpleUnityCreationKit.

## Overview

SimpleFactions lets any game object join factions, accumulate reputation, and earn or lose reputation levels. Everything is driven by ScriptableObject configuration with zero allocations in the hot path (all operation contexts are `readonly ref struct`).

---

## Quick Start

### 1 — Create a faction

```csharp
// Sealed class is auto-created and registered in FactionDatabase.
public sealed class KnightsGuild : FactionBase<PlayerController>
{
    protected internal override void OnJoined(
        in JoinFactionContext<PlayerController> context, in OperationResult result)
    {
        Debug.Log($"{context.member.name} joined the Knights Guild.");
    }
}
```

### 2 — Create reputation levels

```csharp
// Implements IForFaction<KnightsGuild> so FactionLevelAssigner wires it
// automatically into KnightsGuild._levels on script reload.
public sealed class SquireLevel : ReputationLevelBase, IForFaction<KnightsGuild>
{
    // Set AutomaticPromotion=true, PromotionThreshold=100 in the Inspector.
    // Set AutomaticDemotion=true, DemotionThreshold=50 in the Inspector.

    protected internal override void OnLevelAchieved(
        in FactionLevelChangeContext context, in OperationResult result)
    {
        Debug.Log("Became a Squire!");
    }
}

public sealed class KnightLevel : ReputationLevelBase, IForFaction<KnightsGuild>
{
    // AutomaticPromotion=false (manual grant only).
    // AutomaticDemotion=true, DemotionThreshold=200 (auto-revoked if rep drops).
}
```

### 3 — Attach the membership component

```csharp
public sealed class PlayerFactionMembership : FactionMembershipBase<PlayerController>
{
    // GetHolder() auto-resolves to GetComponent<PlayerController>() by default.
    // Override if the PlayerController is elsewhere:
    //   protected override PlayerController GetHolder() => _playerController;
}
```

Attach `PlayerFactionMembership` and `PlayerController` to the same `GameObject`.

### 4 — Drive it from code

```csharp
PlayerFactionMembership membership = player.GetComponent<PlayerFactionMembership>();

// Join
FactionAPI.Join<KnightsGuild, PlayerController>(membership);

// Gain reputation (triggers auto-promotion if threshold met)
FactionAPI.ChangeReputation<KnightsGuild, PlayerController>(membership, 150);

// Manual promotion (bypasses checks — e.g. king grants knighthood)
KnightLevel knightLevel = ReputationLevelDatabase.GetExact<KnightLevel>();
FactionAPI.AssignLevel<KnightsGuild, PlayerController>(membership, knightLevel);

// Query
ReputationLevelBase current = FactionAPI.GetLevel<KnightsGuild, PlayerController>(membership);
bool isKnightOrAbove = FactionAPI.IsAtLeastLevel<KnightsGuild, PlayerController>(membership, knightLevel);

// Leave
FactionAPI.Leave<KnightsGuild, PlayerController>(membership);
```

---

## How IForFaction Auto-Assignment Works

When a concrete `ReputationLevelBase` type implements `IForFaction<TFaction>`, the
`FactionLevelAssigner` editor postprocessor:

1. Scans all `ReputationLevelBase` assets in the project after every script reload or asset import.
2. For each asset, inspects the runtime type for `IForFaction<TFaction>` implementations via reflection.
3. Finds the matching `TFaction` asset (auto-generated in `Assets/Generated/Factions/`).
4. Calls `faction.AssignLevel(level)` — adds the level if absent and re-sorts the list by
   `PromotionThreshold` ascending (index 0 = lowest rank).
5. Marks the faction asset dirty only when the level list changed, then saves outside Unity's asset-import callback.

Levels that do **not** implement `IForFaction<TFaction>` are never auto-assigned and can be managed manually via the Inspector.

---

## Manual vs Automatic Promotion/Demotion

### Automatic
Set `AutomaticPromotion = true` and `PromotionThreshold` on a `ReputationLevelBase`. When
`ChangeReputation` causes reputation to meet or exceed the threshold, the level is automatically
granted (subject to `CanBePromoted` checks).

### Automatic demotion
Set `AutomaticDemotion = true` and `DemotionThreshold`. When reputation falls below the threshold
while the object holds this level, the level is automatically revoked. This flag is evaluated
**independently** of `AutomaticPromotion`, so a manually granted level can still be auto-revoked.

### Manual
`FactionAPI.AssignLevel` bypasses all promotion/demotion checks. Use it for unconditional grants
such as a king promoting a player to knight or an admin command.

---

## Extending Checks at Each Level

Checks fire in order — first faction, then level, then member. Any denial short-circuits.

### Faction level (FactionBase)

```csharp
protected internal override OperationResult CanJoin(in JoinFactionContext<PlayerController> ctx)
{
    // Block join if player is an enemy.
    return ctx.member.IsEnemy ? FactionOperations.Denied() : FactionOperations.Permitted();
}

protected internal override OperationResult CanBePromoted(in FactionLevelChangeContext<PlayerController> ctx)
    => FactionOperations.Permitted();
```

### Level level (ReputationLevelBase)

```csharp
protected internal override OperationResult CanPromoteTo(in FactionLevelChangeContext ctx)
{
    // Require a quest to be completed before promotion to this level.
    return QuestIsComplete() ? FactionOperations.Permitted() : FactionOperations.Denied();
}

protected internal override OperationResult CanDemoteFrom(in FactionLevelChangeContext ctx)
    => FactionOperations.Permitted(); // never protect this level from auto-demotion
```

### Member level (FactionMembershipBase)

```csharp
protected override OperationResult CanJoinFaction<TFaction>(in JoinFactionContext<PlayerController> ctx)
{
    // Block join if the player has a debuff.
    return _hasDebuff ? FactionOperations.Denied() : FactionOperations.Permitted();
}

protected override OperationResult CanBeDemoted<TFaction>(in FactionLevelChangeContext<PlayerController> ctx)
    => FactionOperations.Permitted();
```

---

## FactionAPI Reference

| Method | Description |
|---|---|
| `Join<TFaction, THolder>(membership)` | Join a faction |
| `Leave<TFaction, THolder>(membership)` | Leave a faction |
| `ChangeReputation<TFaction, THolder>(membership, amount)` | Add/subtract reputation; auto-evaluates level thresholds |
| `GetLevel<TFaction, THolder>(membership)` | Get the current reputation level (null if none) |
| `IsAtLeastLevel<TFaction, THolder>(membership, level)` | True if current level index >= target level index |
| `AssignLevel<TFaction, THolder>(membership, level)` | Manually set level; pass null to clear |

All methods accept an optional `ActionSource actionSource` parameter. Passing `ActionSource.Internal`
suppresses all event callbacks (useful for initialisation or batch operations).

---

## Key Types

| Type | Role |
|---|---|
| `FactionBase` / `FactionBase<TFactionObject>` | ScriptableObject config for a faction |
| `ReputationLevelBase` | ScriptableObject config for a reputation tier |
| `FactionMembershipBase<THolder>` | MonoBehaviour component — one per game object, manages all factions |
| `FactionDatabase` | Addressable database for faction assets |
| `ReputationLevelDatabase` | Addressable database for reputation level assets |
| `FactionAPI` | Static facade — the primary entry point for external code |
| `FactionOperations` | Result-code constants and `OperationResult` factories |
| `IForFaction<TFaction>` | Marker interface; triggers auto-assignment of levels to factions |

---

## Examples included

- `Scene - Factions.unity`: exposes runtime Unity UI for join, leave, reputation gain/loss, and manual level cases.
- `ExampleFactionsScene`: scene driver with runtime buttons and context menu actions for replaying the examples.
- `ExampleFaction`, `ExampleFactionMembership`, and `ExampleFactionHolder`: minimal typed faction setup.
- `ExampleReputationLevel`: auto-assigned reputation level example for `ExampleFaction`.
