# SimpleLoading

SimpleLoading provides typed, staged in-game loading and distance-based world streaming. It is deliberately UI-agnostic: use `SimpleRequests` or game-specific presentation code to transfer control to a loading screen, then poll the typed handle for progress.

## Requirements

- Unity 6000.5 or later
- SimpleCore
- Unity Addressables for `AddressableSceneWorldPart`

## Staged data loading

Create a `LoadingSequenceBase` asset and implement plain serializable `LoadingStageBase` classes. Stages are not ScriptableObjects; each creates its own `ILoadingStageOperation` for a request, allowing one sequence asset to be loaded concurrently.

```csharp
LoadingSequenceHandle<GameLoadSequence> handle = LoadingAPI.Load(gameLoadSequence, player, saveFile);

float progress = LoadingAPI.GetCurrentTotalPercentage(handle);
bool isComplete = LoadingAPI.IsLoadingComplete(handle);

if (!isComplete)
    LoadingAPI.AbortLoading(handle);
```

Use `LoadingAPI.TryLoad<TSequence>` when startup validation needs an `OperationResult`. Add one `LoadingSystem` component to the active scene to call `Advance`, or call `LoadingAPI.Advance` from the game's own update loop. Sequence callbacks follow `CanStartLoading`, `OnLoadingStarted`, per-stage start and completion, then completed, failed, or cancelled callbacks. No loading-screen event or presenter contract is imposed by this package.

`AddressableDatabaseLoadingStageBase<TDatabase>` supplies the standard database stage implementation. The database base now exposes `BeginLoading`, terminal state, success state, and progress through a polling contract, so the stage starts the request and completes or fails without an Addressables callback subscription. A concrete stage only returns its database singleton:

```csharp
public sealed class LoadQuestDatabaseStage : AddressableDatabaseLoadingStageBase<QuestDatabase>
{
    protected override QuestDatabase Database => QuestDatabase.Instance;
}
```

## Dynamic world parts

Add `DynamicWorldPart` to an always-active controller object and assign a separate root containing the part's GameObjects. Call `SetTarget(playerTransform)` or `Configure(...)`. The part loads at `Load Distance` and remains loaded until its target moves beyond `Unload Distance`, avoiding boundary thrashing. The static `LoadingAPI.ShouldLoadWorldPart` helper supplies the same hysteresis calculation for caller-owned streaming logic.

For additive Addressables scene streaming, use `AddressableSceneWorldPart` and assign its scene reference. It uses the same target and distance configuration while the scene request completes asynchronously.

## Example scene

Run **Simple Loading/Regenerate Loading Example** to create `Examples/Scene - Loading.unity`. The scene shows a progress panel controlled by a small example controller which polls `LoadingAPI`, not the package API itself. Move the sphere with the configured input axes to load the blue world block within 8 metres and unload it beyond 12 metres. The generated camera uses the same skybox clear mode and placement convention as the Building Playground example.
