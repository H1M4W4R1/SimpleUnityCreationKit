using Systems.SimpleTutorial.Data;
using UnityEngine;

namespace Systems.SimpleTutorial.Abstract
{
    /// <summary>
    ///     Defines one condition-driven step in a <see cref="Components.TutorialBase"/>.
    /// </summary>
    public abstract class TutorialStep : ScriptableObject
    {
        internal bool ShouldShow(in TutorialStepContext context)
        {
            return CanShow(in context);
        }

        internal bool IsStepComplete(in TutorialStepContext context)
        {
            return IsComplete(in context);
        }

        internal void NotifyStarted(in TutorialStepContext context)
        {
            OnTutorialStepStarted(in context);
        }

        internal void NotifyCompleted(in TutorialStepContext context)
        {
            OnTutorialStepCompleted(in context);
        }

        /// <summary>
        ///     Determines whether this step participates in this tutorial run.
        ///     A hidden step is skipped and is not re-evaluated until the tutorial is restarted.
        /// </summary>
        protected virtual bool CanShow(in TutorialStepContext context)
        {
            return true;
        }

        /// <summary>
        ///     Determines whether this step's gameplay condition has been fulfilled.
        /// </summary>
        protected abstract bool IsComplete(in TutorialStepContext context);

        /// <summary>
        ///     Checks whether a configured step of type <typeparamref name="TStep"/> has completed in this tutorial run.
        /// </summary>
        protected bool IsStepComplete<TStep>(in TutorialStepContext context) where TStep : TutorialStep
        {
            return context.Tutorial.IsStepComplete<TStep>();
        }

        /// <summary>
        ///     Called once when this step becomes the active step.
        /// </summary>
        protected virtual void OnTutorialStepStarted(in TutorialStepContext context)
        {
        }

        /// <summary>
        ///     Called once after this step's completion condition is fulfilled.
        /// </summary>
        protected virtual void OnTutorialStepCompleted(in TutorialStepContext context)
        {
        }
    }
}
