using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Selectors.Implementations.Dropdown;
using Systems.SimpleUI.Context.Selectors;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Abstract dropdown UI component bound to a <see cref="Setting{TValue}"/> that
    ///     implements <see cref="ISelectableSetting{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The setting's value type.</typeparam>
    /// <remarks>
    ///     <para>
    ///         This class is abstract because Unity requires concrete (non-generic)
    ///         MonoBehaviour types. Create a sealed subclass per use case:
    ///         <code>
    ///         public sealed class UIQualityDropdown : UIDropdownSetting&lt;int&gt; { }
    ///         </code>
    ///     </para>
    ///     <para>
    ///         Implements <see cref="IWithLocalContext{SelectableContext}"/> so the parent
    ///         dropdown mechanism receives the options list directly from the bound setting,
    ///         with no extra context provider in the hierarchy required.
    ///     </para>
    /// </remarks>
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public abstract class UIDropdownSetting<TValue> :
        UIDropdownSelectorBase<TValue>,
        IWithLocalContext<SelectableContext<TValue>>
    {
        [SerializeField] private SettingBinding _binding = new();

        private Setting<TValue> _setting;
        private ISelectableSetting<TValue> _selectableSetting;
        private SettingSelectableContext<TValue> _context;

        // ───── IWithLocalContext<SelectableContext<TValue>> ────────────────
        // UIDropdownSelectorBase already provides IWithContext backing via
        // UIObjectWithContextBase<SelectableContext<TValue>>; we only need
        // to supply TryGetContext to feed the local-context data.

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

        // ──────────────── UIDropdownSelectorBase ──────────────────────────

        /// <summary>
        ///     Returns the display label for the given option by delegating to
        ///     <see cref="ISelectableSetting{TValue}.GetTypedOptionLabel"/>.
        /// </summary>
        [NotNull] protected override string GetOptionLabel([CanBeNull] TValue obj) =>
            _selectableSetting?.GetTypedOptionLabel(obj) ?? obj?.ToString() ?? string.Empty;

        /// <inheritdoc/>
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

            // Mark dirty so UIObjectWithContextBase triggers an OnRender on next frame.
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
