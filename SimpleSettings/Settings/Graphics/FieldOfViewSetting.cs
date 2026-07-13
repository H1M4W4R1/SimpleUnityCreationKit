using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     Camera field-of-view setting (degrees).
    ///     Applies to <see cref="Camera.main"/> when <see cref="Setting{TValue}.Apply"/> is called.
    /// </summary>
    /// <remarks>
    ///     Override <see cref="Setting{TValue}.OnCurrentValueChanged"/> to preview FoV while dragging
    ///     a slider. Override <see cref="OnApplyInternal"/> in a subclass to target
    ///     a different camera.
    /// </remarks>
    public sealed class FieldOfViewSetting : Setting<float>, ISliderSetting
    {
        // ─────────────────────── ISliderSetting ───────────────────────────
        /// <inheritdoc/>
        public float MinValue => 60f;

        /// <inheritdoc/>
        public float MaxValue => 120f;

        /// <inheritdoc/>
        public float Step => 0f;

        // ──────────────────────── Constructor ─────────────────────────────
        /// <summary>Creates a new <see cref="FieldOfViewSetting"/> with a default of 60 degrees.</summary>
        public FieldOfViewSetting() : base(60f) { }

        // ─────────────────────── Overrides ────────────────────────────────
        /// <inheritdoc/>
        protected override void OnApplyInternal(float value)
        {
            if (Camera.main != null)
                Camera.main.fieldOfView = value;
        }
    }
}
