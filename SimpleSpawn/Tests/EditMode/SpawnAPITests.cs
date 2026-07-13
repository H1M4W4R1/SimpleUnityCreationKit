using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Operations;
using Systems.SimpleSpawn.Utility;
using UnityEngine;

namespace Systems.SimpleSpawn.Tests
{
    public sealed class SpawnAPITests : SimpleSpawnTestBase
    {
        [Test]
        public void TrySpawn_WithDirectPrefabReference_CreatesSpawnedEntity()
        {
            TestSpawnableEntity prefab = CreatePrefab();
            GameObject parentObject = Track(new GameObject("SpawnParent"));
            Vector3 position = new Vector3(1f, 2f, 3f);
            Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);

            OperationResult result = SpawnAPI.TrySpawn(
                prefab, position, rotation, parentObject.transform,
                out ISpawnableEntity spawnedEntity);

            Assert.IsTrue(result);
            Assert.AreEqual(SpawnOperations.SUCCESS_SPAWNED, result.resultCode);
            Component spawnedComponent = spawnedEntity as Component;
            Assert.IsNotNull(spawnedComponent);
            Assert.AreEqual(position, spawnedComponent.transform.position);
            Assert.Less(Quaternion.Angle(rotation, spawnedComponent.transform.rotation), 0.001f);
            Assert.AreEqual(parentObject.transform, spawnedComponent.transform.parent);
        }

        [Test]
        public void TrySpawn_WithNullOrNonComponentPrefab_ReturnsInvalidPrefab()
        {
            OperationResult nullResult = SpawnAPI.TrySpawn(
                null, Vector3.zero, Quaternion.identity, null,
                out ISpawnableEntity nullEntity);
            OperationResult nonComponentResult = SpawnAPI.TrySpawn(
                new TestNonComponentEntity(), Vector3.zero, Quaternion.identity, null,
                out ISpawnableEntity nonComponentEntity);

            Assert.IsFalse(nullResult);
            Assert.AreEqual(SpawnOperations.ERROR_INVALID_PREFAB, nullResult.resultCode);
            Assert.IsNull(nullEntity);
            Assert.IsFalse(nonComponentResult);
            Assert.AreEqual(SpawnOperations.ERROR_INVALID_PREFAB, nonComponentResult.resultCode);
            Assert.IsNull(nonComponentEntity);
        }

        [Test]
        public void TryDespawn_WithControlRejecting_DoesNotDestroyEntity()
        {
            GameObject entityObject = Track(new GameObject("SpawnedEntity"));
            TestSpawnableEntity entity = entityObject.AddComponent<TestSpawnableEntity>();
            TestDespawnControl control = entityObject.AddComponent<TestDespawnControl>();
            control.AllowDespawn = false;

            OperationResult result = SpawnAPI.TryDespawn(entity);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_SPAWN_NOT_PERMITTED, result.resultCode);
            Assert.AreEqual(1, control.DespawnFailedCount);
            Assert.IsTrue(entity);

            control.AllowDespawn = true;
            OperationResult retryResult = SpawnAPI.TryDespawn(entity);

            Assert.IsTrue(retryResult);
            Assert.AreEqual(1, control.DespawnedCount);
            Assert.IsFalse(entity);
        }

        [Test]
        public void TryDespawn_WithNullEntity_ReturnsInvalidPrefab()
        {
            OperationResult result = SpawnAPI.TryDespawn(null);

            Assert.IsFalse(result);
            Assert.AreEqual(SpawnOperations.ERROR_INVALID_PREFAB, result.resultCode);
        }

        [Test]
        public void AutomaticDespawnControl_UsesTheSameValidationLifecycle()
        {
            GameObject entityObject = Track(new GameObject("AutomaticEntity"));
            TestAutomaticDespawnControl control =
                entityObject.AddComponent<TestAutomaticDespawnControl>();

            OperationResult deniedResult = control.TryDespawn();

            Assert.IsFalse(deniedResult);
            Assert.AreEqual(SpawnOperations.ERROR_SPAWN_NOT_PERMITTED, deniedResult.resultCode);

            control.AllowDespawn = true;
            OperationResult allowedResult = control.TryDespawn();

            Assert.IsTrue(allowedResult);
            Assert.AreEqual(SpawnOperations.SUCCESS_DESPAWNED, allowedResult.resultCode);
        }
    }
}
