using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Operations;
using UnityEngine;

namespace Systems.SimpleSpawn.Utility
{
    /// <summary>
    ///     Low-level external spawn and despawn operations.
    /// </summary>
    public static class SpawnAPI
    {
        /// <summary>
        ///     Instantiates a prefab represented by a direct <see cref="ISpawnableEntity"/> reference.
        /// </summary>
        public static OperationResult TrySpawn(
            [NotNull] ISpawnableEntity prefab,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] Transform parent,
            [CanBeNull] out ISpawnableEntity spawnedEntity)
        {
            spawnedEntity = null;
            if (ReferenceEquals(prefab, null)) return SpawnOperations.InvalidPrefab();

            Component prefabComponent = prefab as Component;
            if (ReferenceEquals(prefabComponent, null) || !prefabComponent)
                return SpawnOperations.InvalidPrefab();

            Component spawnedComponent = Object.Instantiate(
                prefabComponent, position, rotation, parent);
            spawnedEntity = spawnedComponent as ISpawnableEntity;
            if (ReferenceEquals(spawnedEntity, null))
            {
                DestroyGameObject(spawnedComponent.gameObject);
                return SpawnOperations.InvalidPrefab();
            }

            return SpawnOperations.Spawned();
        }

        /// <summary>
        ///     Despawns an entity, using its optional <see cref="DespawnControlBase"/> first.
        /// </summary>
        public static OperationResult TryDespawn([NotNull] ISpawnableEntity entity)
        {
            if (ReferenceEquals(entity, null)) return SpawnOperations.InvalidPrefab();

            Component component = entity as Component;
            if (ReferenceEquals(component, null) || !component)
                return SpawnOperations.EntityAlreadyDespawned();

            DespawnControlBase control = component.GetComponent<DespawnControlBase>();
            if (control) return control.TryDespawn();

            DestroyGameObject(component.gameObject);
            return SpawnOperations.Despawned();
        }

        internal static void DestroyGameObject([NotNull] GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(gameObject);
                return;
            }
#endif

            Object.Destroy(gameObject);
        }
    }
}
