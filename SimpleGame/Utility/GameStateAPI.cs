using JetBrains.Annotations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Timing;
using Systems.SimpleGame.Abstract;
using Systems.SimpleGame.Data;
using Systems.SimpleGame.Data.Context;
using Systems.SimpleGame.Data.Enums;
using Systems.SimpleGame.Operations;
using UnityEngine;

namespace Systems.SimpleGame.Utility
{
    /// <summary>Static facade for the optional, mutually exclusive game-state system.</summary>
    public static class GameStateAPI
    {
        [CanBeNull] private static GameStateBase _currentGameState;
        private static bool _isTickSystemHooked;
        private static bool _isTransitionInProgress;

        /// <summary>The currently active game state, or <c>null</c> when states are unused.</summary>
        [CanBeNull] public static GameStateBase Current => _currentGameState;

        /// <summary>Returns <c>true</c> when a game state is currently active.</summary>
        public static bool HasCurrent => !ReferenceEquals(_currentGameState, null) && _currentGameState;

        /// <summary>Returns whether the supplied game-state type is currently active.</summary>
        public static bool IsCurrent<TGameState>() where TGameState : GameStateBase
            => _currentGameState is TGameState;

        /// <summary>
        ///     Changes the active game state to the generated and addressable asset of type
        ///     <typeparamref name="TGameState"/>.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="GameStateChangeFlags.IgnoreConditions"/> for trusted bootstrapping flows, such as
        ///     forcing <c>MainMenuGameState</c> on the first launch. Force skips enter and exit checks.
        /// </remarks>
        public static OperationResult TrySet<TGameState>(
            GameStateChangeFlags flags = GameStateChangeFlags.None)
            where TGameState : GameStateBase, new()
        {
            if (_isTransitionInProgress) return GameStateOperations.TransitionInProgress();

            TGameState requestedGameState = GameStateDatabase.GetExact<TGameState>();
            if (ReferenceEquals(requestedGameState, null) || !requestedGameState)
                return GameStateOperations.GameStateNotFound();

            if (!ReferenceEquals(_currentGameState, null) && !_currentGameState)
                _currentGameState = null;

            if (ReferenceEquals(_currentGameState, requestedGameState))
                return GameStateOperations.AlreadyActive();

            GameStateTransitionContext context = new GameStateTransitionContext(
                _currentGameState, requestedGameState, flags);
            _isTransitionInProgress = true;

            try
            {
                if ((flags & GameStateChangeFlags.IgnoreConditions) == 0 && _currentGameState)
                {
                    OperationResult exitResult = _currentGameState.CanExitGameState(in context);
                    if (!exitResult)
                    {
                        _currentGameState.OnGameStateExitFailed(in context, in exitResult);
                        return exitResult;
                    }
                }

                if ((flags & GameStateChangeFlags.IgnoreConditions) == 0)
                {
                    OperationResult enterResult = requestedGameState.CanEnterGameState(in context);
                    if (!enterResult)
                    {
                        requestedGameState.OnGameStateEnterFailed(in context, in enterResult);
                        return enterResult;
                    }
                }

                OperationResult result = GameStateOperations.Changed();
                if (_currentGameState) _currentGameState.OnGameStateExited(in context, in result);

                _currentGameState = requestedGameState;
                EnsureTickSystemHooked();
                requestedGameState.OnGameStateEntered(in context, in result);
                return result;
            }
            finally
            {
                _isTransitionInProgress = false;
            }
        }

        /// <summary>Clears the active game state after its exit check succeeds.</summary>
        public static OperationResult TryClear(GameStateChangeFlags flags = GameStateChangeFlags.None)
        {
            if (_isTransitionInProgress) return GameStateOperations.TransitionInProgress();
            if (ReferenceEquals(_currentGameState, null) || !_currentGameState)
            {
                _currentGameState = null;
                return GameStateOperations.NoActiveGameState();
            }

            GameStateTransitionContext context = new GameStateTransitionContext(
                _currentGameState, null, flags);
            _isTransitionInProgress = true;

            try
            {
                if ((flags & GameStateChangeFlags.IgnoreConditions) == 0)
                {
                    OperationResult exitResult = _currentGameState.CanExitGameState(in context);
                    if (!exitResult)
                    {
                        _currentGameState.OnGameStateExitFailed(in context, in exitResult);
                        return exitResult;
                    }
                }

                OperationResult result = GameStateOperations.Cleared();
                _currentGameState.OnGameStateExited(in context, in result);
                _currentGameState = null;
                RemoveTickSystemHook();
                return result;
            }
            finally
            {
                _isTransitionInProgress = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            RemoveTickSystemHook();
            _currentGameState = null;
            _isTransitionInProgress = false;
        }

        private static void EnsureTickSystemHooked()
        {
            if (_isTickSystemHooked) return;
            TickSystem.RegisterHandler(OnTick);
            _isTickSystemHooked = true;
        }

        private static void RemoveTickSystemHook()
        {
            if (!_isTickSystemHooked) return;
            TickSystem.UnregisterHandler(OnTick);
            _isTickSystemHooked = false;
        }

        private static void OnTick(float deltaTime)
        {
            if (ReferenceEquals(_currentGameState, null) || !_currentGameState)
            {
                _currentGameState = null;
                RemoveTickSystemHook();
                return;
            }

            _currentGameState.OnGameStateTick(deltaTime);
        }

#if UNITY_INCLUDE_TESTS
        internal static void ClearForTests()
        {
            RemoveTickSystemHook();
            _currentGameState = null;
            _isTransitionInProgress = false;
        }

        internal static void TickForTests(float deltaTime)
        {
            OnTick(deltaTime);
        }
#endif
    }
}
