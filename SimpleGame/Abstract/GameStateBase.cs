using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleGame.Data;
using Systems.SimpleGame.Data.Context;
using Systems.SimpleGame.Operations;
using UnityEngine;

[assembly: InternalsVisibleTo("SimpleGame.Tests")]
namespace Systems.SimpleGame.Abstract
{
    /// <summary>
    ///     Base asset for a high-level, mutually exclusive game state such as a menu, gameplay,
    ///     loading screen, pause screen, or game-over screen.
    /// </summary>
    /// <remarks>
    ///     Concrete subclasses are generated in <c>Assets/Generated/GameStates/</c> and registered
    ///     in <see cref="GameStateDatabase"/>. The system is optional: no state is active until
    ///     <see cref="Utility.GameStateAPI.TrySet{TGameState}"/> is called.
    /// </remarks>
    [AutoCreate("GameStates", GameStateDatabase.LABEL)]
    public abstract class GameStateBase : ScriptableObject
    {
        /// <summary>Validates entry into this state.</summary>
        [UsedImplicitly] protected internal virtual OperationResult CanEnterGameState(
            in GameStateTransitionContext context) => GameStateOperations.Permitted();

        /// <summary>Validates exit from this state.</summary>
        [UsedImplicitly] protected internal virtual OperationResult CanExitGameState(
            in GameStateTransitionContext context) => GameStateOperations.Permitted();

        /// <summary>Called after this state becomes active.</summary>
        protected internal virtual void OnGameStateEntered(
            in GameStateTransitionContext context, in OperationResult result) { }

        /// <summary>Called when an attempt to enter this state fails its entry check.</summary>
        protected internal virtual void OnGameStateEnterFailed(
            in GameStateTransitionContext context, in OperationResult result) { }

        /// <summary>Called after this state is replaced or cleared.</summary>
        protected internal virtual void OnGameStateExited(
            in GameStateTransitionContext context, in OperationResult result) { }

        /// <summary>Called when an attempt to leave this state fails its exit check.</summary>
        protected internal virtual void OnGameStateExitFailed(
            in GameStateTransitionContext context, in OperationResult result) { }

        /// <summary>Called by <see cref="Utility.GameStateAPI"/> while this state is active.</summary>
        protected internal virtual void OnGameStateTick(float deltaTime) { }
    }
}
