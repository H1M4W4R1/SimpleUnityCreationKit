using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

namespace Systems.SimpleDialogue.UI
{
    /// <summary>
    ///     TextMeshPro-backed SimpleUI text element used by dialogue renderers.
    /// </summary>
    public sealed class SimpleDialogueText : UIObjectWithContextBase<LocalizedString>,
        IWithLocalContext<LocalizedString>, IRenderable<LocalizedString>
    {
        [CanBeNull] private LocalizedString _localizedString;

        [field: SerializeField, HideInInspector] private TextMeshProUGUI TextReference { get; set; }

        /// <summary>
        ///     Sets the localized text reference and subscribes to locale updates while this element is enabled.
        /// </summary>
        public void SetText([CanBeNull] LocalizedString localizedString)
        {
            if (ReferenceEquals(_localizedString, localizedString)) return;

            UnsubscribeFromLocalizedString();
            _localizedString = localizedString;
            SubscribeToLocalizedString();
            if (ReferenceEquals(_localizedString, null) || _localizedString.IsEmpty)
                TextReference.SetText(string.Empty);
            RequestRefresh();
        }

        public bool TryGetContext(out LocalizedString context)
        {
            context = _localizedString;
            return !ReferenceEquals(context, null);
        }

        public void OnRender(LocalizedString withContext)
        {
            if (ReferenceEquals(withContext, null)) TextReference.SetText(string.Empty);
        }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            SubscribeToLocalizedString();
        }

        protected override void DetachEvents()
        {
            UnsubscribeFromLocalizedString();
            base.DetachEvents();
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            TextReference = GetComponent<TextMeshProUGUI>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            TextReference = GetComponent<TextMeshProUGUI>();
            Assert.IsNotNull(TextReference, "SimpleDialogueText requires a TextMeshProUGUI component.");
        }

        private void SubscribeToLocalizedString()
        {
            if (!isActiveAndEnabled || ReferenceEquals(_localizedString, null) || _localizedString.IsEmpty) return;
            _localizedString.StringChanged += OnStringChanged;
        }

        private void UnsubscribeFromLocalizedString()
        {
            if (ReferenceEquals(_localizedString, null) || !_localizedString.HasChangeHandler) return;
            _localizedString.StringChanged -= OnStringChanged;
        }

        private void OnStringChanged(string value) => TextReference.SetText(value);
    }
}
