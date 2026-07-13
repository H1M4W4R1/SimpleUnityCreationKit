using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using UnityEngine.InputSystem;

namespace Systems.SimpleSettings.Settings.Controls
{
    /// <summary>
    ///     Represents a single input binding override for one
    ///     <see cref="InputAction"/> / binding-index pair.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The value is the effective path string (e.g. <c>"&lt;Keyboard&gt;/space"</c>).
    ///         An empty string means the binding has no override (uses the asset default).
    ///     </para>
    ///     <para>
    ///         Because multiple instances of this class exist (one per action-binding),
    ///         the <see cref="ISetting.Key"/> is overridden to
    ///         <c>"{actionName}/{bindingIndex}"</c> in the constructor rather than using
    ///         the default <c>GetType().Name</c>.
    ///     </para>
    /// </remarks>
    public sealed class InputBindingSetting : Setting<string>, IKeybindSetting
    {
        // ──────────────────── IKeybindSetting ─────────────────────────────

        /// <inheritdoc/>
        public InputAction Action { get; }

        /// <inheritdoc/>
        public int BindingIndex { get; }

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>
        ///     Creates a binding setting for the specified action and binding index.
        ///     The existing override path (if any) is used as the default value.
        /// </summary>
        public InputBindingSetting([NotNull] InputAction action, int bindingIndex) : base(string.Empty)
        {
            Action       = action;
            BindingIndex = bindingIndex;

            // Override the auto-assigned key with a unique path-based key.
            Key = $"{action.name}/{bindingIndex}";

            // Use any existing override as the starting default.
            string existingOverride = action.bindings[bindingIndex].overridePath;
            if (!string.IsNullOrEmpty(existingOverride))
                LoadValue(existingOverride);
        }

        // ─────────────────────── Overrides ────────────────────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(string value)
        {
            if (string.IsNullOrEmpty(value))
                Action.RemoveBindingOverride(BindingIndex);
            else
                Action.ApplyBindingOverride(BindingIndex, value);
        }
    }
}
