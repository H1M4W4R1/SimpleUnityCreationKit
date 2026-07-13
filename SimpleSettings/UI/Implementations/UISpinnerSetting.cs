using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Selectors.Buttons;
using Systems.SimpleUI.Components.Selectors.Implementations.Spinner;
using Systems.SimpleUI.Context.Selectors;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Abstract left/right spinner UI component bound to a <see cref="Setting{TValue}"/>
    ///     that implements <see cref="ISelectableSetting{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The setting's value type.</typeparam>
    /// <remarks>
    ///     This class is abstract because Unity requires concrete (non-generic) MonoBehaviour types.
    ///     Create a sealed subclass per use case:
    ///     <code>
    ///     public sealed class UIQualitySpinner : UISpinnerSetting&lt;int&gt; { }
    ///     </code>
    ///     Uses <see cref="UISelectorNextButton"/> / <see cref="UISelectorPreviousButton"/> components
    ///     in the hierarchy to drive left/right navigation.
    /// </remarks>
    public abstract class UISpinnerSetting<TValue> :
        UISpinnerSelectorBase<TValue>,
        IWithLocalContext<SelectableContext<TValue>>
    {
        [SerializeField] private SettingBinding _binding = new();

        private Setting<TValue> _setting;
        private ISelectableSetting<TValue> _selectableSetting;
        private SettingSelectableContext<TValue> _context;

        // ───── IWithLocalContext<SelectableContext<TValue>> ────────────────

        bool IWithLocalContext<SelectableContext<TValue>>.TryGetContext(
            out SelectableContext<TValue> context)
        {
            context = _context;
            return _context != null;
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
            if (_setting != null) _setting.OnValueChanged -= SyncSelectionFromSetting;
        }

        // ─────────────────── UISelectorBase overrides ─────────────────────

        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);
            if (_setting == null || Context == null || !Context.IsSelected) return;
            _setting.Set(Context.SelectedItem);
        }

        // ─────────────────────── Binding ──────────────────────────────────

        private void BindSetting()
        {
            ISetting resolved = _binding.Resolve();
            if (resolved == null) return;

            _selectableSetting = resolved as ISelectableSetting<TValue>;
            _setting           = resolved as Setting<TValue>;

            if (_selectableSetting == null)
            {
                Debug.LogWarning($"[SimpleSettings] Resolved setting '{resolved.Key}' does not " +
                                 $"implement ISelectableSetting<{typeof(TValue).Name}>.");
                return;
            }

            IReadOnlyList<TValue> options = _selectableSetting.GetTypedOptions();
            int defaultIndex              = _selectableSetting.SelectedIndex;

            _context = new SettingSelectableContext<TValue>(options, defaultIndex);
            SetDirty();

            if (_setting != null)
                _setting.OnValueChanged += SyncSelectionFromSetting;
        }

        private void SyncSelectionFromSetting()
        {
            if (_selectableSetting == null || _context == null) return;
            _context.TrySelectIndex(_selectableSetting.SelectedIndex);
            RequestRefresh();
        }
    }
}
