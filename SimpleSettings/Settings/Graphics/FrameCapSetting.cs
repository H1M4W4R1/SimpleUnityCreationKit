using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     Target frame-rate cap setting.
    ///     Available options: 30, 60, 120, 144, 240, and -1 (Unlimited).
    /// </summary>
    public sealed class FrameCapSetting : Setting<int>, ISelectableSetting<int>
    {
        private static readonly IReadOnlyList<int> Options = new[] { 30, 60, 120, 144, 240, -1 };

        /// <summary>Creates a new frame-cap setting defaulting to 60 fps.</summary>
        public FrameCapSetting() : base(60) { }

        // ──────────────────── ISelectableSetting<int> ─────────────────────

        /// <inheritdoc/>
        public int SelectedIndex
        {
            get
            {
                int current = CurrentValue;
                for (int i = 0; i < Options.Count; i++)
                    if (Options[i] == current) return i;
                return 1; // Default to 60 fps.
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<object> GetOptions()
        {
            List<object> boxed = new(Options.Count);
            foreach (int v in Options) boxed.Add(v);
            return boxed;
        }

        /// <inheritdoc/>
        public string GetOptionLabel(object option) =>
            option is int v ? GetTypedOptionLabel(v) : option?.ToString() ?? string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<int> GetTypedOptions() => Options;

        /// <inheritdoc/>
        public string GetTypedOptionLabel(int fps) => fps == -1 ? "Unlimited" : $"{fps} fps";

        // ─────────────────── Setting<int> overrides ───────────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(int value) =>
            Application.targetFrameRate = value;
    }
}
