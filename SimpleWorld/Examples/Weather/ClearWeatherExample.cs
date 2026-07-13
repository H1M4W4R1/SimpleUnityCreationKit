using Systems.SimpleWorld.Data;
using UnityEngine;

namespace Systems.SimpleWorld.Examples.Weather
{
    public sealed class ClearWeatherExample : WeatherEffect
    {
        public override float GetWeatherDuration()
        {
            return 5f;
        }

        protected override void OnWeatherEnabled()
        {
            base.OnWeatherEnabled();
            Debug.Log("Clear weather enabled");
        }
        
        protected override void OnWeatherDisabled()
        {
            base.OnWeatherDisabled();
            Debug.Log("Clear weather disabled");
        }
    }
}