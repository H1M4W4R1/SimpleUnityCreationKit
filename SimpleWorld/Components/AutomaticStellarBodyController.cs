using System;
using Systems.SimpleCore.Timing;
using Systems.SimpleWorld.Data;
using Systems.SimpleWorld.Utility;
using UnityEngine;

namespace Systems.SimpleWorld.Components
{
    /// <summary>
    ///     Advances a simulated day and applies calculated sun and moon positions.
    /// </summary>
    public sealed class AutomaticStellarBodyController : MonoBehaviour
    {
        [SerializeField] private WorldSun _sun;
        [SerializeField] private WorldMoon _moon;
        [SerializeField, Range(-90f, 90f)] private float _latitude;
        [SerializeField, Range(-180f, 180f)] private float _longitude;
        [SerializeField, Min(0.01f)] private float _dayDurationSeconds = 120f;
        [SerializeField] private bool _useSystemTime = true;
        [SerializeField] private bool _controlRenderSettingsSun = true;
        [SerializeField] private bool _drawCurveGizmos = true;
        [SerializeField, Range(8, 96)] private int _curveGizmoSamples = 48;
        [SerializeField, Min(0.01f)] private float _curveGizmoRadius = 10f;

        private DateTime _currentDateTime;
        private bool _hasDateTime;
        private Light _previousRenderSettingsSun;
        private bool _hasRenderSettingsSunOverride;

        public WorldSun Sun => _sun;
        public WorldMoon Moon => _moon;
        public float Latitude { get => _latitude; set => _latitude = Mathf.Clamp(value, -90f, 90f); }
        public float Longitude { get => _longitude; set => _longitude = Mathf.Clamp(value, -180f, 180f); }
        public float DayDurationSeconds { get => _dayDurationSeconds; set => _dayDurationSeconds = Mathf.Max(0.01f, value); }
        public bool UseSystemTime { get => _useSystemTime; set => _useSystemTime = value; }
        public DateTime CurrentDateTime => _currentDateTime;

        /// <summary>
        ///     Assigns the stellar body components controlled by this instance.
        /// </summary>
        public void SetStellarBodies(WorldSun sun, WorldMoon moon)
        {
            _sun = sun;
            _moon = moon;

            if (!isActiveAndEnabled) return;
            UpdateWorld(0f);
        }

        private void Awake()
        {
            _currentDateTime = DateTime.UtcNow;
            _hasDateTime = true;
        }

        private void OnEnable()
        {
            TickSystem.RegisterHandler(OnTick);
            UpdateWorld(0f);
        }

        private void OnDisable()
        {
            TickSystem.UnregisterHandler(OnTick);
            RestoreRenderSettingsSun();
        }

        public void SetDateTime(DateTime dateTime)
        {
            _currentDateTime = dateTime;
            _hasDateTime = true;
            UpdateWorld(0f);
        }

        public void UpdateWorld(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds < 0f) return;
            if (!_hasDateTime) SetDateTime(DateTime.UtcNow);
            if (_useSystemTime)
                _currentDateTime = DateTime.UtcNow;
            else if (deltaTimeSeconds > 0f)
                _currentDateTime = _currentDateTime.AddSeconds(deltaTimeSeconds * 86400d / _dayDurationSeconds);

            if (!_sun || !_moon)
            {
                Debug.LogError("[AutomaticStellarBodyController] Both WorldSun and WorldMoon references are required.", this);
                return;
            }

            StellarBodyPosition sunPosition = WorldAPI.CalculateSunPosition(
                _latitude, _longitude, _currentDateTime);
            StellarBodyPosition moonPosition = WorldAPI.CalculateMoonPosition(
                _latitude, _longitude, _currentDateTime);
            _sun.ApplyPosition(sunPosition);
            _moon.ApplyPosition(moonPosition);
            ApplyRenderSettingsSun();
            Color stellarColor = WorldAPI.CalculateStellarEffectColor(sunPosition, moonPosition);
            _sun.ApplyLighting(Color.white, WorldAPI.CalculateSunLightIntensity(sunPosition.elevation));
            _moon.ApplyLighting(Color.white, WorldAPI.CalculateMoonLightIntensity(moonPosition.elevation));
            Shader.SetGlobalColor(WorldAPI.WORLD_TINT_SHADER_PROPERTY, stellarColor);
        }

        private void OnTick(float deltaTimeSeconds)
        {
            UpdateWorld(deltaTimeSeconds);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawCurveGizmos) return;

            DateTime dateFor = _hasDateTime ? _currentDateTime : DateTime.UtcNow;
            Vector3 origin = Vector3.zero;
            int sampleCount = Mathf.Max(8, _curveGizmoSamples);
            float radius = Mathf.Max(0.01f, _curveGizmoRadius);

            if (_sun)
            {
                Gizmos.color = new Color(1f, 0.78f, 0.2f, 0.95f);
                DrawCurveGizmo(origin, dateFor, sampleCount, radius, true);
                DrawBodyGizmo(WorldAPI.CalculateSunPosition(_latitude, _longitude, dateFor), origin, radius, 0.18f);
            }

            if (_moon)
            {
                Gizmos.color = new Color(0.45f, 0.62f, 1f, 0.9f);
                DrawCurveGizmo(origin, dateFor, sampleCount, radius, false);
                DrawBodyGizmo(WorldAPI.CalculateMoonPosition(_latitude, _longitude, dateFor), origin, radius, 0.14f);
            }
        }

        private void ApplyRenderSettingsSun()
        {
            if (!_controlRenderSettingsSun || !_sun) return;

            Light sunLight = _sun.SunLight;
            if (!sunLight) return;

            if (!_hasRenderSettingsSunOverride)
            {
                _previousRenderSettingsSun = RenderSettings.sun;
                _hasRenderSettingsSunOverride = true;
            }

            if (RenderSettings.sun == sunLight) return;
            RenderSettings.sun = sunLight;
        }

        private void RestoreRenderSettingsSun()
        {
            if (!_hasRenderSettingsSunOverride) return;

            Light sunLight = null;
            if (_sun) sunLight = _sun.SunLight;
            if (RenderSettings.sun == sunLight) RenderSettings.sun = _previousRenderSettingsSun;

            _previousRenderSettingsSun = null;
            _hasRenderSettingsSunOverride = false;
        }

        private void DrawCurveGizmo(Vector3 origin, DateTime dateFor, int sampleCount, float radius, bool sun)
        {
            DateTime dayStart = new DateTime(
                dateFor.Year,
                dateFor.Month,
                dateFor.Day,
                0,
                0,
                0,
                DateTimeKind.Utc);
            Vector3 previousPoint = Vector3.zero;
            bool hasPreviousPoint = false;

            for (int sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex++)
            {
                double dayFraction = sampleIndex / (double)sampleCount;
                DateTime sampleDate = dayStart.AddSeconds(dayFraction * 86400d);
                StellarBodyPosition position = sun
                    ? WorldAPI.CalculateSunPosition(_latitude, _longitude, sampleDate, radius)
                    : WorldAPI.CalculateMoonPosition(_latitude, _longitude, sampleDate, radius);
                Vector3 point = GetGizmoPoint(position, origin, radius);

                if (hasPreviousPoint) Gizmos.DrawLine(previousPoint, point);

                previousPoint = point;
                hasPreviousPoint = true;
            }
        }

        private void DrawBodyGizmo(
            in StellarBodyPosition position,
            Vector3 origin,
            float radius,
            float markerRadius)
        {
            Vector3 point = GetGizmoPoint(position, origin, radius);
            Gizmos.DrawSphere(point, markerRadius);
            Gizmos.DrawLine(origin, point);
        }

        private static Vector3 GetGizmoPoint(
            in StellarBodyPosition position,
            Vector3 origin,
            float radius)
        {
            return origin + position.direction * Vector3.back * radius;
        }
    }
}
