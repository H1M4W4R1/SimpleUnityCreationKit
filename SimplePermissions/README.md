# SimplePermissions

`SimplePermissions` provides addressable permission and requirement assets for Unity. A `PermissionStorage` component keeps the explicit allow/deny overrides for one GameObject, while the configured ScriptableObject assets define the behavior shared by all owners.

## Requirements

- Unity 6000.5+
- SimpleCore

### Assembly Definition

- `SimplePermissions.asmdef`

## Setup

Create a sealed `PermissionBase` subclass in its own file. The inherited `AutoCreate` attribute creates an asset at `Assets/Generated/Permissions/` and registers it in Addressables with the `SimplePermissions.Permissions` label after Unity recompiles. Add `IAllowedByDefault` when the permission should be allowed until a storage explicitly denies it.

```csharp
using Systems.SimplePermissions.Abstract;

public sealed class BuildPermission : PermissionBase, IAllowedByDefault
{
}
```

Attach `PermissionStorage` to each GameObject that needs its own permission state. It resolves permission assets through `PermissionDatabase`; do not instantiate permissions with `new` or hold per-owner runtime state on the asset. Explicit overrides are retained as asset references and indexed at runtime by `HashIdentifier`, so lookups use binary search even when an owner has many permissions.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Components;
using UnityEngine;

public sealed class BuildingController : MonoBehaviour
{
    [SerializeField] private PermissionStorage _permissions;

    public bool CanBuild()
    {
        return _permissions.HasPermission<BuildPermission>();
    }

    public void UnlockBuilding()
    {
        OperationResult result = _permissions.TryGrant<BuildPermission>();
        if (!result) return;
    }
}
```

`TryGrant`, `TryDeny`, and `TryRevoke` return `OperationResult`. Revoke removes the explicit override and restores the permission default. Repeating the current state is a successful no-op (`AlreadyGranted`, `AlreadyDenied`, or `AlreadyRevoked`). A missing addressable permission returns `PermissionNotConfigured`; `HasPermission` returns `false` for a missing or destroyed definition.

Permission assets receive protected lifecycle callbacks with a `PermissionContext` that identifies the owner storage. Keep `Can...` callbacks side-effect free; use `On...` and `On...Failed` to respond after the decision.

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimplePermissions.Abstract;
using Systems.SimplePermissions.Data.Context;
using Systems.SimplePermissions.Operations;

public sealed class PaidFeaturePermission : PermissionBase
{
    protected override OperationResult CanBeGranted(in PermissionContext context)
    {
        return PlayerProfile.HasPaidSubscription
            ? PermissionOperations.Permitted()
            : PermissionOperations.PermissionRejected();
    }
}
```

## Requirements

Requirements are also auto-created and registered as addressables, under `Assets/Generated/Requirements/` with the `SimplePermissions.Requirements` label. Create a typed requirement in its own file and evaluate it with `RequirementAPI`.

```csharp
using Systems.SimplePermissions.Abstract;
using Systems.SimplePermissions.Utility;

public sealed class MinimumLevelRequirement : RequirementBase<int>
{
    public override bool IsMet(int level)
    {
        return level >= 5;
    }
}

bool isEligible = RequirementAPI.IsMet<MinimumLevelRequirement, int>(playerLevel);
```

`RequirementAPI.IsMetUnsafe<TRequirement>(object)` is available for integrations that only have an untyped context. Both evaluation methods return `false` when the required addressable asset is unavailable or destroyed.

## Example included

Open `Examples/Scene - Permissions.unity` and enter Play Mode. `ExampleBuildPermission` and `ExampleMinimumLevelRequirement` are automatically created. Use the Grant, Deny, and Revoke buttons to see explicit overrides replace each other and then return to the default state; use the level buttons to evaluate the generated requirement.
