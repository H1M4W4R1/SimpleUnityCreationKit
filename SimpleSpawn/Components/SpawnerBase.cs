using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Operations;
using Systems.SimpleSpawn.Utility;
using UnityEngine;

namespace Systems.SimpleSpawn.Components
{
    /// <summary>
    ///     Base component for spawners that track every entity they create.
    /// </summary>
    public abstract class SpawnerBase : MonoBehaviour
    {
        protected Transform _transform;
        private readonly List<ISpawnableEntity> _spawnedEntities = new();

        protected virtual void Awake()
        {
            _transform = transform;
        }

        protected Transform SpawnerTransform
        {
            get
            {
                if (ReferenceEquals(_transform, null) || !_transform)
                    _transform = transform;

                return _transform;
            }
        }

        /// <summary>
        ///     Current entities spawned by this component. Destroyed instances are removed on access.
        /// </summary>
        [NotNull] public IReadOnlyList<ISpawnableEntity> SpawnedEntities
        {
            get
            {
                RemoveDestroyedEntities();
                return _spawnedEntities;
            }
        }

        /// <summary>
        ///     Spawns one entity at the generated spawn position and rotation.
        /// </summary>
        public OperationResult TrySpawnSingle()
        {
            RemoveDestroyedEntities();

            OperationResult canSpawnResult = CanSpawnEntity();
            if (!canSpawnResult) return CompleteSpawnFailure(canSpawnResult);

            if (!TryGenerateNextEntity(out ISpawnableEntity prefab))
                return CompleteSpawnFailure(SpawnOperations.EntityNotGenerated());

            if (!TryGetSpawnPosition(out Vector3 position, out Quaternion rotation))
                return CompleteSpawnFailure(SpawnOperations.SpawnPositionNotGenerated());

            return TrySpawnGeneratedEntity(prefab, position, rotation);
        }

        /// <summary>
        ///     Spawns one entity at the supplied transform values.
        /// </summary>
        public OperationResult TrySpawnSingle(Vector3 position, Quaternion rotation)
        {
            RemoveDestroyedEntities();

            OperationResult canSpawnResult = CanSpawnEntity();
            if (!canSpawnResult) return CompleteSpawnFailure(canSpawnResult);

            if (!TryGenerateNextEntity(out ISpawnableEntity prefab))
                return CompleteSpawnFailure(SpawnOperations.EntityNotGenerated());

            return TrySpawnGeneratedEntity(prefab, position, rotation);
        }

        /// <summary>
        ///     Generates the position and rotation used by the parameterless single and wave spawn operations.
        /// </summary>
        protected virtual bool TryGetSpawnPosition(
            out Vector3 position, out Quaternion rotation)
        {
            Transform spawnerTransform = SpawnerTransform;
            position = spawnerTransform.position;
            rotation = spawnerTransform.rotation;
            return true;
        }

        /// <summary>
        ///     Despawns one tracked or externally supplied entity.
        /// </summary>
        public OperationResult TryDespawn([NotNull] ISpawnableEntity entity)
        {
            OperationResult result = SpawnAPI.TryDespawn(entity);
            if (!result) return result;

            RemoveEntity(entity);
            OnDespawned(entity, result);
            return result;
        }

        /// <summary>
        ///     Despawns all currently tracked entities.
        /// </summary>
        public OperationResult DespawnAll()
        {
            RemoveDestroyedEntities();
            OperationResult lastResult = SpawnOperations.AllDespawned();

            for (int i = _spawnedEntities.Count - 1; i >= 0; i--)
            {
                ISpawnableEntity entity = _spawnedEntities[i];
                OperationResult result = SpawnAPI.TryDespawn(entity);
                if (result)
                {
                    OnDespawned(entity, result);
                    _spawnedEntities.RemoveAt(i);
                }
                else
                {
                    lastResult = result;
                }
            }

            return lastResult;
        }

        /// <summary>
        ///     Checks whether this spawner can produce one entity.
        /// </summary>
        protected virtual OperationResult CanSpawnEntity()
        {
            return SpawnOperations.Permitted();
        }

        /// <summary>
        ///     Generates one prefab for the next single spawn.
        /// </summary>
        protected abstract bool TryGenerateNextEntity(out ISpawnableEntity entity);

        /// <summary>
        ///     Instantiates a generated prefab and adds the instance to this spawner's list.
        /// </summary>
        protected OperationResult TrySpawnGeneratedEntity(
            [NotNull] ISpawnableEntity prefab,
            Vector3 position,
            Quaternion rotation)
        {
            OperationResult result = SpawnAPI.TrySpawn(
                prefab, position, rotation, SpawnerTransform, out ISpawnableEntity spawnedEntity);
            if (!result)
            {
                OnSpawnFailed(result);
                return result;
            }

            _spawnedEntities.Add(spawnedEntity);
            OnSpawned(spawnedEntity!, result);
            return result;
        }

        protected virtual void OnSpawned(
            [NotNull] ISpawnableEntity entity, in OperationResult result) { }

        protected virtual void OnSpawnFailed(in OperationResult result) { }

        protected virtual void OnDespawned(
            [NotNull] ISpawnableEntity entity, in OperationResult result) { }

        protected virtual void OnDestroy()
        {
            _spawnedEntities.Clear();
        }

        private OperationResult CompleteSpawnFailure(OperationResult result)
        {
            OnSpawnFailed(result);
            return result;
        }

        private void RemoveDestroyedEntities()
        {
            for (int i = _spawnedEntities.Count - 1; i >= 0; i--)
            {
                ISpawnableEntity entity = _spawnedEntities[i];
                if (ReferenceEquals(entity, null))
                {
                    _spawnedEntities.RemoveAt(i);
                    continue;
                }

                Component component = entity as Component;
                if (ReferenceEquals(component, null) || !component)
                    _spawnedEntities.RemoveAt(i);
            }
        }

        private void RemoveEntity(ISpawnableEntity entity)
        {
            for (int i = _spawnedEntities.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_spawnedEntities[i], entity))
                    _spawnedEntities.RemoveAt(i);
            }
        }
    }
}
