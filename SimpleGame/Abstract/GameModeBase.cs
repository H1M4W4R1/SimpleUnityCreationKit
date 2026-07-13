using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleGame.Data;
using Systems.SimpleGame.Data.Context;
using Systems.SimpleGame.Operations;
using UnityEngine;

namespace Systems.SimpleGame.Abstract
{
    /// <summary>
    ///     Base asset for a high-level game mode. Modes are independent from game states, allowing
    ///     a game to use combinations such as gameplay plus single-player or gameplay plus co-op.
    /// </summary>
    /// <remarks>
    ///     Concrete subclasses are generated in <c>Assets/Generated/GameModes/</c> and registered
    ///     in <see cref="GameModeDatabase"/>. No mode is active unless the game opts into one.
    /// </remarks>
    [AutoCreate("GameModes", GameModeDatabase.LABEL)]
    public abstract class GameModeBase : ScriptableObject
    {
        /// <summary>Validates entry into this mode.</summary>
        [UsedImplicitly] protected internal virtual OperationResult CanEnterGameMode(
            in GameModeTransitionContext context) => GameModeOperations.Permitted();

        /// <summary>Validates exit from this mode.</summary>
        [UsedImplicitly] protected internal virtual OperationResult CanExitGameMode(
            in GameModeTransitionContext context) => GameModeOperations.Permitted();

        /// <summary>Called after this mode becomes active.</summary>
        protected internal virtual void OnGameModeEntered(
            in GameModeTransitionContext context, in OperationResult result) { }

        /// <summary>Called when an attempt to enter this mode fails its entry check.</summary>
        protected internal virtual void OnGameModeEnterFailed(
            in GameModeTransitionContext context, in OperationResult result) { }

        /// <summary>Called after this mode is replaced or cleared.</summary>
        protected internal virtual void OnGameModeExited(
            in GameModeTransitionContext context, in OperationResult result) { }

        /// <summary>Called when an attempt to leave this mode fails its exit check.</summary>
        protected internal virtual void OnGameModeExitFailed(
            in GameModeTransitionContext context, in OperationResult result) { }

        /// <summary>Called by <see cref="Utility.GameModeAPI"/> while this mode is active.</summary>
        protected internal virtual void OnGameModeTick(float deltaTime) { }
    }
}
