using System;
using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleWorld.Data;
using Systems.SimpleWorld.Operations;
using UnityEngine;

namespace Systems.SimpleWorld.Utility
{
    /// <summary>
    ///     Stateless world calculations and the active weather facade.
    /// </summary>
    public static class WorldAPI
    {
        public const float STELLAR_BODY_DISTANCE_DEFAULT = 2500f;
        public const float SUN_HIDDEN_ELEVATION = 0f;
        public const float SUN_FULL_INTENSITY_ELEVATION = 6f;
        public const float SUN_TINT_START_ELEVATION = -6f;
        public const float SUN_TINT_FULL_ELEVATION = 24f;
        public const float MOON_HIDDEN_ELEVATION = -4f;
        public const float MOON_FULL_INTENSITY_ELEVATION = 4f;
        public const string WORLD_TINT_SHADER_PROPERTY = "_SimpleWorldTint";

        private static readonly List<WeatherEffect> _activeWeatherEffects = new List<WeatherEffect>();

        public static WeatherEffect ActiveWeatherEffect
        {
            get
            {
                RemoveInvalidActiveWeatherEffects();
                return _activeWeatherEffects.Count == 0 ? null : _activeWeatherEffects[0];
            }
        }

        public static IReadOnlyList<WeatherEffect> ActiveWeatherEffects
        {
            get
            {
                RemoveInvalidActiveWeatherEffects();
                return _activeWeatherEffects;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _activeWeatherEffects.Clear();
        }

        public static StellarBodyPosition CalculateSunPosition(
            float latitude,
            float longitude,
            DateTime dateFor,
            float distance = STELLAR_BODY_DISTANCE_DEFAULT)
        {
            DateTime utcDate = dateFor.ToUniversalTime();
            double dayOfYear = utcDate.DayOfYear;
            double fractionalHour = utcDate.TimeOfDay.TotalHours;
            double declinationRadians = DegreesToRadians(
                23.44d * Math.Sin(DegreesToRadians(360d * (284d + dayOfYear) / 365d)));
            double solarTime = fractionalHour + longitude / 15d;
            double hourAngleRadians = DegreesToRadians(15d * (solarTime - 12d));
            return CalculateBodyPosition(latitude, declinationRadians, hourAngleRadians, distance);
        }

        public static StellarBodyPosition CalculateMoonPosition(
            float latitude,
            float longitude,
            DateTime dateFor,
            float distance = STELLAR_BODY_DISTANCE_DEFAULT)
        {
            DateTime referenceDate = new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);
            double daysSinceReference = (dateFor.ToUniversalTime() - referenceDate).TotalDays;
            double lunarCycle = 27.321661d;
            double orbitalPhase = WrapDegrees(13.1763966d * daysSinceReference + 133.162d);
            double declinationRadians = DegreesToRadians(
                5.14d * Math.Sin(DegreesToRadians(orbitalPhase)));
            double rightAscension = orbitalPhase + 218.316d;
            double solarTime = dateFor.ToUniversalTime().TimeOfDay.TotalHours + longitude / 15d;
            double hourAngle = DegreesToRadians(
                WrapDegrees(solarTime * 15d - rightAscension + daysSinceReference * (360d / lunarCycle)));
            return CalculateBodyPosition(latitude, declinationRadians, hourAngle, distance);
        }

        public static Color CalculateStellarEffectColor(
            in StellarBodyPosition sunPosition,
            in StellarBodyPosition moonPosition)
        {
            float daylight = CalculateHorizonIntensity(
                sunPosition.elevation,
                SUN_TINT_START_ELEVATION,
                SUN_TINT_FULL_ELEVATION);
            float moonlight = CalculateHorizonIntensity(
                moonPosition.elevation,
                MOON_HIDDEN_ELEVATION,
                MOON_FULL_INTENSITY_ELEVATION) * (1f - daylight);
            Color nightColor = new Color(0.025f, 0.04f, 0.11f, 1f);
            Color moonColor = new Color(0.16f, 0.2f, 0.35f, 1f);
            Color dayColor = new Color(1f, 0.96f, 0.86f, 1f);
            Color nightWithMoon = Color.Lerp(nightColor, moonColor, moonlight);
            return Color.Lerp(nightWithMoon, dayColor, daylight);
        }

        public static float CalculateSunLightIntensity(float elevation)
            => CalculateHorizonIntensity(
                elevation,
                SUN_HIDDEN_ELEVATION,
                SUN_FULL_INTENSITY_ELEVATION);

        public static float CalculateMoonLightIntensity(float elevation)
            => CalculateHorizonIntensity(
                elevation,
                MOON_HIDDEN_ELEVATION,
                MOON_FULL_INTENSITY_ELEVATION);

        public static OperationResult SetWeatherEffect(WeatherEffect weatherEffect)
        {
            RemoveInvalidActiveWeatherEffects();
            if (!weatherEffect) return WorldOperations.WeatherIsNull();
            if (_activeWeatherEffects.Contains(weatherEffect))
                return WorldOperations.WeatherAlreadyEnabled();

            for (int effectIndex = _activeWeatherEffects.Count - 1; effectIndex >= 0; effectIndex--)
            {
                OperationResult disableResult = _activeWeatherEffects[effectIndex].Disable();
                if (!disableResult) return disableResult;
                _activeWeatherEffects.RemoveAt(effectIndex);
            }

            return EnableWeatherEffect(weatherEffect);
        }

        public static OperationResult EnableWeatherEffect(WeatherEffect weatherEffect)
        {
            RemoveInvalidActiveWeatherEffects();
            if (!weatherEffect) return WorldOperations.WeatherIsNull();
            if (_activeWeatherEffects.Contains(weatherEffect))
                return WorldOperations.WeatherAlreadyEnabled();

            OperationResult enableResult = weatherEffect.Enable();
            if (!enableResult) return enableResult;

            _activeWeatherEffects.Add(weatherEffect);
            return enableResult;
        }

        public static OperationResult SetWeatherEffect<TWeatherEffect>()
            where TWeatherEffect : WeatherEffect
        {
            TWeatherEffect weatherEffect = WeatherEffectDatabase.GetAny<TWeatherEffect>();
            return weatherEffect ? SetWeatherEffect(weatherEffect) : WorldOperations.WeatherNotFound();
        }

        public static OperationResult EnableWeatherEffect<TWeatherEffect>()
            where TWeatherEffect : WeatherEffect
        {
            TWeatherEffect weatherEffect = WeatherEffectDatabase.GetAny<TWeatherEffect>();
            return weatherEffect ? EnableWeatherEffect(weatherEffect) : WorldOperations.WeatherNotFound();
        }

        public static OperationResult DisableWeatherEffect<TWeatherEffect>()
            where TWeatherEffect : WeatherEffect
        {
            TWeatherEffect weatherEffect = WeatherEffectDatabase.GetAny<TWeatherEffect>();
            return weatherEffect ? DisableWeatherEffect(weatherEffect) : WorldOperations.WeatherNotFound();
        }

        public static OperationResult ClearWeatherEffect()
            => ClearWeatherEffects();

        public static OperationResult ClearWeatherEffects()
        {
            RemoveInvalidActiveWeatherEffects();
            if (_activeWeatherEffects.Count == 0) return WorldOperations.WeatherDisabled();

            for (int effectIndex = _activeWeatherEffects.Count - 1; effectIndex >= 0; effectIndex--)
            {
                OperationResult result = _activeWeatherEffects[effectIndex].Disable();
                if (!result) return result;
                _activeWeatherEffects.RemoveAt(effectIndex);
            }

            return WorldOperations.WeatherDisabled();
        }

        public static OperationResult DisableWeatherEffect(WeatherEffect weatherEffect)
        {
            RemoveInvalidActiveWeatherEffects();
            if (!weatherEffect) return WorldOperations.WeatherIsNull();
            int effectIndex = _activeWeatherEffects.IndexOf(weatherEffect);
            if (effectIndex < 0) return WorldOperations.WeatherNotActive();

            OperationResult result = weatherEffect.Disable();
            if (!result) return result;

            _activeWeatherEffects.RemoveAt(effectIndex);
            return WorldOperations.WeatherDisabled();
        }

        private static void RemoveInvalidActiveWeatherEffects()
        {
            for (int effectIndex = _activeWeatherEffects.Count - 1; effectIndex >= 0; effectIndex--)
            {
                WeatherEffect weatherEffect = _activeWeatherEffects[effectIndex];
                if (!weatherEffect) _activeWeatherEffects.RemoveAt(effectIndex);
            }
        }

        private static StellarBodyPosition CalculateBodyPosition(
            float latitude,
            double declinationRadians,
            double hourAngleRadians,
            float distance)
        {
            double latitudeRadians = DegreesToRadians(Mathf.Clamp(latitude, -90f, 90f));
            double elevationRadians = Math.Asin(
                Math.Sin(latitudeRadians) * Math.Sin(declinationRadians) +
                Math.Cos(latitudeRadians) * Math.Cos(declinationRadians) * Math.Cos(hourAngleRadians));
            double azimuthRadians = Math.Atan2(
                Math.Sin(hourAngleRadians),
                Math.Cos(hourAngleRadians) * Math.Sin(latitudeRadians) -
                Math.Tan(declinationRadians) * Math.Cos(latitudeRadians));
            Vector3 bodyDirection = new Vector3(
                Mathf.Sin((float)azimuthRadians) * Mathf.Cos((float)elevationRadians),
                Mathf.Sin((float)elevationRadians),
                Mathf.Cos((float)azimuthRadians) * Mathf.Cos((float)elevationRadians));
            Quaternion direction = Quaternion.LookRotation(-bodyDirection, Vector3.up);
            return new StellarBodyPosition(
                direction,
                Mathf.Rad2Deg * (float)elevationRadians,
                Mathf.Max(0f, distance));
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

        private static float CalculateHorizonIntensity(
            float elevation,
            float hiddenElevation,
            float fullIntensityElevation)
        {
            if (elevation <= hiddenElevation) return 0f;
            if (elevation >= fullIntensityElevation) return 1f;

            float transition = Mathf.InverseLerp(hiddenElevation, fullIntensityElevation, elevation);
            return transition * transition * (3f - 2f * transition);
        }

        private static double WrapDegrees(double degrees)
        {
            double wrapped = degrees % 360d;
            return wrapped < 0d ? wrapped + 360d : wrapped;
        }
    }
}
