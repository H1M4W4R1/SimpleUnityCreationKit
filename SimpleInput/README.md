# SimpleInput

SimpleInput wraps Unity Input System binding lookup, display names, device filtering, and interactive rebinding.

## Requirements

- Unity Input System

## Setup

Call `InputAPI.Initialize()` once during application startup before listening for binding updates or running interactive rebinds.

```csharp
using Systems.SimpleInput;
using Systems.SimpleInput.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputBootstrap : MonoBehaviour
{
    [SerializeField] private InputActionReference _jumpActionReference;

    private void Awake()
    {
        InputAPI.Initialize();
        string displayName = InputAPI.GetBindingDisplayName(_jumpActionReference);
        bool started = InputAPI.Rebind(_jumpActionReference, InputDeviceType.Keyboard);
    }
}
```

`InputAPI` exposes extensions for `InputAction`, `InputActionReference`, and `InputActionAsset`. Use `InputDeviceType` to restrict binding lookup and rebinding to supported devices.

## Example scene

Open `Examples/Scene - Input Rebinding.unity` to try a self-contained keyboard rebinding flow. It creates four actions at runtime and uses `ExampleRuntimePanel` directly, so it does not require SimpleSettings. Each button changes from `Change <Action> Key` to `Press any key...`, then reports the newly assigned display name. Press Escape to cancel, or use the reset button to restore all defaults.

If the scene needs to be recreated, use **Simple Input > Regenerate Input Rebinding Example**.

## Tests

EditMode tests are in `Tests/EditMode/SimpleInput.Tests.asmdef`.
