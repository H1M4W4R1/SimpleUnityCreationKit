using Systems.SimpleTutorial.Components;

namespace Systems.SimpleTutorial.Data
{
    /// <summary>
    ///     Read-only runtime data supplied to a tutorial step while it is evaluated.
    /// </summary>
    public readonly ref struct TutorialStepContext
    {
        /// <summary>
        ///     Tutorial instance that owns the evaluated step.
        /// </summary>
        public readonly TutorialBase Tutorial;

        /// <summary>
        ///     Position of the evaluated step in the tutorial configuration.
        /// </summary>
        public readonly int StepIndex;

        internal TutorialStepContext(TutorialBase tutorial, int stepIndex)
        {
            Tutorial = tutorial;
            StepIndex = stepIndex;
        }
    }
}
