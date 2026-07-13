using System;

namespace Systems.SimpleGame.Data.Enums
{
    /// <summary>Flags that modify a game-mode transition.</summary>
    [Flags]
    public enum GameModeChangeFlags : byte
    {
        /// <summary>Runs the normal exit and entry checks.</summary>
        None = 0,

        /// <summary>Skips exit and entry checks for trusted transitions.</summary>
        IgnoreConditions = 1 << 0
    }
}
