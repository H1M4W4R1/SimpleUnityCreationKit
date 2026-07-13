using Systems.SimpleSettings.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Settings.Graphics
{
    /// <summary>
    ///     VSync enable/disable setting.
    ///     When applied, sets <see cref="QualitySettings.vSyncCount"/> to 1 (on) or 0 (off).
    /// </summary>
    public sealed class VSyncSetting : Setting<bool>, IToggleSetting
    {
        /// <summary>Creates a new VSync setting defaulting to on (vSyncCount == 1).</summary>
        public VSyncSetting() : base(QualitySettings.vSyncCount > 0) { }

        /// <inheritdoc/>
        protected override void OnApplyInternal(bool value) =>
            QualitySettings.vSyncCount = value ? 1 : 0;
    }
}
