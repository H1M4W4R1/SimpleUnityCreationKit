using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Settings.Localization;

namespace Systems.SimpleSettings.Groups
{
    /// <summary>
    ///     Built-in group for localization settings.
    ///     Requires the Unity Localization package to be installed and configured.
    /// </summary>
    public sealed class LocalizationSettingsGroup : SettingGroupBase
    {
        /// <inheritdoc/>
        public override string GroupId => "localization";

        // ─────────────────────── Settings ─────────────────────────────────

        /// <summary>Currently selected game language.</summary>
        [NotNull] public LanguageSetting Language { get; }

        // ──────────────────────── Constructor ─────────────────────────────

        /// <summary>Creates the localization settings group.</summary>
        public LocalizationSettingsGroup()
        {
            Language = new LanguageSetting();
            RegisterSettings(GetSettings());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ISetting> GetSettings() => new ISetting[] { Language };
    }
}
