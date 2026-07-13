using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleDialogue.Abstract;
using Systems.SimpleDialogue.Data;
using Systems.SimpleUI.Components.Panels;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleDialogue.UI
{
    /// <summary>
    ///     Bottom-of-screen visual novel style renderer built with SimpleUI and TextMeshPro.
    /// </summary>
    public sealed class SimpleVisualNovelDialogueRenderer : UIPanelBase, IDialogueRenderer
    {
        [SerializeField] private SimpleDialogueText _speakerText;
        [SerializeField] private SimpleDialogueText _bodyText;
        [SerializeField] private SimpleDialogueAnswerContainer _answerContainer;
        [SerializeField] private Button _continueButton;

        public void RenderDialogue(DialogueViewContext context)
        {
            if (!IsVisible) Show();

            _speakerText.SetText(context.SpeakerName);
            _bodyText.SetText(context.Text);
            _answerContainer.SetOptions(context.OptionsContext);
            
            _continueButton.gameObject.SetActive(context.CanAdvance);
            if (!context.CanAdvance || ReferenceEquals(context.Dialogue, null)) return;
            _continueButton.onClick.RemoveAllListeners();
            _continueButton.onClick.AddListener(() => context.Dialogue.Advance(ActionSource.External));
        }

        public void ClearDialogue()
        {
            _speakerText.SetText(string.Empty);
            _bodyText.SetText(string.Empty);
            _answerContainer.ClearOptions();
            if (IsVisible) Hide();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!_speakerText) _speakerText = GetComponentInChildren<SimpleDialogueText>();
            Assert.IsNotNull(_speakerText, "SimpleVisualNovelDialogueRenderer requires a speaker SimpleDialogueText.");
            Assert.IsNotNull(_bodyText, "SimpleVisualNovelDialogueRenderer requires a body SimpleDialogueText.");
            Assert.IsNotNull(_answerContainer, "SimpleVisualNovelDialogueRenderer requires a SimpleDialogueAnswerContainer.");
        }
    }
}
