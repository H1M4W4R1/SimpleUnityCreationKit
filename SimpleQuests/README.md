<div align="center">
  <h1>Simple Quests</h1>
</div>

# About

Simple Quests is a lightweight, data-driven quest system for Unity games. It provides a framework for creating quests with multiple objectives that can be combined, sequenced, or run in parallel. Quests are defined as ScriptableObjects and managed through an addressable database, making it easy to organize and load quest content without hard coding.

# Requirements

- **Unity**: 2022.3+
- **SimpleCore**: Required system module (dependency for operation results and tick system)
- **Unity Addressables**: For quest database loading
- **Unity Burst**: Included in assembly references
- **Unity Collections**: Included in assembly references
- **DOTween**: Included in assembly references (optional, for animation support)
- **.NET 4.x** or **.NET Standard 2.1** API Compatibility Level

# Usage Examples

## Creating a Custom Quest

Create a quest by extending the `Quest` base class and adding objectives in `Create()`:

```csharp
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Data;

public class MyQuest : Quest
{
    public override QuestInstance Create()
    {
        return base.Create()
            .WithObjective(new CollectItemObjective(5))
            .WithObjective(new DefeatEnemyObjective());
    }

    protected internal override void OnQuestStarted(QuestInstance instance)
    {
        Debug.Log($"Quest started: {name}");
    }

    protected internal override void OnQuestStartFailed(OperationResult reason)
    {
        Debug.Log($"Quest failed to start: {reason}");
    }

    protected internal override void OnQuestCompleted(QuestInstance instance)
    {
        Debug.Log($"Quest completed: {name}");
        // Reward player here
    }

    protected internal override void OnQuestFailed(QuestInstance instance)
    {
        Debug.Log($"Quest failed: {name}");
    }
}
```

## Creating Custom Objectives

Extend `QuestObjective` and implement completion logic. `ShouldBeComplete()` and `ShouldBeFailed()` are polled automatically each frame for every in-progress objective:

```csharp
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Data;

public class CollectItemObjective : QuestObjective
{
    private int _itemsCollected;
    private readonly int _itemsNeeded;

    public CollectItemObjective(int itemsNeeded)
    {
        _itemsNeeded = itemsNeeded;
    }

    // Called every frame - return true when the objective should complete
    public override bool ShouldBeComplete() => _itemsCollected >= _itemsNeeded;

    // Called every frame - return true to fail the objective (failure takes priority over completion)
    public override bool ShouldBeFailed() => false;

    // Notify the objective that an item was collected
    public void OnItemCollected() => _itemsCollected++;

    protected internal override void OnQuestObjectiveStarted(QuestInstance quest)
    {
        Debug.Log($"Collect {_itemsNeeded} items");
    }

    protected internal override void OnQuestObjectiveCompleted(QuestInstance quest)
    {
        Debug.Log("Collection objective completed!");
    }

    // Called every frame while the objective is in progress
    protected internal override void OnQuestObjectiveTick(QuestInstance questInstance, float deltaTime)
    {
        // Use for per-frame logic, e.g. polling input or updating a timer
    }
}
```

## Unique Quests

Implement `IUniqueQuest` on a quest class to prevent more than one active instance at a time. `TryStartQuest` will return `QuestAlreadyStarted` if an instance of that type is already running:

```csharp
using Systems.SimpleQuests.Abstract;
using Systems.SimpleQuests.Abstract.Markers;

public class MyUniqueQuest : Quest, IUniqueQuest
{
    // Only one active instance allowed at a time — enforced automatically by TryStartQuest
}
```

## Starting and Managing Quests

Use `QuestAPI` to start, complete, and track quests:

```csharp
using Systems.SimpleQuests.Utility;

// Start a quest (multiple instances allowed unless IUniqueQuest is implemented)
OperationResult result = QuestAPI.TryStartQuest<MyQuest>(out QuestInstance questInstance);
if (result)
{
    Debug.Log("Quest started successfully");
}

// Force complete or fail an active quest
QuestAPI.CompleteQuest<MyQuest>();
QuestAPI.FailQuest<MyQuest>();

// Programmatically complete or fail a specific objective on an instance
questInstance.TryCompleteObjective<CollectItemObjective>();
questInstance.TryFailObjective<DefeatEnemyObjective>();

// Check if at least one instance of a quest type has been completed or failed
bool completed = QuestAPI.IsQuestCompleted<MyQuest>();
bool failed    = QuestAPI.IsQuestFailed<MyQuest>();

// Check state on a specific instance
bool instanceCompleted = questInstance.IsCompleted;
bool instanceFailed    = questInstance.IsFailed;

// Get the first active quest of a type
MyQuest activeQuest = QuestAPI.GetFirstActiveQuestOfType<MyQuest>();

// Get all finished quests (completed or failed) of a type
ROListAccess<MyQuest> finishedQuests = QuestAPI.GetAllFinishedQuestsOfType<MyQuest>();

// All finished quests regardless of type
IReadOnlyList<QuestInstance> allFinished = QuestAPI.FinishedQuests;

// Clear all quests
QuestAPI.ClearAllQuests();
```

## Accessing Objectives on an Instance

```csharp
// Get the first objective of a type
CollectItemObjective collectObjective = questInstance.GetObjective<CollectItemObjective>();
collectObjective?.OnItemCollected();

// Get all objectives of a type
ROListAccess<CollectItemObjective> allCollectObjectives = questInstance.GetObjectives<CollectItemObjective>();
```

## Combining Objectives

Use `CombinedQuestObjective` to activate multiple objectives simultaneously. All child objectives start at the same time and the combined objective completes when all required children complete:

```csharp
using Systems.SimpleQuests.Objectives;

CombinedQuestObjective combined = new CombinedQuestObjective()
    .WithObjective(new KillEnemyObjective())
    .WithObjective(new CollectLootObjective());

questInstance.WithObjective(combined);
```

## Checking Quest States

Monitor quest progress using `QuestState` or the convenience properties on `QuestInstance`:

```csharp
using Systems.SimpleQuests.Data.Enums;

// Quest states: Hidden, Inactive, InProgress, Completed, Failed
if (questInstance.State == QuestState.Completed)
{
    Debug.Log("Quest is complete!");
}

// Convenience bool properties on QuestInstance
if (questInstance.IsCompleted) { /* ... */ }
if (questInstance.IsFailed)    { /* ... */ }

// Check individual objectives
for (int objectiveIndex = 0; objectiveIndex < questInstance.Objectives.Count; objectiveIndex++)
{
    QuestObjective objective = questInstance.Objectives[objectiveIndex];
    if (objective.State == QuestState.InProgress)
    {
        Debug.Log($"Objective in progress: {objective}");
    }
}
```

## Optional vs Required Objectives

Create optional objectives by overriding `IsRequired`:

```csharp
public class BonusObjective : QuestObjective
{
    public override bool IsRequired => false; // Optional objective

    // ... implementation
}
```

Optional objectives fail independently without failing the quest. Required objectives activate sequentially - only one required objective is `InProgress` at a time. Optional objectives that appear before the next required objective in the list activate alongside it.

# Architecture Overview

- **Quest**: Base class for quest definitions (ScriptableObject)
- **QuestInstance**: Runtime instance of a quest with state tracking
- **QuestObjective**: Base class for individual quest tasks
- **QuestDatabase**: Addressable database for quest management
- **QuestAPI**: Static API for quest management and querying
- **QuestState**: Enum tracking quest and objective progression (`Hidden`, `Inactive`, `InProgress`, `Completed`, `Failed`)
- **IUniqueQuest**: Marker interface; implement on a `Quest` to enforce a single active instance at a time
- **CombinedQuestObjective**: Composite pattern for grouped objectives that all activate at once

The system integrates with the SimpleCore tick system for automatic per-frame objective evaluation. Each frame, every in-progress objective is ticked, then `ShouldBeFailed()` and `ShouldBeComplete()` are checked (failure takes priority). When all required objectives finish, the quest moves to the finished list automatically.

## Examples included

Open `Examples/Examples - Quest Scene.unity` and enter Play Mode to use the side-by-side runtime panels for the normal quest and unique quest examples.

- `Examples - Quest Scene.unity`: exposes runtime Unity UI for normal quest start/complete/fail/clear and unique quest duplicate/override/state cases.
- `ExampleQuestStarter`: runtime UI and context menu driver for normal quest lifecycle operations.
- `ExampleUniqueQuestStarter`: runtime UI and context menu driver for `IUniqueQuest` duplicate and override cases.
- `ExampleQuest`, `ExampleUniqueQuest`, and objective examples: configured quest definitions used by the scene.
