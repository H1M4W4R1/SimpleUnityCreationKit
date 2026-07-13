# SimpleInteract
SimpleInteract is a lightweight, performant interaction detection system for Unity that enables game objects to interact with each other based on proximity. It provides a flexible framework for handling 2D and 3D interactions with built-in support for detection callbacks and interaction validation.

## Requirements

- **Unity 2022.3+**
- **SimpleCore** - Dependency for operation result handling
- **SimpleDetection** - Dependency for object detection (Circle2D and Sphere3D detectors)

## References

The assembly definition includes the following dependencies:
- `Unity.Burst` - Burst compilation support
- `Unity.Collections` - Collections framework
- `Unity.Mathematics` - Mathematics utilities
- `SimpleDetection` - Detection framework
- `SimpleCore` - Core operation utilities

## Features

- **2D and 3D Support** - Use `InteractableDetector2D` for 2D games or `InteractableDetector3D` for 3D games
- **Performance Optimized** - Override `CanBeDetected()` on interactable objects to filter which interactors can detect them, reducing unnecessary detection work
- **Event-Driven Architecture** - Hooks for detection start/end and successful/failed interactions
- **Permission-Based Validation** - Override `CanInteract()` and `CanBeInteractedWith()` for custom rules
- **Batch Interactions** - Interact with single or multiple objects in range

## Usage

### Create an Interactable Object

```csharp
public sealed class Chest : InteractableObjectBase
{
    protected override void OnInteract(in InteractionContext context, in OperationResult result)
    {
        Debug.Log("Chest opened by: " + context.interactor.gameObject.name);
        // Handle interaction logic
    }

    protected override void OnInteractFailed(in InteractionContext context, in OperationResult result)
    {
        Debug.Log("Failed to interact with chest");
    }
}
```

In the Unity Inspector:
1. Add a `Chest` component to your GameObject
2. Add an `InteractableDetector2D` or `InteractableDetector3D` component (required)
3. Configure the detector's collision layer and radius

> **Note:** Do not override `Awake` or `OnDestroy` in subclasses of `InteractableObjectBase`. These methods manage detector event subscriptions and are intentionally non-virtual. Use `OnInteractionZoneEnter` and `OnInteractionZoneExit` for lifecycle hooks instead.

### 2. Create an Interactor

The simplest interactor is an empty marker class — detection and permission logic lives on the interactable side:

```csharp
public sealed class Player : InteractorBase { }
```

Override `CanInteract()` when the interactor itself needs to impose conditions:

```csharp
public sealed class Player : InteractorBase
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OperationResult result = Interact();
            if (!result) Debug.Log("No interactable objects in range");
        }
    }

    protected internal override OperationResult CanInteract(InteractionContext context)
    {
        if (!HasEnoughMana())
            return InteractOperations.Denied();

        return InteractOperations.Permitted();
    }
}
```

In the Unity Inspector:
- Add the `Player` component to your character GameObject
- The player will detect nearby interactable objects automatically

### Restricting Which Interactors Can Detect and Interact

Override `CanBeDetected()` to limit which interactors can enter the detection zone, and `CanBeInteractedWith()` to add runtime conditions at interaction time. Both checks are evaluated per-interactor.

```csharp
// Marker class — logic lives on the interactable side
public sealed class Player : InteractorBase { }

public sealed class QuestNPC : InteractableObjectBase
{
    protected internal override OperationResult CanBeDetected(ObjectDetectionContext context)
    {
        // Only players can enter this object's interaction zone
        if (context.detectableObject is not Player)
            return InteractOperations.Denied();

        return InteractOperations.Permitted();
    }

    protected internal override OperationResult CanBeInteractedWith(InteractionContext context)
    {
        // Checked again at interaction time — add runtime conditions here
        if (context.interactor is not Player player || !player.HasQuestItem)
            return InteractOperations.Denied();

        return InteractOperations.Permitted();
    }

    protected override void OnInteract(in InteractionContext context, in OperationResult result)
    {
        Debug.Log("Quest NPC interacted with");
    }

    protected override void OnInteractFailed(in InteractionContext context, in OperationResult result)
    {
        Debug.Log("Interaction failed");
    }
}
```

### Handling Zone Enter/Exit

```csharp
public sealed class TrapFloor : InteractableObjectBase
{
    private bool _isTriggered;

    protected override void OnInteractionZoneEnter(InteractorBase obj)
    {
        Debug.Log($"{obj.gameObject.name} entered the trap zone");
    }

    protected override void OnInteractionZoneExit(InteractorBase obj)
    {
        Debug.Log($"{obj.gameObject.name} left the trap zone");
    }

    protected override void OnInteract(in InteractionContext context, in OperationResult result)
    {
        if (!_isTriggered)
        {
            _isTriggered = true;
            ActivateTrap();
        }
    }
}
```

### Batch Interactions

`InteractAll` attempts to interact with every object currently in range and returns the count via an out parameter.

```csharp
public sealed class Player : InteractorBase
{
    private void OnPressInteractAll()
    {
        OperationResult result = InteractAll(out int interactionsCount);

        if (result)
            Debug.Log($"Interacted with {interactionsCount} objects");
        else
            Debug.Log("No interactable objects in range");
    }
}
```

## API Reference

### InteractorBase
- `Interact()` - Interact with the first detected object
- `InteractAll(out int count)` - Interact with all detected objects
- `CanInteract(InteractionContext)` - Virtual method to control interaction permissions

### InteractableObjectBase
- `Interactors` - Read-only list of nearby interactors in range
- `OnInteract()` - Called when interaction succeeds
- `OnInteractFailed()` - Called when interaction fails
- `OnInteractionZoneEnter()` - Called when interactor enters detection zone
- `OnInteractionZoneExit()` - Called when interactor exits detection zone
- `CanBeInteractedWith()` - Virtual method to validate interaction
- `CanBeDetected()` - Virtual method to control detection permissions

### Detectors
- `InteractableDetector2D` - Circle-based detection for 2D games
- `InteractableDetector3D` - Sphere-based detection for 3D games