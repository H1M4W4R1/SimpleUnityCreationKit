using Systems.SimpleCore.Operations;

namespace Systems.SimpleGame.Operations
{
    /// <summary>Result codes and factories for game-mode operations.</summary>
    public static class GameModeOperations
    {
        /// <summary>Unique system identifier for game-mode operations.</summary>
        public const ushort SYSTEM_GAME_MODE = 0x001B;

        /// <summary>The requested game-mode asset was not found.</summary>
        public const ushort ERROR_GAME_MODE_NOT_FOUND = 1;

        /// <summary>The requested game mode is already active.</summary>
        public const ushort ERROR_ALREADY_ACTIVE = 2;

        /// <summary>No game mode is active to clear.</summary>
        public const ushort ERROR_NO_ACTIVE_GAME_MODE = 3;

        /// <summary>A callback tried to start a second transition before the first completed.</summary>
        public const ushort ERROR_TRANSITION_IN_PROGRESS = 4;

        /// <summary>A game mode was entered successfully.</summary>
        public const ushort SUCCESS_CHANGED = 1;

        /// <summary>The active game mode was cleared successfully.</summary>
        public const ushort SUCCESS_CLEARED = 2;

        /// <summary>Returns a generic check-passed result.</summary>
        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_GAME_MODE, OperationResult.SUCCESS_PERMITTED);

        /// <summary>Returns a generic check-denied result.</summary>
        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_GAME_MODE, OperationResult.ERROR_DENIED);

        /// <summary>Returns a requested-game-mode-not-found error.</summary>
        public static OperationResult GameModeNotFound()
            => OperationResult.Error(SYSTEM_GAME_MODE, ERROR_GAME_MODE_NOT_FOUND);

        /// <summary>Returns an already-active error.</summary>
        public static OperationResult AlreadyActive()
            => OperationResult.Error(SYSTEM_GAME_MODE, ERROR_ALREADY_ACTIVE);

        /// <summary>Returns a no-active-game-mode error.</summary>
        public static OperationResult NoActiveGameMode()
            => OperationResult.Error(SYSTEM_GAME_MODE, ERROR_NO_ACTIVE_GAME_MODE);

        /// <summary>Returns a transition-in-progress error.</summary>
        public static OperationResult TransitionInProgress()
            => OperationResult.Error(SYSTEM_GAME_MODE, ERROR_TRANSITION_IN_PROGRESS);

        /// <summary>Returns a game-mode-changed success.</summary>
        public static OperationResult Changed()
            => OperationResult.Success(SYSTEM_GAME_MODE, SUCCESS_CHANGED);

        /// <summary>Returns a game-mode-cleared success.</summary>
        public static OperationResult Cleared()
            => OperationResult.Success(SYSTEM_GAME_MODE, SUCCESS_CLEARED);
    }
}
