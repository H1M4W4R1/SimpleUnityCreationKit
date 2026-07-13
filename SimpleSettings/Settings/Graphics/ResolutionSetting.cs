using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     Screen resolution setting. Options are populated from <see cref="Screen.resolutions"/>.
    /// </summary>
    public sealed class ResolutionSetting : Setting<Resolution>, ISelectableSetting<Resolution>
    {
        private readonly IReadOnlyList<Resolution> _options;

        /// <summary>Creates a new resolution setting defaulting to the current screen resolution.</summary>
        public ResolutionSetting() : base(Screen.currentResolution)
        {
            Resolution[] resolutions = Screen.resolutions;
            _options = resolutions;
        }

        // ─────────────────── ISelectableSetting<Resolution> ───────────────

        /// <inheritdoc/>
        public int SelectedIndex
        {
            get
            {
                Resolution current = CurrentValue;
                for (int i = 0; i < _options.Count; i++)
                {
                    Resolution r = _options[i];
                    if (r.width == current.width && r.height == current.height) return i;
                }

                return 0;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<object> GetOptions()
        {
            List<object> boxed = new(_options.Count);
            foreach (Resolution r in _options) boxed.Add(r);
            return boxed;
        }

        /// <inheritdoc/>
        public string GetOptionLabel(object option)
        {
            if (option is Resolution r) return GetTypedOptionLabel(r);
            return option?.ToString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Resolution> GetTypedOptions() => _options;

        /// <inheritdoc/>
        public string GetTypedOptionLabel(Resolution r) =>
            $"{r.width}×{r.height} @ {r.refreshRateRatio.value:F0} Hz";

        // ─────────────────── Setting<Resolution> overrides ────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(Resolution value) =>
            Screen.SetResolution(value.width, value.height, Screen.fullScreenMode,
                                 value.refreshRateRatio);

        /// <inheritdoc/>
        protected override string SerializeValue() =>
            $"{CurrentValue.width},{CurrentValue.height}," +
            $"{CurrentValue.refreshRateRatio.numerator}," +
            $"{CurrentValue.refreshRateRatio.denominator}";

        /// <inheritdoc/>
        protected override bool TryDeserializeValue(string serialized, out Resolution value)
        {
            value = default;
            string[] parts = serialized.Split(',');
            if (parts.Length < 4) return false;

            if (!int.TryParse(parts[0], out int w)   ||
                !int.TryParse(parts[1], out int h)   ||
                !uint.TryParse(parts[2], out uint n) ||
                !uint.TryParse(parts[3], out uint d)) return false;

            value = new Resolution
            {
                width              = w,
                height             = h,
                refreshRateRatio   = new RefreshRate { numerator = n, denominator = d },
            };
            return true;
        }
    }
}
