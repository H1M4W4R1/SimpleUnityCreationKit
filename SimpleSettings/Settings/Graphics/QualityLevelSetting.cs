using System.Collections.Generic;
using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     Unity quality level setting. Options are populated from
    ///     <see cref="QualitySettings.names"/>.
    /// </summary>
    public sealed class QualityLevelSetting : Setting<int>, ISelectableSetting<int>
    {
        private readonly IReadOnlyList<int> _options;
        private readonly string[] _names;

        /// <summary>Creates a new quality level setting defaulting to the current quality level.</summary>
        public QualityLevelSetting() : base(QualitySettings.GetQualityLevel())
        {
            _names = QualitySettings.names;
            int[] indices = new int[_names.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
            _options = indices;
        }

        // ──────────────────── ISelectableSetting<int> ─────────────────────

        /// <inheritdoc/>
        public int SelectedIndex => CurrentValue;

        /// <inheritdoc/>
        public IReadOnlyList<object> GetOptions()
        {
            List<object> boxed = new(_options.Count);
            foreach (int i in _options) boxed.Add(i);
            return boxed;
        }

        /// <inheritdoc/>
        public string GetOptionLabel(object option) =>
            option is int i ? GetTypedOptionLabel(i) : option?.ToString() ?? string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<int> GetTypedOptions() => _options;

        /// <inheritdoc/>
        public string GetTypedOptionLabel(int index) =>
            index >= 0 && index < _names.Length ? _names[index] : index.ToString();

        // ─────────────────── Setting<int> overrides ───────────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(int value) =>
            QualitySettings.SetQualityLevel(value, applyExpensiveChanges: true);
    }
}
