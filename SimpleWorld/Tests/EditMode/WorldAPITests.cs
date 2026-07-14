using System;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleWorld.Data;
using Systems.SimpleWorld.Operations;
using Systems.SimpleWorld.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleWorld.Tests
{
    public sealed class WorldAPITests
    {
        private TestWeatherEffect _weatherEffect;

        [SetUp]
        public void SetUp()
        {
            _weatherEffect = ScriptableObject.CreateInstance<TestWeatherEffect>();
        }

        [TearDown]
        public void TearDown()
        {
            WorldAPI.ClearWeatherEffect();
            if (_weatherEffect) Object.DestroyImmediate(_weatherEffect);
        }

        [Test]
        public void CalculateSunPosition_ReturnsHighSunAtEquatorialNoon()
        {
            StellarBodyPosition position = WorldAPI.CalculateSunPosition(
                0f, 0f, new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc));

            Assert.That(position.elevation, Is.GreaterThan(88f));
            Assert.AreEqual(WorldAPI.STELLAR_BODY_DISTANCE_DEFAULT, position.distance);
            Assert.That(Vector3.Dot(position.direction * Vector3.forward, Vector3.down), Is.GreaterThan(0.99f));
        }

        [Test]
        public void StellarCalculations_ClampDistanceAndProduceDifferentMoonPosition()
        {
            DateTime date = new DateTime(2024, 8, 1, 18, 0, 0, DateTimeKind.Utc);
            StellarBodyPosition sunPosition = WorldAPI.CalculateSunPosition(52f, 21f, date, -10f);
            StellarBodyPosition moonPosition = WorldAPI.CalculateMoonPosition(52f, 21f, date);

            Assert.AreEqual(0f, sunPosition.distance);
            Assert.That(Quaternion.Angle(sunPosition.direction, moonPosition.direction), Is.GreaterThan(0.01f));
        }

        [Test]
        public void CalculateSunPosition_TreatsLocalDateTimeAsSameInstant()
        {
            DateTime utcDate = new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc);
            DateTime localDate = utcDate.ToLocalTime();
            StellarBodyPosition utcPosition = WorldAPI.CalculateSunPosition(52f, 21f, utcDate);
            StellarBodyPosition localPosition = WorldAPI.CalculateSunPosition(52f, 21f, localDate);

            Assert.That(localPosition.elevation, Is.EqualTo(utcPosition.elevation).Within(0.01f));
            Assert.That(Quaternion.Angle(localPosition.direction, utcPosition.direction), Is.LessThan(0.01f));
        }

        [Test]
        public void CalculateMoonPhase_ReturnsEachNamedStageFromFullMoonReference()
        {
            DateTime fullMoon = new DateTime(2000, 1, 21, 4, 40, 0, DateTimeKind.Utc);

            Assert.AreEqual(MoonPhase.Full, WorldAPI.CalculateMoonPhase(fullMoon));
            Assert.AreEqual(MoonPhase.WaningGibbous, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS / 8d)));
            Assert.AreEqual(MoonPhase.LastQuarter, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS / 4d)));
            Assert.AreEqual(MoonPhase.WaningCrescent, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS * 3d / 8d)));
            Assert.AreEqual(MoonPhase.New, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS / 2d)));
            Assert.AreEqual(MoonPhase.WaxingCrescent, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS * 5d / 8d)));
            Assert.AreEqual(MoonPhase.FirstQuarter, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS * 3d / 4d)));
            Assert.AreEqual(MoonPhase.WaxingGibbous, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS * 7d / 8d)));
        }

        [Test]
        public void CalculateMoonPhase_RepeatsAfterOneSynodicMonthAndUsesUniversalTime()
        {
            DateTime fullMoon = new DateTime(2000, 1, 21, 4, 40, 0, DateTimeKind.Utc);
            DateTime localDate = fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS / 8d).ToLocalTime();

            Assert.AreEqual(MoonPhase.Full, WorldAPI.CalculateMoonPhase(
                fullMoon.AddDays(WorldAPI.SYNODIC_MONTH_DAYS)));
            Assert.AreEqual(MoonPhase.WaningGibbous, WorldAPI.CalculateMoonPhase(localDate));
        }

        [Test]
        public void StellarLightIntensity_FadesSmoothlyAfterVisibleHorizon()
        {
            Assert.AreEqual(0f, WorldAPI.CalculateSunLightIntensity(WorldAPI.SUN_HIDDEN_ELEVATION));
            Assert.AreEqual(1f, WorldAPI.CalculateSunLightIntensity(WorldAPI.SUN_FULL_INTENSITY_ELEVATION));
            Assert.AreEqual(0f, WorldAPI.CalculateSunLightIntensity(-0.1f));
            Assert.That(WorldAPI.CalculateSunLightIntensity(0.1f), Is.GreaterThan(0f).And.LessThan(0.001f));
            Assert.That(WorldAPI.CalculateSunLightIntensity(3f), Is.EqualTo(0.5f).Within(0.001f));

            Assert.AreEqual(0f, WorldAPI.CalculateMoonLightIntensity(WorldAPI.MOON_HIDDEN_ELEVATION));
            Assert.AreEqual(1f, WorldAPI.CalculateMoonLightIntensity(WorldAPI.MOON_FULL_INTENSITY_ELEVATION));
            Assert.That(WorldAPI.CalculateMoonLightIntensity(0f), Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void StellarEffectColor_EasesSunTintBeforeDirectSunlight()
        {
            StellarBodyPosition twilightSunPosition = new StellarBodyPosition(
                Quaternion.identity,
                WorldAPI.SUN_TINT_START_ELEVATION,
                0f);
            StellarBodyPosition horizonSunPosition = new StellarBodyPosition(
                Quaternion.identity,
                WorldAPI.SUN_HIDDEN_ELEVATION,
                0f);
            StellarBodyPosition hiddenMoonPosition = new StellarBodyPosition(
                Quaternion.identity,
                WorldAPI.MOON_HIDDEN_ELEVATION,
                0f);

            Color twilightColor = WorldAPI.CalculateStellarEffectColor(
                twilightSunPosition,
                hiddenMoonPosition);
            Color horizonColor = WorldAPI.CalculateStellarEffectColor(
                horizonSunPosition,
                hiddenMoonPosition);

            Assert.That(horizonColor.r, Is.GreaterThan(twilightColor.r));
            Assert.That(horizonColor.r - twilightColor.r, Is.LessThan(0.12f));
        }

        [Test]
        public void WeatherEffect_UsesCallbacksAndOwnDuration()
        {
            OperationResult enableResult = WorldAPI.SetWeatherEffect(_weatherEffect);

            Assert.IsTrue(OperationResult.AreSimilar(WorldOperations.WeatherEnabled(), enableResult));
            Assert.AreEqual(1, _weatherEffect.EnabledCount);
            Assert.AreEqual(30f, _weatherEffect.GetWeatherDuration());
            Assert.AreSame(_weatherEffect, WorldAPI.ActiveWeatherEffect);

            OperationResult disableResult = WorldAPI.ClearWeatherEffect();

            Assert.IsTrue(OperationResult.AreSimilar(WorldOperations.WeatherDisabled(), disableResult));
            Assert.AreEqual(1, _weatherEffect.DisabledCount);
        }

        [Test]
        public void WeatherAPI_AllowsMultipleEffectsToRemainActive()
        {
            TestWeatherEffect secondWeatherEffect = ScriptableObject.CreateInstance<TestWeatherEffect>();

            OperationResult firstResult = WorldAPI.EnableWeatherEffect(_weatherEffect);
            OperationResult secondResult = WorldAPI.EnableWeatherEffect(secondWeatherEffect);

            Assert.IsTrue(firstResult);
            Assert.IsTrue(secondResult);
            Assert.AreEqual(2, WorldAPI.ActiveWeatherEffects.Count);
            Assert.AreSame(_weatherEffect, WorldAPI.ActiveWeatherEffects[0]);
            Assert.AreSame(secondWeatherEffect, WorldAPI.ActiveWeatherEffects[1]);

            OperationResult disableResult = WorldAPI.DisableWeatherEffect(_weatherEffect);

            Assert.IsTrue(disableResult);
            Assert.AreEqual(1, WorldAPI.ActiveWeatherEffects.Count);
            Assert.AreEqual(1, secondWeatherEffect.EnabledCount);
            WorldAPI.DisableWeatherEffect(secondWeatherEffect);
            Object.DestroyImmediate(secondWeatherEffect);
        }

        [Test]
        public void ActiveWeatherEffects_RemovesDestroyedEffects()
        {
            Assert.IsTrue(WorldAPI.EnableWeatherEffect(_weatherEffect));

            Object.DestroyImmediate(_weatherEffect);
            _weatherEffect = null;

            Assert.AreEqual(0, WorldAPI.ActiveWeatherEffects.Count);
            Assert.IsNull(WorldAPI.ActiveWeatherEffect);
        }

        [Test]
        public void ClearWeatherEffects_WhenOneEffectRejectsDisable_RemovesOnlyDisabledEffects()
        {
            TestWeatherEffect secondWeatherEffect = ScriptableObject.CreateInstance<TestWeatherEffect>();
            BlockingDisableWeatherEffect blockingEffect =
                ScriptableObject.CreateInstance<BlockingDisableWeatherEffect>();

            Assert.IsTrue(WorldAPI.EnableWeatherEffect(blockingEffect));
            Assert.IsTrue(WorldAPI.EnableWeatherEffect(secondWeatherEffect));

            OperationResult clearResult = WorldAPI.ClearWeatherEffects();

            Assert.IsFalse(clearResult);
            Assert.IsTrue(OperationResult.AreSimilar(WorldOperations.WeatherDisableDenied(), clearResult));
            Assert.AreEqual(1, WorldAPI.ActiveWeatherEffects.Count);
            Assert.AreSame(blockingEffect, WorldAPI.ActiveWeatherEffect);
            Assert.AreEqual(1, secondWeatherEffect.DisabledCount);

            blockingEffect.AllowDisable = true;
            Assert.IsTrue(WorldAPI.ClearWeatherEffects());
            Object.DestroyImmediate(secondWeatherEffect);
            Object.DestroyImmediate(blockingEffect);
        }

        private sealed class TestWeatherEffect : WeatherEffect
        {
            public int EnabledCount { get; private set; }
            public int DisabledCount { get; private set; }

            public override float GetWeatherDuration() => 30f;

            protected override void OnWeatherEnabled()
            {
                EnabledCount++;
            }

            protected override void OnWeatherDisabled()
            {
                DisabledCount++;
            }
        }

        private sealed class BlockingDisableWeatherEffect : WeatherEffect
        {
            public bool AllowDisable { get; set; }

            public override float GetWeatherDuration() => 30f;

            protected override OperationResult CanDisableWeather()
            {
                return AllowDisable
                    ? base.CanDisableWeather()
                    : OperationResult.Error(WorldOperations.SYSTEM_WORLD, 100);
            }
        }
    }
}
