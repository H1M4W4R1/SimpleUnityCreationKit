# SimpleBuilding

SimpleBuilding provides an inventory-agnostic transaction for placing and demolishing world buildings. `BuildingAPI` owns the common validation, resource, instantiation, slot-reservation, refund, and callback flow. A concrete `BuildingEntryBase` owns game-specific availability, costs, permissions, and rewards.

## Setup

1. Create a concrete `BuildingEntryBase` asset and assign its completed `BuildingBase` prefab.
2. Override the entry's `CanBuild`, `TryConsumeResources`, `CanDemolish`, and `TryRefundResources` methods for game rules.
3. Add a `BuildingRaycasterBase` implementation to the player or controller. Use `CameraBuildingRaycaster` for center-screen building or `PointerBuildingRaycaster` for mouse/UI pointer building.
4. Optionally add `BuildingGhostPreview` to the same controller, then assign a `BuildingGhostMaterialConfiguration` with valid and invalid materials.
5. If a building needs fixed positions, implement `ISlotBuilding` on its `BuildingBase` prefab and place matching `BuildingSlot` components in the scene.

Building entry assets are auto-created under `Assets/Generated/Buildings/` and registered with the `SimpleBuilding.Buildings` Addressables label.

## Entry rules and callbacks

Every override receives the operation context. The placement context includes the entry, user, controller, parent, position, rotation, and candidate slots; the demolition context includes the building, user, and controller.

```csharp
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleCore.Operations;

namespace Game.Buildings
{
    public sealed class WoodWallEntry : BuildingEntryBase
    {
        protected internal override OperationResult CanBuild(in BuildingPlacementContext context)
        {
            return HasWood(context.user, 5)
                ? BuildingOperations.Permitted()
                : BuildingOperations.Denied();
        }

        protected internal override OperationResult TryConsumeResources(in BuildingPlacementContext context)
        {
            return RemoveWood(context.user, 5)
                ? BuildingOperations.Permitted()
                : BuildingOperations.Denied();
        }

        protected internal override void OnBuildingPlaced(
            in BuildingPlacementContext context,
            BuildingBase building,
            in OperationResult result)
        {
            // Update game-specific state after a successful placement.
        }

        private bool HasWood(IBuildingUser user, int amount) => true;
        private bool RemoveWood(IBuildingUser user, int amount) => true;
    }
}
```

Resource consumption and refunds should be atomic. `TryRefundResources` is invoked before a building is destroyed, so a failed refund leaves the building intact. Placement failures invoke `OnBuildingPlacementFailed`; demolition failures invoke `OnBuildingDemolitionFailed` on both the entry and the existing building instance.

## Saving placed buildings

SimpleBuilding saves the buildings created through `BuildingAPI` with the SimpleCore save pipeline. It stores each entry's `SaveIdentifier`, world transform, local scale, and any reserved slot identifiers. Give every entry and scene slot a unique, stable identifier before shipping. When no identifier is assigned, the asset or GameObject name is used as a compatibility fallback.

Before loading, register every entry that can appear in the save; active `BuildingSlot` components register themselves. Loading replaces the currently API-placed buildings without consuming resources or issuing refunds, while still invoking placement and demolition callbacks. Save-driven placement and removal contexts set `isSaveSystemRequest` to `true`, so custom rules can recognize the source explicitly.

```csharp
using System.Collections.Generic;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data.SaveFiles;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Saving.Abstract;

namespace Game.Buildings
{
    public sealed class BuildingSaveController
    {
        private readonly IReadOnlyList<BuildingEntryBase> _entries;

        public BuildingSaveController(IReadOnlyList<BuildingEntryBase> entries)
        {
            _entries = entries;
        }

        public SaveFileBase Save()
        {
            return BuildingAPI.SaveToMemory();
        }

        public void Load(BuildingSaveFile saveFile)
        {
            BuildingAPI.RegisterEntries(_entries);
            BuildingAPI.Load(saveFile);
        }
    }
}
```

`SaveToMemory` returns a `SaveFileBase` so it can be embedded in a host game's larger save file. The host owns disk serialization and versioning, while `BuildingAPI.Load` accepts the same SimpleCore save-file base type.

## Selecting, rotating, and placing

```csharp
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Game.Buildings
{
    public sealed class BuildingInput : MonoBehaviour
    {
        [SerializeField] private BuildingRaycasterBase _raycaster;
        [SerializeField] private BuildingEntryBase _wallEntry;

        public void SelectWall()
        {
            OperationResult result = _raycaster.Select(_wallEntry);
            if (!result) return;
        }

        public void RotateClockwise()
        {
            _raycaster.Rotate();
        }

        public void Place()
        {
            OperationResult result = _raycaster.TryBuild(out BuildingBase building);
            if (!result) return;
        }
    }
}
```

`Rotate(int steps)` rotates in the configured inspector increment (90 degrees by default). `SetRotation(float rotationDegrees)` sets an exact yaw. The ghost and placement both use the same final rotation. `PointerBuildingRaycaster.SetPointerPosition(Vector2)` can be called from an EventSystem pointer callback; when no position is supplied it can use the legacy mouse position.

## Ghost previews

`BuildingGhostPreview` instantiates `GhostPrefab` from the selected entry, or falls back to the completed building prefab when no dedicated ghost is assigned. It disables `MonoBehaviour`, 3D collider, and 2D collider components on that transient instance. Prefer a dedicated mesh-only `GhostPrefab` when the completed prefab has runtime side effects in `Awake`.

`BuildingGhostMaterialConfiguration` applies one material while placement validates and another while it does not. These are ordinary material assets, so a custom shader can show fresnel, outlines, dissolve, grid projection, or any other ghost effect without changing the building system.

## Example scene

`Scene - Building Playground.unity` demonstrates free placement, slot-only placement, valid/invalid shader-ready ghosts, rotation, and demolition. Open it and enter Play mode:

- Use the SimpleCore runtime panel to select the free-placement cube or the one-slot cylinder, rotate the selected preview, and save/load/clear the API-placed buildings in memory.
- Left-click builds and right-click demolishes.

The generated materials, prefabs, and entry assets are stored in the package's `Examples` folder. Use **Simple Building/Regenerate Building Playground** to recreate the complete example after changing its generator.

## Slot buildings

Implement `ISlotBuilding` on a building prefab to require a fixed number of unoccupied slots:

```csharp
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;

namespace Game.Buildings
{
    public sealed class FourTileFoundation : BuildingBase, ISlotBuilding
    {
        public int SlotCount => 4;
    }
}
```

The default raycaster contributes the `BuildingSlot` on the hit object. Override `CollectPlacementSlots(in RaycastHit hit, List<BuildingSlot> slots)` in a custom raycaster to collect a grid footprint or other multi-slot layout. The API validates the exact count, rejects duplicates and occupied slots, reserves slots on placement, and releases them on demolition or destruction.

`ISlotBuilding.SnapToSlot` defaults to `true`: the raycaster places the building at the selected slot position, or at the average of a multi-slot footprint. It also uses the first slot's rotation as the base rotation, then applies the caller's configured yaw. Override `SnapToSlot` as `false` only when a custom building needs to derive its position independently.
