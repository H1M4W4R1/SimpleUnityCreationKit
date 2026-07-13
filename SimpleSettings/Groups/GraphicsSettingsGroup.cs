using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Settings.Graphics;

namespace Systems.SimpleSettings.Groups
{
    /// <summary>
    ///     Built-in group that bundles all graphics-related settings:
    ///     field of view, resolution, quality level, fullscreen mode, VSync, and frame cap.
    /// </summary>
    public sealed class GraphicsSettingsGroup : SettingGroupBase
    {
        /// <inheritdoc/>
        public override string GroupId => "graphics";

        // ─────────────────────── Settings ─────────────────────────────────

        /// <summary>Camera field of view (degrees).</summary>
        [NotNull] public FieldOfViewSetting FieldOfView { get; }

        /// <summary>Screen resolution.</summary>
        [NotNull] public ResolutionSetting Resolution { get; }

        /// <summary>Unity quality level index.</summary>
        [NotNull] public QualityLevelSetting QualityLevel { get; }

        /// <summary>Fullscreen / windowed mode.</summary>
        [NotNull] public FullscreenModeSetting FullscreenMode { get; }

        /// <summary>VSync enable/disable.</summary>
        [NotNull] public VSyncSetting VSync { get; }

        /// <summary>Target frame rate cap.</summary>
        [NotNull] public FrameCapSetting FrameCap { get; }

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>Creates the graphics settings group with default values.</summary>
        public GraphicsSettingsGroup()
        {
            FieldOfView    = new FieldOfViewSetting();
            Resolution     = new ResolutionSetting();
            QualityLevel   = new QualityLevelSetting();
            FullscreenMode = new FullscreenModeSetting();
            VSync          = new VSyncSetting();
            FrameCap       = new FrameCapSetting();

            RegisterSettings(GetSettings());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ISetting> GetSettings() => new ISetting[]
        {
            FieldOfView,
            Resolution,
            QualityLevel,
            FullscreenMode,
            VSync,
            FrameCap,
        };
    }
}
