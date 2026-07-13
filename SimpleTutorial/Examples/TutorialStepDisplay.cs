using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleTutorial.Examples
{
    /// <summary>
    ///     SimpleUI-backed display for an active tutorial step.
    /// </summary>
    public sealed class TutorialStepDisplay : UIObjectBase
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;

        /// <summary>
        ///     Updates the panel content and plays its configured SimpleUI show animation.
        /// </summary>
        public void ShowStep([CanBeNull] ExampleKeyTutorialStep tutorialStep, int stepIndex, int stepCount)
        {
            if (ReferenceEquals(tutorialStep, null) || !tutorialStep) return;

            _titleText.text = tutorialStep.DisplayTitle;
            _descriptionText.text = tutorialStep.DisplayDescription;
            _progressText.text = $"STEP {stepIndex + 1} OF {stepCount}";
            Show();
        }

        /// <summary>
        ///     Plays the configured SimpleUI hide animation.
        /// </summary>
        public void HideStep()
        {
            Hide();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(_titleText, "TutorialStepDisplay requires a title TextMeshProUGUI reference.");
            Assert.IsNotNull(_descriptionText,
                "TutorialStepDisplay requires a description TextMeshProUGUI reference.");
            Assert.IsNotNull(_progressText, "TutorialStepDisplay requires a progress TextMeshProUGUI reference.");
        }
    }
}
