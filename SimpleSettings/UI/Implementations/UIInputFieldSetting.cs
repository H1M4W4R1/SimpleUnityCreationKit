using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.InputFields;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Input field UI component bound to a <see cref="Setting{TValue}"/> of type
    ///     <see cref="string"/>. Submits the value on field end-edit.
    /// </summary>
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public sealed class UIInputFieldSetting : UIInputFieldBase, IWithLocalContext<ISetting>
    {
        [SerializeField] private SettingBinding _binding = new();

        private Setting<string> _setting;

        // ─────────────── IWithLocalContext<ISetting> ──────────────────────

        List<IContextProvider> IWithContext.AvailableContextProviders { get; } = new();
        bool IWithContext.IsDirty { get; set; }
        public void ValidateContext() { }

        bool IWithLocalContext<ISetting>.TryGetContext(out ISetting context)
        {
            context = _setting;
            return _setting != null;
        }

        // ─────────────────────── Lifecycle ────────────────────────────────

        protected override void OnSetupComplete()
        {
            base.OnSetupComplete();
            BindSetting();
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            if (_setting != null) _setting.OnValueChanged -= SyncFromSetting;
        }

        // ─────────────────── UIInputFieldBase ─────────────────────────────

        protected override void OnFieldEndEdited(string currentText) =>
            _setting?.Set(currentText);

        // ─────────────────────── Binding ──────────────────────────────────

        private void BindSetting()
        {
            _setting = _binding.Resolve<Setting<string>>();
            if (_setting == null) return;

            // Pre-fill with current value.
            // InputField text is set via the underlying TMP_InputField.
            _setting.OnValueChanged += SyncFromSetting;
            SyncFromSetting();
        }

        private void SyncFromSetting()
        {
            if (_setting != null && InputFieldReference != null)
                InputFieldReference.text = _setting.CurrentValue ?? string.Empty;
        }
    }
}
