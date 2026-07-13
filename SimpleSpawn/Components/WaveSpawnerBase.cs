using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Operations;
using UnityEngine;

namespace Systems.SimpleSpawn.Components
{
    /// <summary>
    ///     Base for spawners that generate and spawn multiple entities as one group or wave.
    /// </summary>
    public abstract class WaveSpawnerBase : SpawnerBase
    {
        /// <summary>
        ///     Checks whether a group/wave can be generated.
        /// </summary>
        protected virtual OperationResult CanSpawnWave(int groupSize)
        {
            if (groupSize <= 0) return SpawnOperations.InvalidSpawnCount();
            return CanSpawnEntity();
        }

        /// <summary>
        ///     Generates the next entity in a group/wave.
        /// </summary>
        protected virtual bool TryGenerateNextWaveEntity(out ISpawnableEntity entity)
            => TryGenerateNextEntity(out entity);

        /// <summary>
        ///     Generates and spawns a group/wave immediately.
        /// </summary>
        public virtual OperationResult TrySpawnWave(int groupSize)
        {
            return TrySpawnWaveInternal(groupSize);
        }

        /// <summary>
        ///     Executes group and wave generation.
        /// </summary>
        protected OperationResult TrySpawnWaveInternal(int groupSize)
        {
            int initialSpawnedCount = SpawnedEntities.Count;
            OperationResult canSpawnResult = CanSpawnWave(groupSize);
            if (!canSpawnResult)
            {
                RollbackGroupSpawn(initialSpawnedCount);
                OnGroupSpawnFailed(groupSize, canSpawnResult);
                return canSpawnResult;
            }

            for (int i = 0; i < groupSize; i++)
            {
                if (!TryGenerateNextWaveEntity(out ISpawnableEntity prefab))
                {
                    OperationResult result = SpawnOperations.EntityNotGenerated();
                    RollbackGroupSpawn(initialSpawnedCount);
                    OnGroupSpawnFailed(groupSize, result);
                    return result;
                }

                if (!TryGetSpawnPosition(out Vector3 position, out Quaternion rotation))
                {
                    OperationResult result = SpawnOperations.SpawnPositionNotGenerated();
                    RollbackGroupSpawn(initialSpawnedCount);
                    OnGroupSpawnFailed(groupSize, result);
                    return result;
                }

                OperationResult spawnResult = TrySpawnGeneratedEntity(
                    prefab, position, rotation);
                if (spawnResult) continue;

                RollbackGroupSpawn(initialSpawnedCount);
                OnGroupSpawnFailed(groupSize, spawnResult);
                return spawnResult;
            }

            OperationResult groupResult = GetGroupSpawnedResult();
            OnGroupSpawned(groupSize, groupResult);
            return groupResult;
        }

        protected virtual OperationResult GetGroupSpawnedResult()
            => SpawnOperations.WaveSpawned();

        protected virtual void OnGroupSpawned(int groupSize, in OperationResult result) { }

        protected virtual void OnGroupSpawnFailed(int groupSize, in OperationResult result) { }

        private void RollbackGroupSpawn(int initialSpawnedCount)
        {
            IReadOnlyList<ISpawnableEntity> spawnedEntities = SpawnedEntities;
            for (int i = spawnedEntities.Count - 1; i >= initialSpawnedCount; i--)
                TryDespawn(spawnedEntities[i]);
        }
    }
}
