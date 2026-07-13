using Systems.SimpleTutorial.Abstract;
using Systems.SimpleTutorial.Data;
using UnityEngine;

namespace Systems.SimpleTutorial.Examples
{
    /// <summary>
    ///     Completes when its configured keyboard key is pressed.
    /// </summary>
    public sealed class ExampleKeyTutorialStep : TutorialStep
    {
        [field: SerializeField] public string DisplayTitle { get; private set; } = "Press a key";
        [field: SerializeField, TextArea] public string DisplayDescription { get; private set; } = string.Empty;

        [SerializeField] private KeyCode _requiredKey = KeyCode.A;

        protected override bool IsComplete(in TutorialStepContext context)
        {
            return Input.GetKeyDown(_requiredKey);
        }
    }
}
