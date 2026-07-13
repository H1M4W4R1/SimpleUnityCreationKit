# SimpleCrafting

SimpleCrafting provides a small, inventory-agnostic crafting transaction. `CraftingAPI` owns the common flow; each `CraftingRecipeBase` owns its own rules and side effects through protected overrides.

The package depends on `SimpleCore` only. It does not impose item, currency, skill, quest, category, ingredient, or output data models.

## Setup

1. Create a concrete `CraftingRecipeBase` ScriptableObject.
2. Put the recipe's validation and side effects in its crafting overrides.
3. Implement `ICraftingUser` on the game-specific actor or owner passed to the API.
4. Optionally pass one or more `CraftingStationBase` components from the scene.
5. Implement `ICraftingTimed` only on recipes that should create a timed `CraftingInstance`.
6. Add `CraftingTickSystem` to one persistent scene object if timed instances should follow the global SimpleCore tick automatically.

Recipes are auto-created under `Assets/Generated/CraftingRecipes/` and registered with the `SimpleCrafting.Recipes` Addressables label. Stations are ordinary in-world components and are not generated assets.

## Why there is no processor

The old `ICraftingProcessor` split one recipe operation across a generic adapter: the API validated every serialized ingredient/output, then called the processor once per asset. That made the processor responsible for domain behavior while the recipe remained a passive container, added a required indirection to every call, and made atomic multi-item operations awkward.

The API now coordinates only the stable transaction:

1. Validate the recipe, supplied stations, ingredients, and result.
2. Consume ingredients.
3. Complete immediately or create a timed instance.
4. Grant the result, or refund ingredients when completion fails.

The recipe implements each domain-specific step as one operation. This keeps multi-item transactions, currencies, skill checks, and custom crafting rules together and lets systems with no conventional ingredient/output model use the same package.

## Recipe overrides

```csharp
using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Operations;

namespace Game.Crafting
{
    public sealed class IronSwordRecipe : CraftingRecipeBase, ICraftingTimed
    {
        public float DurationSeconds => 3f;

        protected internal override OperationResult CanConsumeCraftingIngredients(
            in CraftingContext context)
        {
            return HasRequiredMaterials(context.user)
                ? CraftingOperations.Permitted()
                : CraftingOperations.Denied();
        }

        protected internal override OperationResult TryConsumeCraftingIngredients(
            in CraftingContext context)
        {
            return RemoveRequiredMaterials(context.user)
                ? CraftingOperations.Permitted()
                : CraftingOperations.Denied();
        }

        protected internal override OperationResult TryRefundCraftingIngredients(
            in CraftingContext context)
        {
            return ReturnRequiredMaterials(context.user)
                ? CraftingOperations.Permitted()
                : CraftingOperations.Denied();
        }

        protected internal override OperationResult CanGrantCraftingResult(
            in CraftingContext context)
        {
            return HasResultSpace(context.user)
                ? CraftingOperations.Permitted()
                : CraftingOperations.Denied();
        }

        protected internal override OperationResult TryGrantCraftingResult(
            in CraftingContext context)
        {
            return AddSword(context.user)
                ? CraftingOperations.Completed()
                : CraftingOperations.Denied();
        }

        private bool HasRequiredMaterials(ICraftingUser user) => true;
        private bool RemoveRequiredMaterials(ICraftingUser user) => true;
        private bool ReturnRequiredMaterials(ICraftingUser user) => true;
        private bool HasResultSpace(ICraftingUser user) => true;
        private bool AddSword(ICraftingUser user) => true;
    }
}
```

`CanConsumeCraftingIngredients` and `CanGrantCraftingResult` must describe the same conditions their `Try...` methods rely on. The consume, refund, and grant methods should be atomic: either complete the whole operation or leave the external store unchanged.

General permissions and custom conditions belong in the recipe's `CanCraft` override. There are no condition ScriptableObjects to configure or remember.

## Crafting users

The context carries `ICraftingUser`, not `object`, so integrations can require an explicit compliance marker:

```csharp
using Systems.SimpleCrafting.Abstract;

public sealed class PlayerCraftingUser : ICraftingUser
{
    public int CraftingLevel { get; set; }
}
```

## Starting and completing crafting

```csharp
OperationResult result = CraftingAPI.TryStartCrafting(
    recipe,
    out CraftingInstance instance,
    user: player);

if (!result) return;
if (instance is null) return; // The recipe completed immediately.

CraftingAPI.AdvanceCrafting(instance, deltaTime);
if (instance.IsReadyToComplete)
{
    CraftingAPI.TryCompleteCrafting(instance);
}
```

Timed state is caller-owned. A component, save system, job queue, or entity system can store `CraftingInstance` and advance it using its own clock.

To use the current global tick, add one `Systems.SimpleCrafting.Components.CraftingTickSystem` component to a persistent scene object. It subscribes to `SimpleCore.Timing.TickSystem`, advances every timed instance started through `CraftingAPI`, and completes instances when their duration elapses. Do not add it more than once.

## Stations

Stations are in-world `MonoBehaviour` components:

```csharp
using Systems.SimpleCrafting.Abstract;

public sealed class WorkbenchStation : CraftingStationBase
{
}
```

Pass the station that is being used to the API. Its `CanUseStation` override can validate ownership, distance, state, recipe compatibility, or any other station-specific rule. A recipe that needs multiple stations can receive an `IReadOnlyList<CraftingStationBase>` through `CraftingContext`.

```csharp
OperationResult result = CraftingAPI.CanCraft(recipe, workbench, player);
```

There is no station requirement list on recipes. Recipes that need a particular station decide that in `CanCraft`, while station-local availability remains in `CraftingStationBase`.

## Recipe discovery

Recipe assets remain available through `CraftingRecipeDatabase.GetAllRecipes()`. Categories and station-based queries are intentionally not part of the base system; projects can classify recipes through their own recipe subclasses, interfaces, databases, or UI-facing registries.

## Examples included

- `Scene - Crafting.unity`: exposes runtime Unity UI for instant, blocked, timed-start, timed-advance, and completion cases.
- `ExampleCraftingScene`: scene driver, runtime UI controller, and sample `IExampleCraftingLevelProvider` implementation.
- `ExampleSimpleItemRecipe`: minimal recipe asset type.
- `ExampleStationRecipe`: recipe type intended for station-driven examples.
- `ExampleTimedRecipe`: opts into `ICraftingTimed`.
- `ExampleBlockedRecipe`: demonstrates a recipe-level permission failure.
- `ExampleWorkbenchStation`: in-world station component.
- `IExampleCraftingLevelProvider`: example `ICraftingUser` integration contract.
