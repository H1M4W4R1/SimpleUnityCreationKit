using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Systems.SimpleSettings.Settings.Localization
{
    /// <summary>
    ///     Language (locale) selection setting.
    ///     Options are populated from <see cref="LocalizationSettings.AvailableLocales"/>.
    ///     The value is the locale identifier code string (e.g. <c>"en"</c>, <c>"fr"</c>).
    /// </summary>
    public sealed class LanguageSetting : Setting<string>, ISelectableSetting<string>
    {
        private IReadOnlyList<string> _optionCodes;

        /// <summary>Creates a new language setting defaulting to the currently selected locale.</summary>
        public LanguageSetting() : base(GetCurrentLocaleCode())
        {
            RefreshOptions();
        }

        // ──────────────────── ISelectableSetting<string> ──────────────────

        /// <inheritdoc/>
        public int SelectedIndex
        {
            get
            {
                EnsureOptions();
                string current = CurrentValue;
                for (int i = 0; i < _optionCodes.Count; i++)
                    if (_optionCodes[i] == current) return i;
                return 0;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<object> GetOptions()
        {
            EnsureOptions();
            List<object> boxed = new(_optionCodes.Count);
            foreach (string c in _optionCodes) boxed.Add(c);
            return boxed;
        }

        /// <inheritdoc/>
        public string GetOptionLabel(object option) =>
            option is string s ? GetTypedOptionLabel(s) : option?.ToString() ?? string.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<string> GetTypedOptions()
        {
            EnsureOptions();
            return _optionCodes;
        }

        /// <inheritdoc/>
        public string GetTypedOptionLabel(string code)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            return locale != null ? locale.LocaleName : code;
        }

        // ─────────────────── Setting<string> overrides ────────────────────

        /// <inheritdoc/>
        protected override void OnApplyInternal(string code)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale != null) LocalizationSettings.SelectedLocale = locale;
        }

        // ─────────────────────── Helpers ──────────────────────────────────

        private void EnsureOptions()
        {
            if (_optionCodes == null || _optionCodes.Count == 0)
                RefreshOptions();
        }

        private void RefreshOptions()
        {
            IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
            List<string> codes = new(locales.Count);
            foreach (Locale locale in locales)
                codes.Add(locale.Identifier.Code);
            _optionCodes = codes;
        }

        [CanBeNull]
        private static string GetCurrentLocaleCode()
        {
            Locale selected = LocalizationSettings.SelectedLocale;
            return selected != null ? selected.Identifier.Code : string.Empty;
        }
    }
}
