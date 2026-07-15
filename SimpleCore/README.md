# SimpleCore

SimpleCore is the shared foundation for the Simple systems. It provides identifiers (including `IIdentifiable<TIdentifier>` for stable object IDs), operation results, addressable storage, timing, contract-driven behaviours, automation, localization helpers, math utilities, and the shared example UI helper.

## Dependencies

- Unity Addressables and ResourceManager
- Unity Burst, Collections, and Mathematics
- Unity Localization
- Unity uGUI

Input rebinding is provided by **SimpleInput**. Save-file abstractions and conversion pipelines are provided by **SimpleSaving**.

## Included modules

- `Automation/` – ScriptableObject and addressable generation automation.
- `Identifiers/` – fixed-size IDs, `HashIdentifier`, and `Snowflake128`.
- `Operations/` – allocation-free `OperationResult` values.
- `Storage/` – addressable and live runtime databases, plus pooled list-access wrappers.
- `Timing/` – global tick scheduling.
- `Utility/` – localization and math helpers.
- `Examples/` – `ExampleRuntimePanel`, used by package example scenes.

## SimpleBehaviour

Derive runtime components from `SimpleBehaviour` and add only the contracts they need. Contracts are markers: no manual Unity-message forwarding, tick subscription, persistence call, singleton guard, or database lookup is required. Override the matching protected hook when the component has work to do.

```csharp
using Systems.SimpleCore.Behaviours;

public sealed class ExampleTickingBehaviour : SimpleBehaviour, ITickableBehaviour, IActiveUpdate
{
    protected override void SetupAndValidateComponents()
    {
    }

    protected override void OnTick(float deltaTimeSeconds)
    {
    }

    protected override void OnBehaviourActiveUpdated()
    {
    }
}
```

An identifiable behaviour receives its missing `Snowflake128` before `SetupAndValidateComponents` and `Initialize`, which then run at the beginning of Awake in that order. The base class guards Awake so initialization cannot run more than once. `IPersistentBehaviour` preserves the GameObject across scene loads and `IUniqueBehaviour` keeps one live instance of the concrete component type. `IRegisterInDatabase<TDatabase>` registers the component on Awake and unregisters it on destruction. A component should select only one runtime database for automatic registration.

`UpdateSystem` is created automatically before the first scene and is the sole Unity component with Update, FixedUpdate, and LateUpdate callbacks. Use `IActiveUpdate`, `IInactiveUpdate`, or `IAlwaysUpdate` for frame updates; use the equivalent `IActiveFixedUpdate` / `IInactiveFixedUpdate` / `IAlwaysFixedUpdate` and `IActiveLateUpdate` / `IInactiveLateUpdate` / `IAlwaysLateUpdate` contracts for fixed and late phases. Active and inactive dispatch uses a bool cached by `SimpleBehaviour.OnEnable` and `OnDisable`, not `isActiveAndEnabled` checks during each update.

## Tests

EditMode tests are in `Tests/EditMode/SimpleCore.Tests.asmdef`. Run them from Unity Test Runner after changing core APIs.
