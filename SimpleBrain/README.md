# SimpleBrain

`SimpleBrain` provides a lightweight, extensible AI foundation for actor GameObjects. A `BrainBase` component owns per-actor knowledge, short-lived decisions, and reusable long-running subprocesses. It integrates with SimpleCore's global tick system and exposes all operations through the brain component; there is no static API.

## About

Use SimpleBrain when an actor needs to remember learned facts, make typed queries from that state, and run independent behaviours such as scanning, patrolling, or targeting. The system separates those responsibilities deliberately:

- **Knowledge** is persistent, per-brain data or capability state.
- **Decisions** are created for one query and return a value immediately.
- **Subprocesses** are one-per-type, brain-owned state machines that can run, pause, resume, stop, or finish.
- **Coma** suspends normal brain activity while preserving processes explicitly allowed to run.

## Requirements

- **Unity 6000.5 or later**
- **SimpleCore** assembly

`BrainBase` automatically registers with SimpleCore's `TickSystem` while its GameObject is enabled. No scene singleton or manual tick registration is required.

## Features

- Per-brain managed-reference knowledge storage
- Typed and untyped decisions with validation and success/failure callbacks
- Reusable, one-instance-per-type subprocesses
- Explicit start, stop, pause, resume, and finish lifecycle operations
- Coma handling that pauses ordinary processes and continues opted-in processes
- Stack-only `BrainContext` and `BrainSubprocessContext` callback data
- `OperationResult` outcomes for expected gameplay failures

## Quick Start

### Create a brain component

Inherit from `BrainBase` and add the component to the actor it controls. The brain is intentionally attached to the actor rather than caching a separate actor component.

```csharp
using Systems.SimpleBrain.Components;

public sealed class GuardBrain : BrainBase
{
    protected override void OnBrainTick(float deltaTimeSeconds)
    {
        // Perform actor-wide work while the brain is awake.
    }

    protected override void OnBrainComaTick(float deltaTimeSeconds)
    {
        // Perform actor-wide work while the brain is in a coma.
    }
}
```

`OnBrainBorn()` runs once when the component is created. `OnBrainTick(float)` runs on each SimpleCore tick while awake; `OnBrainComaTick(float)` runs instead while in coma.

## Knowledge

Knowledge is created by `TryLearn<TKnowledge>()` and belongs to one brain. The brain stores one instance of each concrete knowledge type, retaining its serialized data for later access. Types learned with `TryLearn<TKnowledge>()` must have a public parameterless constructor. Mark concrete knowledge types with `[Serializable]` when their state must be Unity-serialized.

```csharp
using System;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;

[Serializable]
public sealed class GuardKnowledge : KnowledgeBase
{
    public int alertness;
    public bool isAlert;

    protected override OperationResult CanLearn(in BrainContext context)
    {
        return BrainOperations.Permitted();
    }

    protected override bool IsKnown(in BrainContext context)
    {
        return isAlert;
    }
}
```

```csharp
using Systems.SimpleCore.Operations;

OperationResult learnResult = guardBrain.TryLearn<GuardKnowledge>();
if (!learnResult) return;

bool hasLearned = guardBrain.HasLearned<GuardKnowledge>();
bool knows = guardBrain.Knows<GuardKnowledge>();

if (guardBrain.TryGetKnowledge(out GuardKnowledge knowledge))
    knowledge.alertness = 3;
```

`HasLearned<TKnowledge>()` reports whether the brain owns the knowledge. `Knows<TKnowledge>()` also evaluates `KnowledgeBase.IsKnown`, so learned knowledge may be temporarily unavailable. Calling `TryLearn<TKnowledge>()` again preserves the original instance and returns the successful `KnowledgeAlreadyLearned` result.

Override these callbacks to customize the knowledge lifecycle:

- `CanLearn(in BrainContext)` validates learning.
- `IsKnown(in BrainContext)` determines current availability.
- `OnLearned(in BrainContext, in OperationResult)` runs after storage succeeds.
- `OnLearningFailed(in BrainContext, in OperationResult)` runs when validation fails.

## Decisions and assessments

Decisions are short-lived queries. Each `TryDecide` or `TryAssess` call creates a new decision instance, evaluates `CanDecide`, then returns its result. Keep durable state in knowledge, the brain, or the actor—not on a decision instance.

```csharp
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Data.Context;

public sealed class ShouldInvestigateDecision : DecisionBase<bool>
{
    protected override bool DecideTyped(in BrainContext context)
    {
        return context.brain.TryGetKnowledge(out GuardKnowledge knowledge) &&
               knowledge.alertness > 0;
    }
}
```

```csharp
using Systems.SimpleCore.Operations;

OperationResult decisionResult = guardBrain.TryDecide<ShouldInvestigateDecision, bool>(
    out bool shouldInvestigate);
if (!decisionResult) return;

OperationResult assessmentResult = guardBrain.TryAssess<ShouldInvestigateDecision, bool>(
    out bool shouldAssess);
```

`TryAssess` is an alias for `TryDecide`. Use the one-generic-argument overloads for `DecisionBase` implementations that return an `object`; use the two-generic-argument overloads for `DecisionBase<TDecisionResult>`.

Override `CanDecide`, `OnDecided`, and `OnDecisionFailed` to validate a decision and react to its outcome. If `CanDecide` returns an error, the decision result is the default value of its result type.

## Subprocesses

Subprocesses model long-running behaviours. A brain creates at most one subprocess instance for each concrete subprocess type. Stopping a process keeps that instance, so a later start reuses its state. Types started with `TryStartSubprocess<TSubprocess>()` must have a public parameterless constructor.

```csharp
using System;
using Systems.SimpleBrain.Components;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleCore.Operations;
using UnityEngine;

[Serializable]
public sealed class ScanForThreatsSubprocess : BrainSubprocessBase
{
    private float _scanElapsedSeconds;

    protected override void OnTick(in BrainSubprocessContext context)
    {
        _scanElapsedSeconds += context.deltaTimeSeconds;

        if (_scanElapsedSeconds < 1f) return;

        _scanElapsedSeconds = 0f;
        // Inspect the actor's surroundings here.
    }

    protected override void OnStopped(in BrainSubprocessContext context, in OperationResult result)
    {
        _scanElapsedSeconds = 0f;
    }
}
```

Use the brain's public lifecycle methods to control a subprocess:

```csharp
using Systems.SimpleCore.Operations;

OperationResult startResult = guardBrain.TryStartSubprocess<ScanForThreatsSubprocess>();
OperationResult pauseResult = guardBrain.TryPauseSubprocess<ScanForThreatsSubprocess>();
OperationResult resumeResult = guardBrain.TryResumeSubprocess<ScanForThreatsSubprocess>();
OperationResult stopResult = guardBrain.TryStopSubprocess<ScanForThreatsSubprocess>();

bool isCreated = guardBrain.IsSubprocessCreated<ScanForThreatsSubprocess>();
bool isRunning = guardBrain.IsSubprocessRunning<ScanForThreatsSubprocess>();
bool isPaused = guardBrain.IsSubprocessPaused<ScanForThreatsSubprocess>();
```

### Lifecycle rules

| Operation | Valid state | Resulting state | Notes |
| --- | --- | --- | --- |
| `TryStartSubprocess` | Stopped or not yet created | Running | Creates the instance when necessary. Starting an already running process returns a successful already-running result. Starting a paused process returns an error. |
| `TryPauseSubprocess` | Running | Paused | Returns an error when the process is missing or not running. |
| `TryResumeSubprocess` | Paused | Running | Returns an error when the process is missing or not paused. |
| `TryStopSubprocess` | Running or paused | Stopped | Keeps the subprocess instance for a later restart. Stopping an already stopped process returns a successful already-stopped result. |
| `Finish(context)` | Running | Stopped | Call from inside the subprocess when it completes normally. This invokes `OnFinished`. |

`Finish(in BrainSubprocessContext)` is protected, so only a subprocess can finish itself using the context supplied to one of its callbacks.

Override the lifecycle hooks that apply to the process:

- Validation: `CanStart`, `CanStop`, `CanPause`, `CanResume`
- Successful transitions: `OnStarted`, `OnStopped`, `OnPaused`, `OnResumed`, `OnFinished`
- Rejected transitions: `OnStartFailed`, `OnStopFailed`, `OnPauseFailed`, `OnResumeFailed`
- Running work: `OnTick`

Every validation hook returns an `OperationResult`. When it returns an error, the state does not change and the matching failure callback runs.

## Coma

Coma is a brain-wide suspended state controlled by `TryEnterComa()` and `TryExitComa()`. While in coma, the brain calls `OnBrainComaTick` instead of `OnBrainTick`.

```csharp
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;

OperationResult enterComaResult = guardBrain.TryEnterComa();
if (!enterComaResult) return;

// The brain and ordinary subprocesses are now suspended.

OperationResult exitComaResult = guardBrain.TryExitComa();
```

On entering coma, the brain attempts to pause every running subprocess that does not opt in to coma execution. On exit, it resumes only processes that were paused by that coma entry. A manually paused process remains paused after coma ends.

```csharp
using System;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Components;

[Serializable]
public sealed class VitalSignsSubprocess : BrainSubprocessBase, ISubprocessAllowedInComa
{
    // This process remains running and receives OnTick calls during coma.
}
```

The `BrainSubprocessContext.isComaInduced` field is `true` for automatic coma pause/resume callbacks and ticks received by `ISubprocessAllowedInComa` processes during coma. Use it when a validation rule needs different behavior for manual and coma-driven transitions.

If a normal subprocess rejects its coma-induced pause, it remains in the running state but does not receive ticks during coma. If it rejects its coma-induced resume, it remains paused after the brain wakes. Handle these expected cases in `OnPauseFailed` and `OnResumeFailed`.

Override `CanEnterComa` or `CanExitComa` to prevent a brain transition. Override `OnComaEntered` or `OnComaExited` only when needed, and call the base implementation to retain automatic subprocess pause/resume behavior.

## Results and callbacks

SimpleBrain uses `OperationResult` for expected operational failures rather than exceptions or events. Check the result with its boolean conversion when the caller needs to react:

```csharp
using Systems.SimpleCore.Operations;

OperationResult result = guardBrain.TryStartSubprocess<ScanForThreatsSubprocess>();
if (!result)
{
    // Inspect result.systemCode and result.resultCode when a specific response is required.
    return;
}
```

`BrainOperations` supplies the standard success and error results, including knowledge already learned, subprocess state errors, and coma outcomes. Use protected virtual callbacks for system customization; SimpleBrain does not expose new event, `Action`, or `UnityEvent` APIs.

## Architecture

- **Abstract**: `KnowledgeBase`, `DecisionBase`, and `ISubprocessAllowedInComa` define extension points.
- **Components**: `BrainBase` owns all actor state; `BrainSubprocessBase` defines reusable long-running behaviour.
- **Data**: `BrainContext` and `BrainSubprocessContext` provide stack-only callback data.
- **Operations**: `BrainOperations` creates the package's standard `OperationResult` values.
- **Runtime integration**: SimpleCore's `TickSystem` calls enabled brains automatically.
