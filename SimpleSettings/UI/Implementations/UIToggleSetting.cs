using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Toggles;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Toggle UI component bound to a <see cref="Setting{TValue}"/> of type
    ///     <see cref="bool"/>.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public sealed class UIToggleSetting : UIToggleBase, IWithLocalContext<ISetting>
    {
        [SerializeField] private SettingBinding _binding = new();

        private Setting<bool> _setting;

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

        // ─────────────────────── UIToggleBase ─────────────────────────────

        protected override void OnToggleValueChanged(bool newValue) =>
            _setting?.Set(newValue);

        // ─────────────────────── Binding ──────────────────────────────────

        private void BindSetting()
        {
            _setting = _binding.Resolve<Setting<bool>>();
            if (_setting == null) return;

            IsToggled = _setting.CurrentValue;
            _setting.OnValueChanged += SyncFromSetting;
        }

        private void SyncFromSetting()
        {
            if (_setting != null) IsToggled = _setting.CurrentValue;
        }
    }
}
