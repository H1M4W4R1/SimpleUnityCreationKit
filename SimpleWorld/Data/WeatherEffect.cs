using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleWorld.Operations;
using UnityEngine;

namespace Systems.SimpleWorld.Data
{
    /// <summary>
    ///     Extension point for a weather state. Visuals, audio, fog, and shader changes belong in
    ///     the lifecycle callbacks implemented by a concrete effect.
    /// </summary>
    [AutoCreate("WeatherEffects", WeatherEffectDatabase.LABEL)]
    public abstract class WeatherEffect : ScriptableObject
    {
        /// <summary>Returns the active duration in seconds before the next weather transition.</summary>
        public abstract float GetWeatherDuration();

        public OperationResult Enable()
        {
            OperationResult result = CanEnableWeather();
            if (!result) return WorldOperations.WeatherEnableDenied();

            OnWeatherEnabled();
            return WorldOperations.WeatherEnabled();
        }

        public OperationResult Disable()
        {
            OperationResult result = CanDisableWeather();
            if (!result) return WorldOperations.WeatherDisableDenied();

            OnWeatherDisabled();
            return WorldOperations.WeatherDisabled();
        }

        protected virtual OperationResult CanEnableWeather()
            => OperationResult.Success(WorldOperations.SYSTEM_WORLD, OperationResult.SUCCESS_PERMITTED);

        protected virtual OperationResult CanDisableWeather()
            => OperationResult.Success(WorldOperations.SYSTEM_WORLD, OperationResult.SUCCESS_PERMITTED);

        protected virtual void OnWeatherEnabled()
        {
        }

        protected virtual void OnWeatherDisabled()
        {
        }
    }
}
