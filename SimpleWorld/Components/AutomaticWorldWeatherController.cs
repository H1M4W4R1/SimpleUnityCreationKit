using System.Collections.Generic;
using Systems.SimpleCore.Timing;
using Systems.SimpleWorld.Data;
using Systems.SimpleWorld.Utility;
using UnityEngine;

namespace Systems.SimpleWorld.Components
{
    /// <summary>
    ///     Cycles through configured weather effects using each effect's duration.
    /// </summary>
    public sealed class AutomaticWorldWeatherController : MonoBehaviour
    {
        [SerializeField] private List<WeatherEffect> _weatherEffects = new List<WeatherEffect>();
        [SerializeField] private bool _randomizeOrder;
        [SerializeField] private int _randomSeed = 1;

        private System.Random _random;
        private float _elapsedSeconds;
        private float _durationSeconds;
        private int _currentIndex = -1;

        public IReadOnlyList<WeatherEffect> WeatherEffects => _weatherEffects;
        public WeatherEffect CurrentWeatherEffect => WorldAPI.ActiveWeatherEffect;
        public float ElapsedSeconds => _elapsedSeconds;
        public float DurationSeconds => _durationSeconds;

        public bool TryAddWeatherEffect(WeatherEffect weatherEffect)
        {
            if (!weatherEffect || _weatherEffects.Contains(weatherEffect)) return false;
            _weatherEffects.Add(weatherEffect);
            return true;
        }

        public bool TryRemoveWeatherEffect(WeatherEffect weatherEffect)
        {
            if (!weatherEffect) return false;
            return _weatherEffects.Remove(weatherEffect);
        }

        public bool TryAddWeatherEffect<TWeatherEffect>()
            where TWeatherEffect : WeatherEffect, new()
        {
            TWeatherEffect weatherEffect = WeatherEffectDatabase.GetExact<TWeatherEffect>();
            if (ReferenceEquals(weatherEffect, null)) return false;
            return TryAddWeatherEffect(weatherEffect);
        }

        public bool TryRemoveWeatherEffect<TWeatherEffect>()
            where TWeatherEffect : WeatherEffect, new()
        {
            TWeatherEffect weatherEffect = WeatherEffectDatabase.GetExact<TWeatherEffect>();
            if (ReferenceEquals(weatherEffect, null)) return false;
            return TryRemoveWeatherEffect(weatherEffect);
        }


        public void EnableConfiguredWeatherEffects()
        {
            for (int effectIndex = 0; effectIndex < _weatherEffects.Count; effectIndex++)
            {
                WeatherEffect weatherEffect = _weatherEffects[effectIndex];
                if (weatherEffect) WorldAPI.EnableWeatherEffect(weatherEffect);
            }
        }

        public void DisableConfiguredWeatherEffects()
        {
            for (int effectIndex = 0; effectIndex < _weatherEffects.Count; effectIndex++)
            {
                WeatherEffect weatherEffect = _weatherEffects[effectIndex];
                if (weatherEffect) WorldAPI.DisableWeatherEffect(weatherEffect);
            }
        }

        private void Awake()
        {
            _random = new System.Random(_randomSeed);
        }

        private void OnEnable()
        {
            TickSystem.RegisterHandler(OnTick);
            SelectNextWeather();
        }

        private void OnDisable()
        {
            TickSystem.UnregisterHandler(OnTick);
        }

        public void SelectNextWeather()
        {
            if (_weatherEffects.Count == 0) return;
            int nextIndex = GetNextIndex();
            WeatherEffect nextEffect = _weatherEffects[nextIndex];
            if (!nextEffect) return;

            _durationSeconds = Mathf.Max(0f, nextEffect.GetWeatherDuration());
            _elapsedSeconds = 0f;
            WorldAPI.SetWeatherEffect(nextEffect);
        }

        public void Advance(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds < 0f) return;
            _elapsedSeconds += deltaTimeSeconds;
            if (_durationSeconds <= 0f || _elapsedSeconds >= _durationSeconds) SelectNextWeather();
        }

        private int GetNextIndex()
        {
            if (_randomizeOrder) return _random.Next(0, _weatherEffects.Count);
            _currentIndex = (_currentIndex + 1) % _weatherEffects.Count;
            return _currentIndex;
        }

        private void OnTick(float deltaTimeSeconds)
        {
            Advance(deltaTimeSeconds);
        }
    }
}