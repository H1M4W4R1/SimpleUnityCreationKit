# SimpleIntegration

SimpleIntegration centralizes optional external-platform SDK adapters. Systems query a feature contract instead of owning their own platform adapters, so a single Steam or Epic integration can provide multiple capabilities.

## Requirements

- Unity 6000.5+
- SimpleCore assembly
- Unity Addressables package
- Unity ResourceManager package

## Setup

Create a concrete `IntegratedPlatformBase` for each external SDK. Concrete implementations inherit `[AutoCreate("IntegratedPlatforms", IntegratedPlatformDatabase.LABEL)]`, which creates their configuration asset under `Assets/Generated/IntegratedPlatforms/` and registers it with the `SimpleIntegration.Platforms` Addressables label.

Implement each feature contract supported by the platform. For example, `IAchievementPlatform` makes the integration available to SimpleAchievements. A contract is returned only while its `IntegratedPlatformBase.IsInitialized` property is true. No scene is required: the integration API initializes configured platforms before the first scene loads.

```csharp
using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Abstract.Features;
using UnityEngine;

public sealed class ConsolePlatform : IntegratedPlatformBase, IAchievementPlatform
{
    public override string PlatformName => "Console";

    public override void Initialize()
    {
        // Set IsInitialized to the SDK initialization result in a real implementation.
        IsInitialized = true;
    }

    public void UnlockAchievement(string achievementId)
    {
        Debug.Log($"Unlock platform achievement: {achievementId}");
    }

#if UNITY_EDITOR
    public override void DrawSettings(UnityEditor.SerializedObject serializedObject)
    {
    }
#endif
}
```

## Contract availability

Use `IntegrationAPI.IsAvailable<TContract>()` when only availability is needed. Use `GetAvailable<TContract>` to broadcast an operation to every configured provider. Release returned lists after use.

```csharp
using Systems.SimpleIntegration.Abstract.Features;
using Systems.SimpleIntegration.Utility;

if (IntegrationAPI.IsAvailable<IAchievementPlatform>())
{
    // At least one configured platform can unlock achievements.
}
```

## Examples included

- `SteamPlatform`: mocked Steamworks achievement integration.
- `EpicPlatform`: mocked Epic Online Services achievement integration.

Replace the mock bodies with calls to the corresponding SDK after installing it. Configure each integration from `Edit > Project Settings > Integrations`.
