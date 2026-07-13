using Systems.SimpleTutorial.Abstract;
using Systems.SimpleTutorial.Components;
using UnityEngine;

namespace Systems.SimpleTutorial.Examples
{
    /// <summary>
    ///     Starts the example tutorial and forwards its lifecycle to the SimpleUI display.
    /// </summary>
    public sealed class ExampleTutorial : TutorialBase
    {
        [SerializeField] private TutorialStepDisplay _tutorialStepDisplay;

        private void Start()
        {
            StartTutorial();
        }

        protected override void OnTutorialStepStarted(TutorialStep tutorialStep, int stepIndex)
        {
            if (ReferenceEquals(_tutorialStepDisplay, null) || !_tutorialStepDisplay) return;

            ExampleKeyTutorialStep exampleStep = tutorialStep as ExampleKeyTutorialStep;
            if (ReferenceEquals(exampleStep, null) || !exampleStep)
            {
                _tutorialStepDisplay.HideStep();
                return;
            }

            _tutorialStepDisplay.ShowStep(exampleStep, stepIndex, StepCount);
        }

        protected override void OnTutorialCompleted()
        {
            if (ReferenceEquals(_tutorialStepDisplay, null) || !_tutorialStepDisplay) return;
            _tutorialStepDisplay.HideStep();
        }

        private void OnValidate()
        {
            if (!ReferenceEquals(_tutorialStepDisplay, null)) return;
            _tutorialStepDisplay = GetComponentInChildren<TutorialStepDisplay>(true);
        }
    }
}
