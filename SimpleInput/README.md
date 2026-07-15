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

## Tests

EditMode tests are in `Tests/EditMode/SimpleInput.Tests.asmdef`.
