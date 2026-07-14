# SimpleBrain

`SimpleBrain` supplies the AI component that belongs on an actor. `BrainBase` is the public entry point; there is no static API.

## Setup

1. Create a component that inherits `BrainBase` and attach it to the actor it controls.
2. Define serializable `KnowledgeBase` types for per-brain learned data.
3. Add `DecisionBase<TResult>` types to query that data, or `BrainSubprocessBase` types for long-running behaviour.

```csharp
using System;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Components;
using Systems.SimpleBrain.Data.Context;

[Serializable]
public sealed class GuardKnowledge : KnowledgeBase
{
    public int alertness;
}

public sealed class ShouldInvestigateDecision : DecisionBase<bool>
{
    protected override bool DecideTyped(in BrainContext context)
    {
        return context.brain.TryGetKnowledge(out GuardKnowledge knowledge) && knowledge.alertness > 0;
    }
}
```

Call `TryLearn<TKnowledge>()` to create and retain knowledge. Its data is stored as a managed reference in hidden, Unity-serialized brain storage. Use `TryGetKnowledge<TKnowledge>(out TKnowledge knowledge)` in decisions or actor code to access it.

## Subprocesses and coma

`TryStartSubprocess<TSubprocess>()` creates one subprocess instance per type for a brain. Stopping leaves the instance available for a later start.

When the brain enters coma, normal running subprocesses are paused and automatically resumed when it exits. Implement `ISubprocessAllowedInComa` on a subprocess that must continue ticking during coma. Manually paused subprocesses stay paused after coma ends.

Subprocess callbacks receive `in BrainSubprocessContext context`. Its `isComaInduced` field is true for automatic coma pause/resume calls and subprocess ticks that occur during coma, so conditions such as `CanPause` can apply the appropriate rule.

If you override `OnComaEntered` or `OnComaExited`, call the base implementation to retain this default subprocess handling.
