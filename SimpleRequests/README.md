# SimpleRequests

`SimpleRequests` is a small, allocation-free request dispatch system for Unity. It lets systems exchange value-type request data without direct assembly references, events, or `UnityEvent` setup.

## Requirements

- Unity 6000.5 or later
- No package dependencies

## Concepts

- **Request context**: A `struct` implementing `IRequestContext`. It contains the data sent to handlers.
- **Request handler**: A method registered for a specific request context type.
- **Response**: An optional `struct` that request handlers update through a `ref` parameter.

Each closed generic request type has its own static handler list. Sending a request invokes higher-priority handlers first. For handlers with the same priority, the most recently registered handler runs first. Registering the same handler more than once does not add duplicates.

## Send a request

Define a small context type and register a handler for it. Register and unregister handlers with the same lifecycle so static handler lists do not retain disabled objects.

```csharp
using Systems.SimpleRequests.Abstract;
using Systems.SimpleRequests.Utility;
using UnityEngine;

public readonly struct PlaySoundRequest : IRequestContext
{
    public readonly int SoundId;

    public PlaySoundRequest(int soundId)
    {
        SoundId = soundId;
    }
}

public sealed class SoundPlayer : MonoBehaviour
{
    private void OnEnable()
    {
        RequestAPI.RegisterHandler<PlaySoundRequest>(HandlePlaySound);
    }

    private void OnDisable()
    {
        RequestAPI.UnregisterHandler<PlaySoundRequest>(HandlePlaySound);
    }

    private static void HandlePlaySound(in PlaySoundRequest request)
    {
        Debug.Log("Play sound " + request.SoundId);
    }
}

public sealed class SoundButton : MonoBehaviour
{
    public void PlayClick()
    {
        PlaySoundRequest request = new PlaySoundRequest(1);
        RequestAPI.Send(in request);
    }
}
```

## Send a request with a response

Response handlers share one response value. Each handler can inspect and update the value left by handlers that ran before it. Because higher-priority handlers run first, give override handlers a higher priority than fallback handlers.

```csharp
using Systems.SimpleRequests.Abstract;
using Systems.SimpleRequests.Utility;

public readonly struct PriceRequest : IRequestContext
{
    public readonly int ItemId;

    public PriceRequest(int itemId)
    {
        ItemId = itemId;
    }
}

public struct PriceResponse
{
    public int Value;
}

public static class PriceRequests
{
    public static void Register()
    {
        RequestAPI.RegisterHandler<PriceRequest, PriceResponse>(SetDefaultPrice);
        RequestAPI.RegisterHandler<PriceRequest, PriceResponse>(ApplySalePrice, priority: 100);
    }

    public static PriceResponse GetPrice(int itemId)
    {
        PriceRequest request = new PriceRequest(itemId);
        return RequestAPI.Send<PriceRequest, PriceResponse>(in request);
    }

    private static void SetDefaultPrice(in PriceRequest request, ref PriceResponse response)
    {
        response.Value = 100;
    }

    private static void ApplySalePrice(in PriceRequest request, ref PriceResponse response)
    {
        if (request.ItemId == 1)
            response.Value = 75;
    }
}
```

`RequestAPI.Send<TRequestContext, TResponseType>()` starts with a default response. When a caller needs to provide an initial value, use the lower-level request store directly:

```csharp
PriceRequest request = new PriceRequest(1);
PriceResponse response = new PriceResponse { Value = 120 };
Request<PriceRequest, PriceResponse>.Send(in request, ref response);
```

## Clearing handlers

Use `ClearHandlers` during deterministic teardown, especially in tests. Clearing only affects the exact generic request type or request-and-response type pair supplied.

```csharp
RequestAPI.ClearHandlers<PlaySoundRequest>();
RequestAPI.ClearHandlers<PriceRequest, PriceResponse>();
```

## Notes

- Request registration, unregistration, clearing, and sending are intended for the Unity main thread.
- Set `priority` when registering a handler to control dispatch order. Higher values run first; equal values preserve newest-registration-first order.
- Handlers should remain short and avoid allocating in hot paths.
- A handler may be unregistered safely even when it is not currently registered. Null handlers are ignored.
