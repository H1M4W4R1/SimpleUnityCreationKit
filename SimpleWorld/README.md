# SimpleWorld

SimpleWorld provides deterministic sun and moon positioning, tick-driven stellar body controllers, and a weather effect extension point for URP projects.

## Requirements

- Unity 6000.5+
- Universal Render Pipeline
- SimpleCore

The runtime assembly is `SimpleWorld`. Edit-mode tests are in `SimpleWorld.Tests`.

## Stellar bodies

Add `WorldSun` and `WorldMoon` to directional-light GameObjects, then assign both to `AutomaticStellarBodyController`. The controller advances a simulated day using `DayDurationSeconds`. Set `UseSystemTime` to false and call `SetDateTime` when a deterministic simulation clock is required.

```csharp
using System;
using Systems.SimpleWorld.Components;
using UnityEngine;

public sealed class WorldSetup : MonoBehaviour
{
    [SerializeField] private AutomaticStellarBodyController _controller;
    [SerializeField] private WorldSun _sun;
    [SerializeField] private WorldMoon _moon;

    private void Start()
    {
        _controller.SetStellarBodies(_sun, _moon);
        _controller.UseSystemTime = false;
        _controller.SetDateTime(new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc));
    }
}
```

`WorldAPI.CalculateSunPosition` and `WorldAPI.CalculateMoonPosition` return a `StellarBodyPosition` containing the light rotation, elevation in degrees, and distance. `CalculateStellarEffectColor` returns the day/night tint used by the global `_SimpleWorldTint` shader property. The automatic controller assigns the configured `WorldSun` light as `RenderSettings.sun` while enabled, so Unity's sky and ambient calculations do not switch between the moon and sun directional lights during sunrise or sunset. It also keeps direct sunlight at zero until the sun reaches the visible horizon, then fades it to full intensity over the next few degrees while the separate tint curve eases twilight in before direct sunlight appears.

Selecting `AutomaticStellarBodyController` in the Scene view can draw sun and moon day-curve gizmos for assigned `WorldSun` and `WorldMoon` references. The gizmo fields are inspector-only debug settings, with controls for visibility, sample count, and drawing radius.

## Weather effects

Derive a `WeatherEffect` asset. The asset owns its duration and all visual or gameplay changes. Put shader, fog, particle, audio, and other state changes in `OnWeatherEnabled` and `OnWeatherDisabled`.

```csharp
using Systems.SimpleWorld.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Simple World/Weather/Rain")]
public sealed class RainWeather : WeatherEffect
{
    [SerializeField] private ParticleSystem _rain;

    public override float GetWeatherDuration() => 45f;

    protected override void OnWeatherEnabled()
    {
        if (_rain) _rain.Play();
    }

    protected override void OnWeatherDisabled()
    {
        if (_rain) _rain.Stop();
    }
}
```

Weather assets marked with `AutoCreate` are generated under `Assets/Generated/WeatherEffects` and registered with the `SimpleWorld.WeatherEffects` Addressables label. Call `WorldAPI.SetWeatherEffect<TWeatherEffect>()` to load and enable a typed effect, `WorldAPI.EnableWeatherEffect<TWeatherEffect>()` to add one, or `WorldAPI.DisableWeatherEffect<TWeatherEffect>()` to remove one. You can also place effects in `AutomaticWorldWeatherController` to cycle through them. The controller uses each effect's `GetWeatherDuration()` result.

`SetWeatherEffect` replaces the active set. Use `EnableWeatherEffect` and `DisableWeatherEffect` when independent effects such as rain and fog should run at the same time. `ActiveWeatherEffects` exposes the current set, and `AutomaticWorldWeatherController.EnableConfiguredWeatherEffects` / `DisableConfiguredWeatherEffects` provide batch control for its configured list.

## URP shaders

`Shaders/WorldSun.shader` is an unlit transparent sun-disc shader that multiplies its material by `_SimpleWorldTint`. `Shaders/WorldFlare.shader` is an additive flare shader for a billboard or particle representation. Weather callbacks can update the same global property or configure their own materials.
