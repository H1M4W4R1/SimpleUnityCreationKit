# SimpleSpawn

Prefab spawning primitives for Unity. SimpleSpawn provides a low-level `SpawnAPI`, tracked single-entity and wave spawner components, and optional despawn-control components.

## Core concepts

| Type | Role |
|---|---|
| `ISpawnableEntity` | Marker contract implemented by a prefab component that can be instantiated. |
| `SpawnableEntityBase` | Minimal `MonoBehaviour` base that implements `ISpawnableEntity`. |
| `SpawnAPI` | Low-level external `TrySpawn` and `TryDespawn` operations. |
| `SpawnerBase` | Tracks entities spawned by one component and exposes spawn/despawn operations. |
| `SingleSpawnerBase` | Base for spawners that create one entity per `TrySpawnSingle` call. |
| `WaveSpawnerBase` | Base for immediate multi-entity spawning with validation, rollback, and wave callbacks. |
| `DespawnControlBase` | Optional component for custom despawn validation and cleanup. |
| `AutomaticDespawnControlBase` | Registers a despawn check with `TickSystem` while enabled. |

## Creating a spawner

Create a prefab with a component derived from `SpawnableEntityBase`:

```csharp
public sealed class EnemyEntity : SpawnableEntityBase { }
```

Create a spawner that supplies the next prefab through the generation hook:

```csharp
public sealed class EnemySpawner : SingleSpawnerBase
{
    [SerializeField] private EnemyEntity _prefab;

    protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
    {
        entity = _prefab;
        return _prefab;
    }
}
```

Each successful spawn is added to `SpawnedEntities`. Destroyed entries are removed the next time that list is accessed. The default parent of entities created by a spawner is the spawner's transform.

`SpawnerBase` calls `CanSpawnEntity` before generation. Override it when the spawner has a condition that must be satisfied before spawning. Return `SpawnOperations.Permitted()` when the condition passes and an appropriate error result when it fails. Override `TryGetSpawnPosition(out Vector3 position, out Quaternion rotation)` when generated entities need a position or rotation other than the spawner's transform; the hook is called for each single or wave entity. Returning `false` fails the operation with `SpawnPositionNotGenerated`.

## Waves

Override `TryGenerateNextEntity` in a `WaveSpawnerBase` implementation and call `TrySpawnWave`:

```csharp
public sealed class EnemyWaveSpawner : WaveSpawnerBase
{
    [SerializeField] private EnemyEntity _prefab;

    protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
    {
        entity = _prefab;
        return _prefab;
    }
}
```

`TrySpawnWave` generates all entities immediately. A non-positive count returns `InvalidSpawnCount`. If generation or spawning fails, entities created during that call are despawned before the wave-failure callback runs. A despawn-control component that rejects rollback can leave an entity tracked and active.

## Calling the low-level API

Use `SpawnAPI` when the caller owns the prefab reference and does not need spawner tracking:

```csharp
OperationResult result = SpawnAPI.TrySpawn(
    enemyPrefab, position, Quaternion.identity, parent, out ISpawnableEntity entity);

if (result)
    SpawnAPI.TryDespawn(entity);
```

The prefab reference must point to a `Component` implementing `ISpawnableEntity`. Invalid or destroyed references return an error result without instantiating an object.

## Despawn control

Add a `DespawnControlBase` implementation to the prefab when despawn needs validation or cleanup:

```csharp
public sealed class EnemyDespawnControl : DespawnControlBase
{
    protected override OperationResult CanDespawn()
    {
        return IsReadyToLeave
            ? SpawnOperations.Permitted()
            : SpawnOperations.SpawnNotPermitted();
    }

    protected override void OnDespawn()
    {
        // Release gameplay-owned resources before Destroy runs.
    }
}
```

Derive from `AutomaticDespawnControlBase` when the same validation should be attempted on every enabled `TickSystem` tick. Its registration is removed when the component is disabled.

## Operation results

`SpawnOperations` uses system code `0x0012` and provides `Spawned`, `Despawned`, `AllDespawned`, `WaveSpawned`, configuration errors, invalid-prefab errors, invalid-count errors, and spawn-position generation errors.

## Examples included

- `Scene - Spawn.unity`: exposes runtime Unity UI for single spawn, batch spawn, latest despawn, and despawn-all cases.
- `ExampleSpawnScene`: scene driver with runtime buttons and a context menu action for replaying the batch example.
- `ExampleSingleSpawner` and `ExampleSpawnableEntity`: minimal tracked single-spawner setup.
