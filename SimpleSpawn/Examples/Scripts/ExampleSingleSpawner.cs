using Systems.SimpleSpawn.Abstract;
using Systems.SimpleSpawn.Components;
using UnityEngine;

namespace Systems.SimpleSpawn.Examples.Scripts
{
    public sealed class ExampleSingleSpawner : SingleSpawnerBase
    {
        [SerializeField] private ExampleSpawnableEntity _prefab;
        [SerializeField] private Vector3 _spawnOffset = new Vector3(1.5f, 0f, 0f);

        protected override bool TryGenerateNextEntity(out ISpawnableEntity entity)
        {
            entity = _prefab;
            return _prefab;
        }

        protected override bool TryGetSpawnPosition(out Vector3 position, out Quaternion rotation)
        {
            Transform spawnerTransform = SpawnerTransform;
            position = spawnerTransform.position + _spawnOffset * (SpawnedEntities.Count + 1);
            rotation = spawnerTransform.rotation;
            return true;
        }
    }
}
