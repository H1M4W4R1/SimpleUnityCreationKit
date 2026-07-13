using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Timing;
using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Components;
using Systems.SimpleSpawn.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleSpawn.Tests
{
    public abstract class SimpleSpawnTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();

            TickSystem[] tickSystems = Object.FindObjectsByType<TickSystem>(
                FindObjectsInactive.Include);
            for (int tickSystemIndex = 0; tickSystemIndex < tickSystems.Length; tickSystemIndex++)
            {
                TickSystem tickSystem = tickSystems[tickSystemIndex];
                if (ReferenceEquals(tickSystem, null)) continue;
                Object.DestroyImmediate(tickSystem.gameObject);
            }
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected TestSpawnableEntity CreatePrefab()
        {
            GameObject prefabObject = Track(new GameObject("SpawnPrefab"));
            return prefabObject.AddComponent<TestSpawnableEntity>();
        }

        protected TestSpawner CreateSpawner()
        {
            GameObject spawnerObject = Track(new GameObject("Spawner"));
            return spawnerObject.AddComponent<TestSpawner>();
        }

        protected TestWaveSpawner CreateWaveSpawner()
        {
            GameObject spawnerObject = Track(new GameObject("WaveSpawner"));
            return spawnerObject.AddComponent<TestWaveSpawner>();
        }
    }

    public sealed class TestSpawnableEntity : SpawnableEntityBase { }

    public sealed class TestSpawner : SingleSpawnerBase
    {
        public ISpawnableEntity Prefab;
        public bool AllowGeneration = true;
        public int GenerateCallCount;
        public int SpawnedCount;
        public int SpawnFailedCount;
        public int DespawnedCount;
        public Vector3 SpawnPosition = new Vector3(7f, 8f, 9f);
        public int SpawnPositionCallCount;
        public bool AllowSpawnPosition = true;

        protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
        {
            GenerateCallCount++;
            entity = Prefab;
            if (!AllowGeneration) return false;

            TestSpawnableEntity prefabEntity = Prefab as TestSpawnableEntity;
            return !ReferenceEquals(prefabEntity, null) && prefabEntity;
        }

        protected override bool TryGetSpawnPosition(
            out Vector3 position, out Quaternion rotation)
        {
            SpawnPositionCallCount++;
            position = SpawnPosition;
            rotation = Quaternion.identity;
            return AllowSpawnPosition;
        }

        protected override void OnSpawned(
            ISpawnableEntity entity, in OperationResult result)
        {
            SpawnedCount++;
        }

        protected override void OnSpawnFailed(in OperationResult result)
        {
            SpawnFailedCount++;
        }

        protected override void OnDespawned(
            ISpawnableEntity entity, in OperationResult result)
        {
            DespawnedCount++;
        }
    }

    public sealed class TestWaveSpawner : WaveSpawnerBase
    {
        public ISpawnableEntity Prefab;
        public int FailAfterGeneration = -1;
        public int GenerateCallCount;
        public int SpawnedCount;
        public int SpawnFailedCount;
        public int WaveSpawnedCount;
        public int WaveFailedCount;

        protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
        {
            GenerateCallCount++;
            if (FailAfterGeneration >= 0 && GenerateCallCount > FailAfterGeneration)
            {
                entity = null;
                return false;
            }

            entity = Prefab;
            TestSpawnableEntity prefabEntity = Prefab as TestSpawnableEntity;
            return !ReferenceEquals(prefabEntity, null) && prefabEntity;
        }

        protected override void OnSpawned(
            ISpawnableEntity entity, in OperationResult result)
        {
            SpawnedCount++;
        }

        protected override void OnSpawnFailed(in OperationResult result)
        {
            SpawnFailedCount++;
        }

        protected override void OnGroupSpawned(
            int groupSize, in OperationResult result)
        {
            WaveSpawnedCount++;
        }

        protected override void OnGroupSpawnFailed(
            int groupSize, in OperationResult result)
        {
            WaveFailedCount++;
        }
    }

    public sealed class InvalidPrefabSpawner : SingleSpawnerBase
    {
        public ISpawnableEntity Prefab;
        public int SpawnFailedCount;

        protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
        {
            entity = Prefab;
            return true;
        }

        protected override void OnSpawnFailed(in OperationResult result)
        {
            SpawnFailedCount++;
        }
    }

    public sealed class TestNonComponentEntity : ISpawnableEntity { }

    public sealed class TestDespawnControl : DespawnControlBase
    {
        public bool AllowDespawn = true;
        public int DespawnedCount;
        public int DespawnFailedCount;

        protected override OperationResult CanDespawn()
        {
            return AllowDespawn
                ? SpawnOperations.Permitted()
                : SpawnOperations.SpawnNotPermitted();
        }

        protected override void OnDespawn()
        {
            DespawnedCount++;
        }

        protected override void OnDespawnFailed(in OperationResult result)
        {
            DespawnFailedCount++;
        }
    }

    public sealed class TestAutomaticDespawnControl : AutomaticDespawnControlBase
    {
        public bool AllowDespawn;

        protected override OperationResult CanDespawn()
        {
            return AllowDespawn
                ? SpawnOperations.Permitted()
                : SpawnOperations.SpawnNotPermitted();
        }
    }
}
