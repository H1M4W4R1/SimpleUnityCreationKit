using JetBrains.Annotations;
using Systems.SimpleGame.Abstract;
using Systems.SimpleGame.Data.Enums;

namespace Systems.SimpleGame.Data.Context
{
    /// <summary>Describes a requested game-state transition.</summary>
    public readonly ref struct GameStateTransitionContext
    {
        /// <summary>The state being left, or <c>null</c> when no state was active.</summary>
        [CanBeNull] public readonly GameStateBase previousGameState;

        /// <summary>The requested next state, or <c>null</c> when the active state is cleared.</summary>
        [CanBeNull] public readonly GameStateBase nextGameState;

        /// <summary>Flags that modify transition behaviour.</summary>
        public readonly GameStateChangeFlags flags;

        /// <summary>Whether this transition clears the active state without entering another.</summary>
        public bool IsClearing => ReferenceEquals(nextGameState, null);

        public GameStateTransitionContext(
            [CanBeNull] GameStateBase previousGameState,
            [CanBeNull] GameStateBase nextGameState,
            GameStateChangeFlags flags)
        {
            this.previousGameState = previousGameState;
            this.nextGameState = nextGameState;
            this.flags = flags;
        }
    }
}
