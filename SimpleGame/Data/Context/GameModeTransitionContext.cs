using JetBrains.Annotations;
using Systems.SimpleGame.Abstract;
using Systems.SimpleGame.Data.Enums;

namespace Systems.SimpleGame.Data.Context
{
    /// <summary>Describes a requested game-mode transition.</summary>
    public readonly ref struct GameModeTransitionContext
    {
        /// <summary>The mode being left, or <c>null</c> when no mode was active.</summary>
        [CanBeNull] public readonly GameModeBase previousGameMode;

        /// <summary>The requested next mode, or <c>null</c> when the active mode is cleared.</summary>
        [CanBeNull] public readonly GameModeBase nextGameMode;

        /// <summary>Flags that modify transition behaviour.</summary>
        public readonly GameModeChangeFlags flags;

        /// <summary>Whether this transition clears the active mode without entering another.</summary>
        public bool IsClearing => ReferenceEquals(nextGameMode, null);

        public GameModeTransitionContext(
            [CanBeNull] GameModeBase previousGameMode,
            [CanBeNull] GameModeBase nextGameMode,
            GameModeChangeFlags flags)
        {
            this.previousGameMode = previousGameMode;
            this.nextGameMode = nextGameMode;
            this.flags = flags;
        }
    }
}
