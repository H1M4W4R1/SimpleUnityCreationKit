# SimpleGame

`SimpleGame` provides optional, high-level game-state and game-mode tracks. Both tracks are
mutually exclusive inside themselves and independent of each other: a game can be in
`GameplayGameState` while its mode is `SinglePlayerGameMode`, for example.

## Requirements

- Unity 6000.5 or later
- `SimpleCore`
- Unity Addressables

## Game states

Game states describe the current flow of the game. `SimpleGame` includes auto-created marker
states for common flows:

- `MainMenuGameState`
- `GameplayGameState`
- `PausedGameState`
- `LoadingGameState`
- `GameOverGameState`

States are optional. Nothing is selected automatically; the game must explicitly set its first
state. On first launch, force the menu state so bootstrapping is not blocked by custom transition
checks:

```csharp
OperationResult result = GameStateAPI.TrySet<MainMenuGameState>(GameStateChangeFlags.Force);
```

`Force` skips `CanExitGameState` and `CanEnterGameState`, but still runs the normal successful
exit and enter callbacks. Use it only for trusted flows such as initialisation, save recovery, or
an explicit developer command.

For normal transitions, use the default flags:

```csharp
OperationResult menuResult = GameStateAPI.TrySet<MainMenuGameState>(GameStateChangeFlags.Force);
OperationResult gameplayResult = GameStateAPI.TrySet<GameplayGameState>();
bool isGameplay = GameStateAPI.IsCurrent<GameplayGameState>();
OperationResult clearResult = GameStateAPI.TryClear();
```

Extend `GameStateBase` to add checks and callbacks. Concrete types are auto-created under
`Assets/Generated/GameStates/` and added to the `SimpleGame.GameStates` Addressables label.

```csharp
public sealed class OnlineMatchGameState : GameStateBase
{
    protected internal override OperationResult CanEnterGameState(
        in GameStateTransitionContext context)
    {
        return IsConnected() ? GameStateOperations.Permitted() : GameStateOperations.Denied();
    }

    protected internal override void OnGameStateEntered(
        in GameStateTransitionContext context, in OperationResult result)
    {
        StartMatchmaking();
    }
}
```

## Game modes

Game modes use the same lifecycle as game states, but represent an independent game-wide choice
such as single-player, co-op, competitive, sandbox, or a custom ruleset. `SimpleGame` includes
neutral `SinglePlayerGameMode` and `MultiplayerGameMode` marker modes, while leaving activation
entirely up to the game.

Create a `GameModeBase` subclass, then select it through `GameModeAPI`:

```csharp
public sealed class SinglePlayerGameMode : GameModeBase
{
}

OperationResult result = GameModeAPI.TrySet<SinglePlayerGameMode>();
bool isSinglePlayer = GameModeAPI.IsCurrent<SinglePlayerGameMode>();
```

Concrete modes are auto-created under `Assets/Generated/GameModes/` and added to the
`SimpleGame.GameModes` Addressables label. `GameModeChangeFlags.Force` has the same trusted-flow
semantics as `GameStateChangeFlags.Force`.

## Lifecycle

For a normal transition, the current item receives `CanExit...`, then the requested item receives
`CanEnter...`. On success, the previous item receives `On...Exited` and the requested item
receives `On...Entered`. A denied exit invokes `On...ExitFailed`; a denied entry invokes
`On...EnterFailed`. The active state and mode each receive their corresponding `On...Tick` callback
through `SimpleCore`'s `TickSystem`.

Transitions requested inside lifecycle callbacks are rejected to keep the current selection
consistent. Both APIs return `OperationResult` values from `GameStateOperations` or
`GameModeOperations`, so callers can distinguish missing assets, denied transitions, duplicate
selections, and re-entrant attempts.
