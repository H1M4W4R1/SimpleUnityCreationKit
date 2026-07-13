using Systems.SimpleUI.Components.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;

namespace Systems.SimpleUI.Components.Text
{
    /// <summary>
    ///     Drives a <see cref="TextMeshProUGUI"/> from a <see cref="LocalizedString"/>,
    ///     refreshing automatically whenever the active locale changes.
    /// </summary>
    [AddComponentMenu("Simple UI/Localization/Localized Text")]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILocalizedTextObject : UIObjectBase
    {
        [SerializeField] private LocalizedString _localizedString = new();

        [field: SerializeField, HideInInspector] private TextMeshProUGUI TextReference { get; set; }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            _localizedString.StringChanged += OnStringChanged;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            _localizedString.StringChanged -= OnStringChanged;
        }

        private void OnStringChanged(string value) => TextReference.SetText(value);

        protected override void OnValidate()
        {
            base.OnValidate();
            TextReference = GetComponent<TextMeshProUGUI>();
            Assert.IsNotNull(TextReference, "UILocalizedTextObject requires a TextMeshProUGUI component.");
        }
    }
}
