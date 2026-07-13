# SimpleTutorial

SimpleTutorial is a small, condition-driven tutorial runner for Unity. A `TutorialBase` owns an ordered list of `TutorialStep` assets, activates one visible step at a time, and advances it automatically when its condition becomes true. Presentation stays entirely in the host game, so the runtime package has no UI dependency and does not require a `TutorialAPI`.

## Requirements

- Unity 6000.5+

### Assembly Definition

- `SimpleTutorial.asmdef`

## Setup

Create a concrete `TutorialStep` for every instruction. Implement `IsComplete` with the gameplay condition and, when needed, override `CanShow` to exclude a step from the current run. Add the assets to a component derived from `TutorialBase` in their intended order.

```csharp
using Systems.SimpleTutorial.Abstract;
using Systems.SimpleTutorial.Data;
using UnityEngine;

public sealed class OpenInventoryTutorialStep : TutorialStep
{
    protected override bool IsComplete(in TutorialStepContext context)
    {
        return PlayerInventory.IsOpen;
    }
}
```

Start a tutorial from the component that owns its presentation:

```csharp
using Systems.SimpleTutorial.Components;
using UnityEngine;

public sealed class PlayerTutorial : TutorialBase
{
    private void Start()
    {
        StartTutorial();
    }
}
```

## Lifecycle

`TutorialBase` evaluates the active step each frame and calls its own protected callbacks in this order:

1. `OnTutorialStarted`
2. `OnTutorialStepStarted`
3. `OnTutorialStepCompleted`
4. `OnTutorialCompleted`

`TutorialStep` receives matching protected `OnTutorialStepStarted` and `OnTutorialStepCompleted` callbacks. `CanShow` is evaluated when the runner reaches a step; a step that returns `false` is skipped for that run and is checked again after `RestartTutorial`.

Use `IsStepComplete<TStep>(in context)` inside a step condition when it depends on another configured step completing in the current tutorial run.

## Example included

Open `Examples/Scene - Tutorial.unity` and enter Play Mode. The `SimpleTutorial.Examples` assembly presents the current step through SimpleUI's `UIObjectBase` and `ScaleShowHideAnimation`. Press A, then B, then C to complete the three steps.
