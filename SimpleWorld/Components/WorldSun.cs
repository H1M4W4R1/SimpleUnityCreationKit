using Systems.SimpleWorld.Data;
using UnityEngine;

namespace Systems.SimpleWorld.Components
{
    /// <summary>
    ///     Directional light and optional visible representation of the world sun.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class WorldSun : MonoBehaviour
    {
        [SerializeField] private Light _sunLight;
        [SerializeField, Min(0f)] private float _baseIntensity = 1f;
        [SerializeField] private Color _baseColor = Color.white;

        public Light SunLight
        {
            get
            {
                EnsureLight();
                return _sunLight;
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
            _sunLight.color = _baseColor * tint;
            _sunLight.intensity = _baseIntensity * Mathf.Max(0f, intensityMultiplier);
        }

        public void SetVisible(bool visible)
        {
            EnsureLight();
            _sunLight.enabled = visible;
        }

        private void EnsureLight()
        {
            if (_sunLight) return;
            _sunLight = GetComponent<Light>();
            _sunLight.type = LightType.Directional;
        }
    }
}
