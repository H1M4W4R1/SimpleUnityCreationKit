using JetBrains.Annotations;
using Systems.SimpleTutorial.Abstract;
using Systems.SimpleTutorial.Data;
using UnityEngine;

namespace Systems.SimpleTutorial.Components
{
    /// <summary>
    ///     Runs an ordered set of tutorial steps and exposes overridable callbacks for presentation.
    /// </summary>
    public abstract class TutorialBase : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps = System.Array.Empty<TutorialStep>();

        private bool[] _completedSteps = System.Array.Empty<bool>();

        /// <summary>
        ///     The step currently awaiting its completion condition, or <see langword="null"/> when inactive.
        /// </summary>
        public TutorialStep ActiveStep { get; private set; }

        /// <summary>
        ///     Index of <see cref="ActiveStep"/>, or <c>-1</c> when no step is active.
        /// </summary>
        public int ActiveStepIndex { get; private set; } = -1;

        /// <summary>
        ///     Whether this tutorial currently checks its active step.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Whether every visible step has completed in the current run.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        ///     Number of configured steps.
        /// </summary>
        public int StepCount => _steps.Length;

        private void Update()
        {
            TickTutorial();
        }

        /// <summary>
        ///     Begins the tutorial from its first configured step.
        /// </summary>
        public void StartTutorial()
        {
            if (IsRunning) return;

            ResetRunState();
            IsRunning = true;
            OnTutorialStarted();
            StartNextStep();
        }

        /// <summary>
        ///     Stops the tutorial without completing its active step.
        /// </summary>
        public void StopTutorial()
        {
            if (!IsRunning) return;

            IsRunning = false;
            ActiveStep = null;
            ActiveStepIndex = -1;
            OnTutorialStopped();
        }

        /// <summary>
        ///     Restarts the tutorial and evaluates every configured step again.
        /// </summary>
        public void RestartTutorial()
        {
            StopTutorial();
            StartTutorial();
        }

        /// <summary>
        ///     Checks whether a configured step of type <typeparamref name="TStep"/> has completed in this run.
        /// </summary>
        public bool IsStepComplete<TStep>() where TStep : TutorialStep
        {
            int stepCount = _steps.Length;
            for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                TutorialStep tutorialStep = _steps[stepIndex];
                if (tutorialStep is not TStep) continue;
                return _completedSteps[stepIndex];
            }

            return false;
        }

        /// <summary>
        ///     Allows derived tutorials to provide steps from code, including in automated tests.
        /// </summary>
        protected void SetSteps([CanBeNull] TutorialStep[] steps)
        {
            _steps = ReferenceEquals(steps, null) ? System.Array.Empty<TutorialStep>() : steps;
            ResetRunState();
        }

        /// <summary>
        ///     Checks the active step. This is called automatically every frame while the tutorial runs.
        /// </summary>
        protected void TickTutorial()
        {
            if (!IsRunning || !ActiveStep) return;

            TutorialStepContext context = new TutorialStepContext(this, ActiveStepIndex);
            if (!ActiveStep.IsStepComplete(in context)) return;

            _completedSteps[ActiveStepIndex] = true;
            ActiveStep.NotifyCompleted(in context);
            OnTutorialStepCompleted(ActiveStep, ActiveStepIndex);
            ActiveStep = null;
            ActiveStepIndex = -1;
            StartNextStep();
        }

        /// <summary>
        ///     Called when a tutorial run begins.
        /// </summary>
        protected virtual void OnTutorialStarted()
        {
        }

        /// <summary>
        ///     Called after a step becomes active.
        /// </summary>
        protected virtual void OnTutorialStepStarted(TutorialStep tutorialStep, int stepIndex)
        {
        }

        /// <summary>
        ///     Called after an active step completes.
        /// </summary>
        protected virtual void OnTutorialStepCompleted(TutorialStep tutorialStep, int stepIndex)
        {
        }

        /// <summary>
        ///     Called when every visible tutorial step has completed.
        /// </summary>
        protected virtual void OnTutorialCompleted()
        {
        }

        /// <summary>
        ///     Called when a running tutorial is stopped before it completes.
        /// </summary>
        protected virtual void OnTutorialStopped()
        {
        }

        private void ResetRunState()
        {
            _completedSteps = new bool[_steps.Length];
            ActiveStep = null;
            ActiveStepIndex = -1;
            IsRunning = false;
            IsComplete = false;
        }

        private void StartNextStep()
        {
            int stepCount = _steps.Length;
            for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                if (_completedSteps[stepIndex]) continue;

                TutorialStep tutorialStep = _steps[stepIndex];
                if (ReferenceEquals(tutorialStep, null) || !tutorialStep)
                {
                    _completedSteps[stepIndex] = true;
                    continue;
                }

                TutorialStepContext context = new TutorialStepContext(this, stepIndex);
                if (!tutorialStep.ShouldShow(in context))
                {
                    _completedSteps[stepIndex] = true;
                    continue;
                }

                ActiveStep = tutorialStep;
                ActiveStepIndex = stepIndex;
                tutorialStep.NotifyStarted(in context);
                OnTutorialStepStarted(tutorialStep, stepIndex);
                return;
            }

            IsRunning = false;
            IsComplete = true;
            OnTutorialCompleted();
        }
    }
}
