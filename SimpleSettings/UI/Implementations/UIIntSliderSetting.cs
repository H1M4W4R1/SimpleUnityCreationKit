using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Sliders;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Slider UI component bound to a <see cref="Setting{TValue}"/> of type
    ///     <see cref="int"/>. The underlying Unity slider uses <c>float</c>;
    ///     values are converted via <see cref="Mathf.RoundToInt"/>.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    public sealed class UIIntSliderSetting : UISliderBase, IWithLocalContext<ISetting>
    {
        [SerializeField] private SettingBinding _binding = new();

        private Setting<int> _setting;

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

        // ─────────────────────── UISliderBase ─────────────────────────────

        protected override void OnSliderValueChanged(float newValue) =>
            _setting?.Set(Mathf.RoundToInt(newValue));

        // ─────────────────────── Binding ──────────────────────────────────

        private void BindSetting()
        {
            _setting = _binding.Resolve<Setting<int>>();
            if (_setting == null) return;

            if (_setting is ISliderSetting meta)
            {
                MinValue = meta.MinValue;
                MaxValue = meta.MaxValue;
            }

            CurrentValue = _setting.CurrentValue;
            _setting.OnValueChanged += SyncFromSetting;
        }

        private void SyncFromSetting()
        {
            if (_setting != null) CurrentValue = _setting.CurrentValue;
        }
    }
}
