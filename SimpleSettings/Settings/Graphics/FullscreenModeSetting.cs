using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     Fullscreen mode setting (Exclusive, Borderless, Windowed, etc.).
    /// </summary>
    public sealed class FullscreenModeSetting : Setting<FullScreenMode>,
        ISelectableSetting<FullScreenMode>
    {
        private static readonly IReadOnlyList<FullScreenMode> Options = new[]
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.MaximizedWindow,
            FullScreenMode.Windowed,
        };

        private static readonly string[] OptionLabels =
        {
            "Exclusive Fullscreen",
            "Borderless Window",
            "Maximized Window",
            "Windowed",
        };

        /// <summary>Creates a new fullscreen mode setting defaulting to the current mode.</summary>
        public FullscreenModeSetting() : base(Screen.fullScreenMode) { }

        // ─────────────── ISelectableSetting<FullScreenMode> ───────────────

        /// <inheritdoc/>
        public int SelectedIndex
        {
            get
            {
                FullScreenMode current = CurrentValue;
                for (int i = 0; i < Options.Count; i++)
                    if (Options[i] == current) return i;
                return 0;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<object> GetOptions()
        {
            List<object> boxed = new(Options.Count);
            foreach (FullScreenMode m in Options) boxed.Add(m);
            return boxed;
        }

        /// <inheritdoc/>
        public string GetOptionLabel(object option) =>
            option is FullScreenMode m
                ? GetTypedOptionLabel(m)
                : option?.ToString() ?? string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<FullScreenMode> GetTypedOptions() => Options;

        /// <inheritdoc/>
        public string GetTypedOptionLabel(FullScreenMode mode)
        {
            for (int i = 0; i < Options.Count; i++)
                if (Options[i] == mode) return OptionLabels[i];
            return mode.ToString();
        }

        // ──────────── Setting<FullScreenMode> overrides ───────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(FullScreenMode value) =>
            Screen.fullScreenMode = value;
    }
}
