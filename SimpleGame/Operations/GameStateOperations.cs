using Systems.SimpleCore.Operations;

namespace Systems.SimpleGame.Operations
{
    /// <summary>Result codes and factories for game-state operations.</summary>
    public static class GameStateOperations
    {
        /// <summary>Unique system identifier for game-state operations.</summary>
        public const ushort SYSTEM_GAME_STATE = 0x001A;

        /// <summary>The requested game-state asset was not found.</summary>
        public const ushort ERROR_GAME_STATE_NOT_FOUND = 1;

        /// <summary>The requested game state is already active.</summary>
        public const ushort ERROR_ALREADY_ACTIVE = 2;

        /// <summary>No game state is active to clear.</summary>
        public const ushort ERROR_NO_ACTIVE_GAME_STATE = 3;

        /// <summary>A callback tried to start a second transition before the first completed.</summary>
        public const ushort ERROR_TRANSITION_IN_PROGRESS = 4;

        /// <summary>A game state was entered successfully.</summary>
        public const ushort SUCCESS_CHANGED = 1;

        /// <summary>The active game state was cleared successfully.</summary>
        public const ushort SUCCESS_CLEARED = 2;

        /// <summary>Returns a generic check-passed result.</summary>
        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_GAME_STATE, OperationResult.SUCCESS_PERMITTED);

        /// <summary>Returns a generic check-denied result.</summary>
        public static OperationResult Denied()
            => OperationResult.Error(SYSTEM_GAME_STATE, OperationResult.ERROR_DENIED);

        /// <summary>Returns a requested-game-state-not-found error.</summary>
        public static OperationResult GameStateNotFound()
            => OperationResult.Error(SYSTEM_GAME_STATE, ERROR_GAME_STATE_NOT_FOUND);

        /// <summary>Returns an already-active error.</summary>
        public static OperationResult AlreadyActive()
            => OperationResult.Error(SYSTEM_GAME_STATE, ERROR_ALREADY_ACTIVE);

        /// <summary>Returns a no-active-game-state error.</summary>
        public static OperationResult NoActiveGameState()
            => OperationResult.Error(SYSTEM_GAME_STATE, ERROR_NO_ACTIVE_GAME_STATE);

        /// <summary>Returns a transition-in-progress error.</summary>
        public static OperationResult TransitionInProgress()
            => OperationResult.Error(SYSTEM_GAME_STATE, ERROR_TRANSITION_IN_PROGRESS);

        /// <summary>Returns a game-state-changed success.</summary>
        public static OperationResult Changed()
            => OperationResult.Success(SYSTEM_GAME_STATE, SUCCESS_CHANGED);

        /// <summary>Returns a game-state-cleared success.</summary>
        public static OperationResult Cleared()
            => OperationResult.Success(SYSTEM_GAME_STATE, SUCCESS_CLEARED);
    }
}
