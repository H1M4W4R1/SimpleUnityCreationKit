using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleDialogue.Data;
using Systems.SimpleUI.Components.Abstract.Markers;
using Systems.SimpleUI.Components.Lists;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
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

        [field: SerializeField, HideInInspector] private Button ButtonReference { get; set; }

        public void OnRender(DialogueOption withContext)
        {
            _text.text = ReferenceEquals(withContext.text, null) ? string.Empty : withContext.text;
            ButtonReference.interactable = withContext.isAvailable;
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            ButtonReference.onClick.AddListener(OnClick);
        }

        protected override void DetachEvents()
        {
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
            option.Select(ActionSource.External);
        }
    }
}
