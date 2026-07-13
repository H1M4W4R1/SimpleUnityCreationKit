using Systems.SimpleSpawn.Abstract;
using UnityEngine;

namespace Systems.SimpleSpawn.Examples.Scripts
{
    public sealed class ExampleSpawnableEntity : SpawnableEntityBase
    {
        [SerializeField] private Color _gizmoColor = Color.green;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Transform currentTransform = ReferenceEquals(_transform, null) ? transform : _transform;
            Gizmos.DrawWireSphere(currentTransform.position, 0.5f);
        }
    }
}
