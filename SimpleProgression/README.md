# SimpleProgression

SimpleProgression provides reusable Unity components for experience and level progression, plus a static facade for modifying progression on any GameObject.

## Requirements

- Unity 6000.5+
- SimpleCore assembly

The runtime assembly is `SimpleProgression`. Edit-mode tests are in `SimpleProgression.Tests`.

## Components

Derive `ExperienceControllerBase` for a component that stores experience. Use `LevelControllerBase` when level should be derived from experience thresholds.

`ExperienceBase` and `LevelBase` remain as obsolete compatibility names. New components should use the controller names.

```csharp
using Systems.SimpleProgression.Components;

public sealed class PlayerProgression : LevelControllerBase
{
    public override int GetMaxLevel() => 50;

    protected override ulong GetExperienceForLevel(int level)
    {
        return (ulong)(level * level * 100);
    }
}
```

Levels start at zero. The default curve requires one experience point per level. Override `GetExperienceForLevel` or `GetExperienceRequiredForLevel` for a custom curve. Curves should be monotonic and return zero or greater for level zero.

## ProgressionAPI

`ProgressionAPI` finds the required controller on the supplied GameObject and returns an `OperationResult` when the operation succeeds or fails.

```csharp
using Systems.SimpleProgression.Utility;
using UnityEngine;

public static class ProgressionExample
{
    public static void GrantReward(GameObject player)
    {
        ProgressionAPI.AddExperience(player, 250);
        ProgressionAPI.IncreaseLevel(player, 1);
    }
}
```

Available operations are `AddExperience`, `IncreaseExperience`, `TakeExperience`, `IncreaseLevel`, and `AddLevel`. Level increases reach the target level's experience threshold, so experience and level remain consistent.

## Callbacks

Override `OnExperienceChanged`, `OnExperienceAdded`, and `OnExperienceTaken` on experience controllers. Level controllers additionally expose `OnLevelIncreased`, `OnLevelChanged`, and `OnMaxLevelReached`. Level-increased callbacks run once per crossed level; level-changed callbacks run once for the resulting change.

## Operation results

`ProgressionOperations` reports successful experience/level changes and expected failures such as invalid amounts, insufficient experience, overflow, missing controllers, invalid level curves, and reaching a finite maximum level.

## Examples included

- `Scene - Progression.unity`: exposes runtime Unity UI for experience rewards, level jumps, spending experience, max-level checks, reset, and full-flow cases.
- `ExampleProgressionScene`: scene driver with runtime buttons and a context menu action for replaying the example.
- `ExampleProgressionController`: sample level controller with a finite max level and quadratic experience curve.

Use explicit types and indexed `for` loops in integrations to match the repository style.
