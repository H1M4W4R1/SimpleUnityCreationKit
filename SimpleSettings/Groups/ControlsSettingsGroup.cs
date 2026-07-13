using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Settings.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.SimpleSettings.Groups
{
    /// <summary>
    ///     Built-in group that exposes one <see cref="InputBindingSetting"/> per
    ///     action-binding pair found in the provided <see cref="InputActionAsset"/>.
    /// </summary>
    /// <remarks>
    ///     Settings are keyed as <c>"{actionName}/{bindingIndex}"</c> (e.g.
    ///     <c>"Jump/0"</c>, <c>"Move/1"</c>).  Each setting stores the binding
    ///     override path string; an empty string means "use the asset default".
    /// </remarks>
    public sealed class ControlsSettingsGroup : SettingGroupBase
    {
        /// <inheritdoc/>
        public override string GroupId => "controls";

        private readonly List<InputBindingSetting> _bindings = new();

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>
        ///     Creates the controls group by iterating all action maps, actions,
        ///     and bindings in <paramref name="asset"/> and generating one
        ///     <see cref="InputBindingSetting"/> per binding.
        /// </summary>
        /// <param name="asset">
        ///     The InputActionAsset to iterate. May be <c>null</c> — the group
        ///     will be created empty and no bindings will be registered.
        /// </param>
        public ControlsSettingsGroup([CanBeNull] InputActionAsset asset)
        {
            if (!asset)
            {
                Debug.LogWarning("[SimpleSettings] ControlsSettingsGroup created without " +
                                 "an InputActionAsset. No bindings will be available.");
                return;
            }

            foreach (InputActionMap map in asset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    for (int bi = 0; bi < action.bindings.Count; bi++)
                    {
                        // Skip composite parts (they are controlled via the composite binding).
                        if (action.bindings[bi].isPartOfComposite) continue;
                        _bindings.Add(new InputBindingSetting(action, bi));
                    }
                }
            }

            RegisterSettings(GetSettings());
        }

        // ─────────────────────── Public API ───────────────────────────────

        /// <summary>
        ///     Returns the <see cref="InputBindingSetting"/> for the specified
        ///     <paramref name="action"/> and <paramref name="bindingIndex"/>,
        ///     or <c>null</c> if not found.
        /// </summary>
        [CanBeNull] public InputBindingSetting GetBinding(
            [NotNull] string action, int bindingIndex)
        {
            string key = $"{action}/{bindingIndex}";
            foreach (InputBindingSetting b in _bindings)
                if (b.Key == key) return b;
            return null;
        }

        // ──────────────────────── Abstract impl ───────────────────────────

        /// <inheritdoc/>
        protected override IEnumerable<ISetting> GetSettings() => _bindings;
    }
}
