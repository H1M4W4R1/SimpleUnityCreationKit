using Systems.SimpleCore.Operations;
using Systems.SimpleSpawn.Operations;
using Systems.SimpleSpawn.Utility;
using UnityEngine;

namespace Systems.SimpleSpawn.Abstract
{
    /// <summary>
    ///     Controls custom validation and cleanup before an entity is destroyed.
    /// </summary>
    public abstract class DespawnControlBase : MonoBehaviour
    {
        /// <summary>
        ///     Attempts to despawn the GameObject containing this component.
        /// </summary>
        public OperationResult TryDespawn()
        {
            if (!this) return SpawnOperations.EntityAlreadyDespawned();

            OperationResult result = CanDespawn();
            if (!result)
            {
                OnDespawnFailed(result);
                return result;
            }

            OnDespawn();
            SpawnAPI.DestroyGameObject(gameObject);
            return SpawnOperations.Despawned();
        }

        /// <summary>
        ///     Checks whether this entity can be despawned.
        /// </summary>
        protected virtual OperationResult CanDespawn() => SpawnOperations.Permitted();

        /// <summary>
        ///     Called immediately before the entity is destroyed.
        /// </summary>
        protected virtual void OnDespawn() { }

        /// <summary>
        ///     Called when despawn validation fails.
        /// </summary>
        protected virtual void OnDespawnFailed(in OperationResult result) { }

    }
}
