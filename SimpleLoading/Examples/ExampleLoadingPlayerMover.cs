using UnityEngine;

namespace Systems.SimpleLoading.Examples
{
    /// <summary>Moves the example player so the blue world part streams at the configured distance thresholds.</summary>
    [DisallowMultipleComponent]
    public sealed class ExampleLoadingPlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _speed = 7f;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 movement = new Vector3(horizontal, 0f, vertical);
            _transform.position += movement.normalized * (_speed * Time.deltaTime);
        }
    }
}
