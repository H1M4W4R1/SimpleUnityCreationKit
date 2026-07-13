using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Operations;
using UnityEngine;

namespace Systems.SimpleSpawn.Tests
{
    public sealed class SpawnerTests : SimpleSpawnTestBase
    {
        [Test]
        public void TrySpawnSingle_UsesGeneratedPrefabTracksEntityAndParentsToSpawner()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestSpawner spawner = CreateSpawner();
            spawner.Prefab = prefab;

            OperationResult result = spawner.TrySpawnSingle();

            Assert.IsTrue(result);
            Assert.AreEqual(SpawnOperations.SUCCESS_SPAWNED, result.resultCode);
            Assert.AreEqual(1, spawner.SpawnedEntities.Count);
            Assert.AreEqual(1, spawner.GenerateCallCount);
            Assert.AreEqual(1, spawner.SpawnedCount);
            Component spawnedComponent = spawner.SpawnedEntities[0] as Component;
            Assert.AreEqual(spawner.transform, spawnedComponent.transform.parent);
            Assert.AreEqual(spawner.SpawnPosition, spawnedComponent.transform.position);
        }

        [Test]
        public void TrySpawn_WhenGenerationFails_NotifiesFailure()
        {
            TestSpawner spawner = CreateSpawner();
            spawner.AllowGeneration = false;

            OperationResult result = spawner.TrySpawnSingle(Vector3.zero, Quaternion.identity);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_ENTITY_NOT_GENERATED, result.resultCode);
            Assert.AreEqual(1, spawner.GenerateCallCount);
            Assert.AreEqual(1, spawner.SpawnFailedCount);
            Assert.AreEqual(0, spawner.SpawnedEntities.Count);
        }

        [Test]
        public void TrySpawn_WhenGeneratedPrefabIsInvalid_ReturnsInvalidPrefabAndNotifiesFailure()
        {
            GameObject spawnerObject = Track(new GameObject("InvalidPrefabSpawner"));
            InvalidPrefabSpawner spawner = spawnerObject.AddComponent<InvalidPrefabSpawner>();
            spawner.Prefab = new TestNonComponentEntity();

            OperationResult result = spawner.TrySpawnSingle(Vector3.zero, Quaternion.identity);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_INVALID_PREFAB, result.resultCode);
            Assert.AreEqual(1, spawner.SpawnFailedCount);
            Assert.AreEqual(0, spawner.SpawnedEntities.Count);
        }

        [Test]
        public void SpawnedEntities_RemovesDestroyedInstancesWhenAccessed()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestSpawner spawner = CreateSpawner();
            spawner.Prefab = prefab;
            Assert.IsTrue(spawner.TrySpawnSingle());

            Component spawnedComponent = spawner.SpawnedEntities[0] as Component;
            Object.DestroyImmediate(spawnedComponent.gameObject);

            Assert.AreEqual(0, spawner.SpawnedEntities.Count);
        }

        [Test]
        public void DespawnAll_WhenOneEntityRejects_KeepsItTrackedAndCanRetry()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestDespawnControl prefabControl = prefab.gameObject.AddComponent<TestDespawnControl>();
            prefabControl.AllowDespawn = false;
            TestSpawner spawner = CreateSpawner();
            spawner.Prefab = prefab;
            Assert.IsTrue(spawner.TrySpawnSingle());

            OperationResult result = spawner.DespawnAll();

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_SPAWN_NOT_PERMITTED, result.resultCode);
            Assert.AreEqual(1, spawner.SpawnedEntities.Count);

            Component spawnedComponent = spawner.SpawnedEntities[0] as Component;
            TestDespawnControl spawnedControl = spawnedComponent.GetComponent<TestDespawnControl>();
            spawnedControl.AllowDespawn = true;

            OperationResult retryResult = spawner.DespawnAll();

            Assert.IsTrue(retryResult);
            Assert.AreEqual(SpawnOperations.SUCCESS_ALL_DESPAWNED, retryResult.resultCode);
            Assert.AreEqual(0, spawner.SpawnedEntities.Count);
            Assert.AreEqual(1, spawner.DespawnedCount);
        }

        [Test]
        public void TrySpawnSingle_UsesOverridableSpawnPosition()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestSpawner spawner = CreateSpawner();
            spawner.Prefab = prefab;

            Assert.IsTrue(spawner.TrySpawnSingle());

            Component spawnedComponent = spawner.SpawnedEntities[0] as Component;
            Assert.AreEqual(spawner.SpawnPosition, spawnedComponent.transform.position);
            Assert.AreEqual(1, spawner.SpawnPositionCallCount);
        }

        [Test]
        public void TrySpawnSingle_WhenSpawnPositionGenerationFails_NotifiesFailure()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestSpawner spawner = CreateSpawner();
            spawner.Prefab = prefab;
            spawner.AllowSpawnPosition = false;

            OperationResult result = spawner.TrySpawnSingle();

            Assert.IsFalse(result);
            Assert.AreEqual(
                SpawnOperations.ERROR_SPAWN_POSITION_NOT_GENERATED, result.resultCode);
            Assert.AreEqual(1, spawner.SpawnPositionCallCount);
            Assert.AreEqual(1, spawner.GenerateCallCount);
            Assert.AreEqual(1, spawner.SpawnFailedCount);
        }

        [Test]
        public void TrySpawnWave_ReturnsWaveResultAndTracksAllEntities()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestWaveSpawner spawner = CreateWaveSpawner();
            spawner.Prefab = prefab;

            OperationResult result = spawner.TrySpawnWave(3);

            Assert.IsTrue(result);
            Assert.AreEqual(SpawnOperations.SUCCESS_WAVE_SPAWNED, result.resultCode);
            Assert.AreEqual(3, spawner.SpawnedEntities.Count);
            Assert.AreEqual(3, spawner.GenerateCallCount);
            Assert.AreEqual(1, spawner.WaveSpawnedCount);
        }

        [Test]
        public void TrySpawnWave_WithInvalidCountFailsBeforeGeneration()
        {
            TestWaveSpawner spawner = CreateWaveSpawner();

            OperationResult result = spawner.TrySpawnWave(0);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_INVALID_SPAWN_COUNT, result.resultCode);
            Assert.AreEqual(0, spawner.GenerateCallCount);
            Assert.AreEqual(1, spawner.WaveFailedCount);
        }

        [Test]
        public void TrySpawnWave_WhenGenerationFails_RollsBackOnlyEntitiesCreatedByThatWave()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            TestWaveSpawner spawner = CreateWaveSpawner();
            spawner.Prefab = prefab;
            Assert.IsTrue(spawner.TrySpawnSingle());
            spawner.FailAfterGeneration = 2;

            OperationResult result = spawner.TrySpawnWave(2);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_ENTITY_NOT_GENERATED, result.resultCode);
            Assert.AreEqual(1, spawner.SpawnedEntities.Count);
            Assert.AreEqual(1, spawner.WaveFailedCount);
        }
    }
}
