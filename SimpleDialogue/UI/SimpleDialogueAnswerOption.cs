using Systems.SimpleDialogue.Data;
using Systems.SimpleUI.Components.Abstract.Markers;
using Systems.SimpleUI.Components.Lists;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Systems.SimpleDialogue.UI
{
    /// <summary>
    ///     SimpleUI list element that renders and selects a dialogue answer.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class SimpleDialogueAnswerOption : UIListElementBase<DialogueOption>, IRenderable<DialogueOption>
    {
        [SerializeField] private TextMeshProUGUI _text;

        private LocalizedString _localizedText;

        [field: SerializeField, HideInInspector] private Button ButtonReference { get; set; }

        public void OnRender(DialogueOption withContext)
        {
            SetText(withContext.text);
            ButtonReference.interactable = withContext.isAvailable;
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            ButtonReference.onClick.AddListener(OnClick);
            SubscribeToLocalizedText();
        }

        protected override void DetachEvents()
        {
            UnsubscribeFromLocalizedText();
            ButtonReference.onClick.RemoveListener(OnClick);
            base.DetachEvents();
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ButtonReference = GetComponent<Button>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ButtonReference = GetComponent<Button>();
            if (!_text) _text = GetComponentInChildren<TextMeshProUGUI>();

            Assert.IsNotNull(ButtonReference, "SimpleDialogueAnswerOption requires a Button component.");
            Assert.IsNotNull(_text, "SimpleDialogueAnswerOption requires a TextMeshProUGUI child.");
        }

        private void OnClick()
        {
            if (!TryGetContext(out DialogueOption option)) return;
            option.Select();
        }

        private void SetText(LocalizedString localizedText)
        {
            if (ReferenceEquals(_localizedText, localizedText)) return;

            UnsubscribeFromLocalizedText();
            _localizedText = localizedText;
            SubscribeToLocalizedText();
            if (ReferenceEquals(_localizedText, null) || _localizedText.IsEmpty) _text.SetText(string.Empty);
        }

        private void SubscribeToLocalizedText()
        {
            if (!isActiveAndEnabled || ReferenceEquals(_localizedText, null) || _localizedText.IsEmpty) return;
            _localizedText.StringChanged += OnStringChanged;
        }

        private void UnsubscribeFromLocalizedText()
        {
            if (ReferenceEquals(_localizedText, null) || !_localizedText.HasChangeHandler) return;
            _localizedText.StringChanged -= OnStringChanged;
        }

        private void OnStringChanged(string value) => _text.SetText(value);
    }
}
