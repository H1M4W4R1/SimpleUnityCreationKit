using System.Collections.Generic;
using Systems.SimpleCore.Input;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Settings.Controls;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Buttons;
using Systems.SimpleUI.Context.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Button UI component that starts an InputSystem rebind flow for the
    ///     <see cref="InputBindingSetting"/> identified by <c>_binding</c>.
    ///     Shows the current binding display name as the button label.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         On click, the action's map is disabled and an interactive rebind operation
    ///         starts. Pressing Escape cancels the operation. Once complete, the setting
    ///         value is updated with the new binding path.
    ///     </para>
    ///     <para>
    ///         Uses the InputSystem <c>PerformInteractiveRebinding</c> API directly.
    ///         Override <see cref="StartRebindInternal"/> to plug in a custom rebind flow
    ///         (e.g. one that uses InputAPI's device-filtering machinery).
    ///     </para>
    /// </remarks>
    public sealed class UIKeybindButton : UIButtonBase, IWithLocalContext<ISetting>
    {
        [SerializeField] private SettingBinding _binding = new();

        [SerializeField, Tooltip("Label shown while waiting for input.")]
        private string _listeningLabel = "…";

        [SerializeField, Tooltip("Optional TMP_Text component that displays the current binding.")]
        private TMP_Text _bindingLabel;

        private InputBindingSetting _setting;
        private InputActionReference _actionReference;
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

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

            if (_setting != null) _setting.OnValueChanged -= RefreshLabel;

            _rebindOperation?.Cancel();
            _rebindOperation?.Dispose();
            _rebindOperation = null;
        }

        // ─────────────────────── UIButtonBase ─────────────────────────────

        protected override void OnClick()
        {
            if (_rebindOperation != null || _setting == null) return;
            StartRebindInternal();
        }

        // ─────────────────────── Binding ──────────────────────────────────

        private void BindSetting()
        {
            _setting = _binding.Resolve<InputBindingSetting>();
            if (_setting == null) return;

            _actionReference = InputActionReference.Create(_setting.Action);
            _setting.OnValueChanged += RefreshLabel;
            RefreshLabel();
        }

        // ─────────────────────── Rebind flow ──────────────────────────────

        /// <summary>
        ///     Starts the interactive rebind. Override to use a custom flow
        ///     (e.g. route through <see cref="InputAPI"/>).
        /// </summary>
        private void StartRebindInternal()
        {
            // Disable the map so the action does not fire during rebind.
            _setting.Action.actionMap?.Disable();
            SetInteractable(false);

            if (_bindingLabel is not null) _bindingLabel.text = _listeningLabel;

            _rebindOperation = _setting.Action
                .PerformInteractiveRebinding(_setting.BindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(OnOperationCancelled)
                .OnComplete(OnOperationCompleted)
                .Start();
        }

        private void OnOperationCompleted(InputActionRebindingExtensions.RebindingOperation op)
        {
            // The operation already wrote the override to the action; capture the path.
            string newPath = _setting.Action.bindings[_setting.BindingIndex].effectivePath;
            _setting.Set(newPath);
            FinishRebind();
        }

        private void OnOperationCancelled(InputActionRebindingExtensions.RebindingOperation op) =>
            FinishRebind();

        private void FinishRebind()
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            _setting.Action.actionMap?.Enable();
            SetInteractable(true);
            RefreshLabel();
        }

        // ─────────────────────── Label helpers ────────────────────────────

        private void RefreshLabel()
        {
            if (_bindingLabel is null || _setting == null) return;
            _bindingLabel.text = InputAPI.GetBindingDisplayName(_actionReference, _setting.BindingIndex);
        }
    }
}
