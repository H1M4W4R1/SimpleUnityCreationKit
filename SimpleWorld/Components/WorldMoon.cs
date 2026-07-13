using Systems.SimpleWorld.Data;
using UnityEngine;

namespace Systems.SimpleWorld.Components
{
    /// <summary>
    ///     Directional light and optional visible representation of the world moon.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class WorldMoon : MonoBehaviour
    {
        [SerializeField] private Light _moonLight;
        [SerializeField, Min(0f)] private float _baseIntensity = 0.1f;
        [SerializeField] private Color _baseColor = new Color(0.65f, 0.75f, 1f, 1f);

        public Light MoonLight
        {
            get
            {
                EnsureLight();
                return _moonLight;
            }
        }
        public float BaseIntensity => _baseIntensity;
        public Color BaseColor => _baseColor;

        private void Awake()
        {
            EnsureLight();
        }

        public void ApplyPosition(in StellarBodyPosition position)
        {
            EnsureLight();
            transform.rotation = position.direction;
            transform.position = -transform.forward * position.distance;
        }

        public void ApplyLighting(Color tint, float intensityMultiplier = 1f)
        {
            EnsureLight();
            _moonLight.color = _baseColor * tint;
            _moonLight.intensity = _baseIntensity * Mathf.Max(0f, intensityMultiplier);
        }

        public void SetVisible(bool visible)
        {
            EnsureLight();
            _moonLight.enabled = visible;
        }

        private void EnsureLight()
        {
            if (_moonLight) return;
            _moonLight = GetComponent<Light>();
            _moonLight.type = LightType.Directional;
        }
    }
}
