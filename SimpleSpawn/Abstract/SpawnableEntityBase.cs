using UnityEngine;

namespace Systems.SimpleSpawn.Abstract
{
    /// <summary>
    ///     Convenient base component for entities that are spawned from prefabs.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class SpawnableEntityBase : MonoBehaviour, ISpawnableEntity { }
}
