# SimpleAchievements

SimpleAchievements is a lightweight achievement system for Unity projects. It stores achievement definitions as ScriptableObjects, polls conditional achievements through `TickSystem`, broadcasts unlocks through SimpleIntegration's achievement contract, and can save unlock state either directly to disk or through SimpleSaving's save pipeline.

## Requirements

- Unity 6000.5+
- SimpleCore assembly
- SimpleIntegration assembly
- Unity Addressables package
- Unity ResourceManager package

### Assembly Definition

- `SimpleAchievements.asmdef`

## Architecture

```
AchievementData             - ScriptableObject definition and unlock callbacks
IProgressibleAchievement    - opt-in contract for active progress updates
AchievementRegistry         - runtime singleton that tracks unlock state
AchievementAPI              - static facade for unlock, progress, query, save, and load operations
IAchievementPlatform        - SimpleIntegration contract for external platform unlocks
AchievementSaveFile         - SimpleSaving save payload for unlocked platform IDs
AchievementsSettings        - Resources-backed save settings
```

Achievement definitions are discovered through `AchievementDatabase` using the `SimpleAchievements.Achievements` Addressables label. External platform support is supplied by SimpleIntegration implementations of `IAchievementPlatform`.

## Setup

Create concrete `AchievementData` assets for each achievement. Concrete subclasses inherit `[AutoCreate("Achievements", AchievementDatabase.LABEL)]`, so generated assets are placed under `Assets/Generated/Achievements/` and registered with the achievement label.

To forward unlocks to Steam, Epic, or another external platform, configure an `IAchievementPlatform` implementation in SimpleIntegration. See the SimpleIntegration README for setup.

Configure save behavior in `Edit > Project Settings > Achievements`:

| Setting | Purpose |
|---|---|
| Auto Save On Unlock | Writes the JSON save file whenever an achievement unlocks |
| Save File Name | File name under `Application.persistentDataPath` |

If the settings asset is missing in a player build, the system uses a transient default configuration with `AutoSaveOnUnlock = true` and `SaveFileName = "achievements.json"`.

## Manual Achievements

Manual achievements unlock through `AchievementAPI.Unlock`:

```csharp
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Operations;

public static class AchievementExample
{
    public static bool TryUnlockAchievement(AchievementData achievement)
    {
        AchievementUnlockContext context = new AchievementUnlockContext(achievement);
        OperationResult result = AchievementAPI.Unlock(in context);
        return result;
    }
}
```

## Conditional Achievements

Set `IsConditional` to `true` and override `EvaluateCondition`. The registry polls conditional achievements each tick and unlocks them automatically once the condition is met.

```csharp
using Systems.SimpleAchievements.Abstract;
using UnityEngine;

public sealed class DefeatTenEnemiesAchievement : AchievementData
{
    [SerializeField] private int _requiredEnemyCount = 10;

    public override bool IsConditional => true;

    protected override bool EvaluateCondition()
    {
        int defeatedEnemyCount = CombatStats.DefeatedEnemyCount;
        return defeatedEnemyCount >= _requiredEnemyCount;
    }
}
```

External code may force a conditional achievement to unlock by passing `forceUnlock: true`:

```csharp
AchievementUnlockContext context = new AchievementUnlockContext(achievement, forceUnlock: true);
OperationResult result = AchievementAPI.Unlock(in context);
```

Without `forceUnlock`, a conditional achievement unlock request fails with `AchievementOperations.ConditionNotMet()` until `EvaluateCondition()` returns `true`.

## Progress Achievements

Implement `IProgressibleAchievement` when gameplay should actively report progress instead of waiting for the registry tick. `AchievementAPI.NotifyProgress` invokes `UpdateProgress` once for each notification. Return `true` after applying the update when the achievement should unlock.

```csharp
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Utility;
using UnityEngine;

public sealed class DefeatOneThousandEnemiesAchievement : AchievementData, IProgressibleAchievement
{
    [SerializeField] private int _requiredEnemyKills = 1000;

    private int _enemyKillCount;

    public bool UpdateProgress()
    {
        _enemyKillCount++;
        return _enemyKillCount >= _requiredEnemyKills;
    }

    public void NotifyEnemyDefeated()
    {
        AchievementAPI.NotifyProgress(this);
    }
}
```

When the achievement type is known, `AchievementAPI.Unlock<TAchievement>()` resolves the generated definition from `AchievementDatabase`:

```csharp
OperationResult result = AchievementAPI.Unlock<DefeatTenEnemiesAchievement>();
```

`NotifyProgress` returns `AchievementOperations.ProgressUpdated()` until the goal is reached, then returns `AchievementOperations.Unlocked()`. Progress state belongs to the achievement implementation; store the source counter in the host game's save data when it must survive a restart.

Use `AchievementAPI.NotifyProgress<TAchievement>()` when the progressible achievement type is known and its generated definition should be resolved automatically.

## Validation Hooks

Override `CanUnlock` to add achievement-specific validation. Return an `OperationResult` from `AchievementOperations` for expected gameplay failures.

```csharp
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleCore.Operations;

protected override OperationResult CanUnlock(in AchievementUnlockContext context)
{
    OperationResult baseResult = base.CanUnlock(in context);
    if (!baseResult) return baseResult;
    if (!PlayerProfile.IsSignedIn) return AchievementOperations.InvalidAchievement();
    return AchievementOperations.Permitted();
}
```

Override `OnUnlocked` for one-time side effects. It is called when an achievement transitions to unlocked state, but not when restoring from a save file.

## Platform Integrations

SimpleAchievements broadcasts every unlock to all initialized `IAchievementPlatform` implementations configured by SimpleIntegration. Platform initialization, shutdown, SDK configuration, and availability checks belong to SimpleIntegration.

```csharp
using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Abstract.Features;
using UnityEngine;

public sealed class ConsolePlatform : IntegratedPlatformBase, IAchievementPlatform
{
    public override string PlatformName => "Console";

    public void UnlockAchievement(string platformId)
    {
        Debug.Log($"Unlock platform achievement: {platformId}");
    }

#if UNITY_EDITOR
    public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
    {
        UnityEditor.EditorGUILayout.HelpBox(
            "Configure console achievement credentials here.",
            UnityEditor.MessageType.Info);
    }
#endif
}
```

## Persistence

Use automatic disk persistence when achievements can manage their own file:

```csharp
AchievementAPI.Save();
AchievementAPI.Load();
```

Use the host save-system integration when achievement data should be embedded in a larger save file:

```csharp
using Systems.SimpleSaving.Abstract;

SaveFileBase saveFile = AchievementAPI.SaveToMemory();
if (!ReferenceEquals(saveFile, null))
{
    AchievementAPI.Load(saveFile);
}
```

## Operation Results

`AchievementAPI.Unlock` returns:

| Result | Meaning |
|---|---|
| `AchievementOperations.Unlocked()` | Achievement transitioned to unlocked |
| `AchievementOperations.ProgressUpdated()` | Progress notification was accepted and the achievement remains locked |
| `AchievementOperations.AlreadyUnlocked()` | Achievement was already unlocked |
| `AchievementOperations.InvalidAchievement()` | Achievement reference, destroyed state, or platform ID is invalid |
| `AchievementOperations.ConditionNotMet()` | Conditional achievement was requested before its condition was met |
| `AchievementOperations.NotProgressible()` | Achievement does not implement `IProgressibleAchievement` |

## Examples included

- `Scene - Achievements.unity`: exposes runtime Unity UI for manual, conditional, blocked, forced, and reset unlock cases.
- `ExampleAchievementsScene`: scene driver with runtime buttons and context menu actions for replaying the examples.
- `ExampleManualAchievement` and `ExampleConditionalAchievement`: configured example assets used by the scene.
- `SteamPlatform` and `EpicPlatform` are supplied by SimpleIntegration as mocked platform integrations.

## Notes

- Platform IDs must be non-empty and match the external SDK configuration exactly.
- Unlock state is stored by platform ID, not by asset reference.
- Restoring save data updates local unlock state only; it does not replay `OnUnlocked` or platform unlock calls.
- Use indexed `for` loops in achievement examples and integrations to match the repository style.
