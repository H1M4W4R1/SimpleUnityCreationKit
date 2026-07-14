using System;
using NUnit.Framework;
using Systems.SimpleWorld.Components;
using Systems.SimpleWorld.Data;
using Systems.SimpleWorld.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleWorld.Tests
{
    public sealed class WorldComponentTests
    {
        private GameObject _sunObject;
        private GameObject _moonObject;
        private GameObject _controllerObject;
        private GameObject _weatherControllerObject;
        private WorldSun _sun;
        private WorldMoon _moon;
        private Light _previousRenderSettingsSun;

        [SetUp]
        public void SetUp()
        {
            _previousRenderSettingsSun = RenderSettings.sun;
            RenderSettings.sun = null;

            _sunObject = new GameObject("Sun");
            _sunObject.AddComponent<Light>();
            _sun = _sunObject.AddComponent<WorldSun>();

            _moonObject = new GameObject("Moon");
            _moonObject.AddComponent<Light>();
            _moon = _moonObject.AddComponent<WorldMoon>();

            _controllerObject = new GameObject("Stellar Controller");
            _controllerObject.SetActive(false);
            AutomaticStellarBodyController controller =
                _controllerObject.AddComponent<AutomaticStellarBodyController>();
            controller.SetStellarBodies(_sun, _moon);
            controller.UseSystemTime = false;
            controller.DayDurationSeconds = 120f;
            _controllerObject.SetActive(true);

            _weatherControllerObject = new GameObject("Weather Controller");
            _weatherControllerObject.SetActive(false);
            _weatherControllerObject.AddComponent<AutomaticWorldWeatherController>();
            _weatherControllerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject) Object.DestroyImmediate(_controllerObject);
            if (_weatherControllerObject) Object.DestroyImmediate(_weatherControllerObject);
            if (_sunObject) Object.DestroyImmediate(_sunObject);
            if (_moonObject) Object.DestroyImmediate(_moonObject);
            Systems.SimpleWorld.Utility.WorldAPI.ClearWeatherEffects();
            RenderSettings.sun = _previousRenderSettingsSun;
        }

        [Test]
        public void AutomaticStellarBodyController_AdvancesConfiguredDate()
        {
            AutomaticStellarBodyController controller =
                _controllerObject.GetComponent<AutomaticStellarBodyController>();
            controller.SetDateTime(new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc));
            DateTime before = controller.CurrentDateTime;

            controller.UpdateWorld(1f);

            Assert.That(controller.CurrentDateTime, Is.EqualTo(before.AddMinutes(12)));
            Assert.That(_sun.SunLight.enabled, Is.True);
            Assert.That(_sun.transform.position.sqrMagnitude, Is.GreaterThan(0f));
        }

        [Test]
        public void AutomaticStellarBodyController_ExposesCurrentMoonPhase()
        {
            AutomaticStellarBodyController controller =
                _controllerObject.GetComponent<AutomaticStellarBodyController>();
            DateTime fullMoon = new DateTime(2000, 1, 21, 4, 40, 0, DateTimeKind.Utc);

            controller.SetDateTime(fullMoon);

            Assert.AreEqual(MoonPhase.Full, controller.CurrentMoonPhase);
        }

        [Test]
        public void AutomaticStellarBodyController_ControlsRenderSettingsSun()
        {
            AutomaticStellarBodyController controller =
                _controllerObject.GetComponent<AutomaticStellarBodyController>();

            controller.UpdateWorld(0f);

            Light assignedSun = RenderSettings.sun;
            Assert.IsTrue(assignedSun == _sun.SunLight);

            _controllerObject.SetActive(false);

            // Disabled for now, do not modify
            // Light restoredSun = RenderSettings.sun;
            // Assert.IsTrue(!restoredSun);
        }

        [Test]
        public void WorldLights_ApplyPositionAndLighting()
        {
            StellarBodyPosition position = new StellarBodyPosition(
                Quaternion.Euler(25f, 40f, 0f), 25f, 100f);

            _sun.ApplyPosition(position);
            _moon.ApplyPosition(position);

            Assert.That(Quaternion.Angle(position.direction, _sun.transform.rotation), Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(position.direction, _moon.transform.rotation), Is.LessThan(0.01f));
            Assert.That(_sun.transform.position.magnitude, Is.EqualTo(100f).Within(0.01f));
            Assert.That(_moon.transform.position.magnitude, Is.EqualTo(100f).Within(0.01f));
        }

        [Test]
        public void WorldLights_StayEnabledAndFadeToZeroBelowHorizon()
        {
            StellarBodyPosition visibleSunPosition = new StellarBodyPosition(
                Quaternion.identity, WorldAPI.SUN_HIDDEN_ELEVATION + 0.1f, 100f);
            StellarBodyPosition hiddenSunPosition = new StellarBodyPosition(
                Quaternion.identity, WorldAPI.SUN_HIDDEN_ELEVATION - 0.1f, 100f);
            StellarBodyPosition visibleMoonPosition = new StellarBodyPosition(
                Quaternion.identity, WorldAPI.MOON_HIDDEN_ELEVATION + 0.1f, 100f);
            StellarBodyPosition hiddenMoonPosition = new StellarBodyPosition(
                Quaternion.identity, WorldAPI.MOON_HIDDEN_ELEVATION - 0.1f, 100f);

            _sun.ApplyPosition(visibleSunPosition);
            _sun.ApplyLighting(Color.white, WorldAPI.CalculateSunLightIntensity(visibleSunPosition.elevation));
            _moon.ApplyPosition(visibleMoonPosition);
            _moon.ApplyLighting(Color.white, WorldAPI.CalculateMoonLightIntensity(visibleMoonPosition.elevation));

            Assert.IsTrue(_sun.SunLight.enabled);
            Assert.IsTrue(_moon.MoonLight.enabled);
            Assert.That(_sun.SunLight.intensity, Is.GreaterThan(0f).And.LessThan(0.001f));
            Assert.That(_moon.MoonLight.intensity, Is.GreaterThan(0f).And.LessThan(0.001f));

            _sun.ApplyPosition(hiddenSunPosition);
            _sun.ApplyLighting(Color.white, WorldAPI.CalculateSunLightIntensity(hiddenSunPosition.elevation));
            _moon.ApplyPosition(hiddenMoonPosition);
            _moon.ApplyLighting(Color.white, WorldAPI.CalculateMoonLightIntensity(hiddenMoonPosition.elevation));

            Assert.IsTrue(_sun.SunLight.enabled);
            Assert.IsTrue(_moon.MoonLight.enabled);
            Assert.AreEqual(0f, _sun.SunLight.intensity);
            Assert.AreEqual(0f, _moon.MoonLight.intensity);
        }

        [Test]
        public void AutomaticWeatherController_CanEnableConfiguredEffectsTogether()
        {
            TestWeatherEffect firstEffect = ScriptableObject.CreateInstance<TestWeatherEffect>();
            TestWeatherEffect secondEffect = ScriptableObject.CreateInstance<TestWeatherEffect>();
            AutomaticWorldWeatherController controller =
                _weatherControllerObject.GetComponent<AutomaticWorldWeatherController>();

            controller.TryAddWeatherEffect(firstEffect);
            controller.TryAddWeatherEffect(secondEffect);
            controller.EnableConfiguredWeatherEffects();

            Assert.AreEqual(2, Systems.SimpleWorld.Utility.WorldAPI.ActiveWeatherEffects.Count);

            controller.DisableConfiguredWeatherEffects();
            Object.DestroyImmediate(firstEffect);
            Object.DestroyImmediate(secondEffect);
        }

        private sealed class TestWeatherEffect : WeatherEffect
        {
            public override float GetWeatherDuration() => 1f;
        }
    }
}
