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
    /// <summary>Static facade for the optional, mutually exclusive game-mode system.</summary>
    public static class GameModeAPI
    {
        [CanBeNull] private static GameModeBase _currentGameMode;
        private static bool _isTickSystemHooked;
        private static bool _isTransitionInProgress;

        /// <summary>The currently active game mode, or <c>null</c> when modes are unused.</summary>
        [CanBeNull] public static GameModeBase Current => _currentGameMode;

        /// <summary>Returns <c>true</c> when a game mode is currently active.</summary>
        public static bool HasCurrent => !ReferenceEquals(_currentGameMode, null) && _currentGameMode;

        /// <summary>Returns whether the supplied game-mode type is currently active.</summary>
        public static bool IsCurrent<TGameMode>() where TGameMode : GameModeBase
            => _currentGameMode is TGameMode;

        /// <summary>Changes the active game mode to the generated asset of type <typeparamref name="TGameMode"/>.</summary>
        public static OperationResult TrySet<TGameMode>(
            GameModeChangeFlags flags = GameModeChangeFlags.None)
            where TGameMode : GameModeBase, new()
        {
            if (_isTransitionInProgress) return GameModeOperations.TransitionInProgress();

            TGameMode requestedGameMode = GameModeDatabase.GetExact<TGameMode>();
            if (ReferenceEquals(requestedGameMode, null) || !requestedGameMode)
                return GameModeOperations.GameModeNotFound();

            if (!ReferenceEquals(_currentGameMode, null) && !_currentGameMode)
                _currentGameMode = null;

            if (ReferenceEquals(_currentGameMode, requestedGameMode))
                return GameModeOperations.AlreadyActive();

            GameModeTransitionContext context = new GameModeTransitionContext(
                _currentGameMode, requestedGameMode, flags);
            _isTransitionInProgress = true;

            try
            {
                if ((flags & GameModeChangeFlags.IgnoreConditions) == 0 && _currentGameMode)
                {
                    OperationResult exitResult = _currentGameMode.CanExitGameMode(in context);
                    if (!exitResult)
                    {
                        _currentGameMode.OnGameModeExitFailed(in context, in exitResult);
                        return exitResult;
                    }
                }

                if ((flags & GameModeChangeFlags.IgnoreConditions) == 0)
                {
                    OperationResult enterResult = requestedGameMode.CanEnterGameMode(in context);
                    if (!enterResult)
                    {
                        requestedGameMode.OnGameModeEnterFailed(in context, in enterResult);
                        return enterResult;
                    }
                }

                OperationResult result = GameModeOperations.Changed();
                if (_currentGameMode) _currentGameMode.OnGameModeExited(in context, in result);

                _currentGameMode = requestedGameMode;
                EnsureTickSystemHooked();
                requestedGameMode.OnGameModeEntered(in context, in result);
                return result;
            }
            finally
            {
                _isTransitionInProgress = false;
            }
        }

        /// <summary>Clears the active game mode after its exit check succeeds.</summary>
        public static OperationResult TryClear(GameModeChangeFlags flags = GameModeChangeFlags.None)
        {
            if (_isTransitionInProgress) return GameModeOperations.TransitionInProgress();
            if (ReferenceEquals(_currentGameMode, null) || !_currentGameMode)
            {
                _currentGameMode = null;
                return GameModeOperations.NoActiveGameMode();
            }

            GameModeTransitionContext context = new GameModeTransitionContext(
                _currentGameMode, null, flags);
            _isTransitionInProgress = true;

            try
            {
                if ((flags & GameModeChangeFlags.IgnoreConditions) == 0)
                {
                    OperationResult exitResult = _currentGameMode.CanExitGameMode(in context);
                    if (!exitResult)
                    {
                        _currentGameMode.OnGameModeExitFailed(in context, in exitResult);
                        return exitResult;
                    }
                }

                OperationResult result = GameModeOperations.Cleared();
                _currentGameMode.OnGameModeExited(in context, in result);
                _currentGameMode = null;
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
            _currentGameMode = null;
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
            if (ReferenceEquals(_currentGameMode, null) || !_currentGameMode)
            {
                _currentGameMode = null;
                RemoveTickSystemHook();
                return;
            }

            _currentGameMode.OnGameModeTick(deltaTime);
        }

#if UNITY_INCLUDE_TESTS
        internal static void ClearForTests()
        {
            RemoveTickSystemHook();
            _currentGameMode = null;
            _isTransitionInProgress = false;
        }

        internal static void TickForTests(float deltaTime)
        {
            OnTick(deltaTime);
        }
#endif
    }
}
