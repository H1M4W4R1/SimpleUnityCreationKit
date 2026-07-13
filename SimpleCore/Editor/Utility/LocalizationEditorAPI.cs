#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Systems.SimpleCore.Editor.Utility
{
    /// <summary>
    ///     Creates the minimal project-wide Unity Localization setup and CSV-enabled string table collections.
    /// </summary>
    public static class LocalizationEditorAPI
    {
        public const string GENERATED_DIRECTORY = "Assets/Generated/Localization";
        public const string DEFAULT_LOCALE_CODE = "en";

        /// <summary>
        ///     Returns the active Localization Settings, creating it together with the default English locale when needed.
        /// </summary>
        [NotNull] public static LocalizationSettings EnsureLocalizationSettings()
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (ReferenceEquals(settings, null))
            {
                EnsureDirectory(GENERATED_DIRECTORY);
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Localization Settings";
                AssetDatabase.CreateAsset(settings, GENERATED_DIRECTORY + "/Localization Settings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            EnsureLocale();
            return settings;
        }

        /// <summary>
        ///     Returns a string table collection with a configured CSV extension.
        /// </summary>
        [NotNull] public static StringTableCollection EnsureCsvStringTableCollection([NotNull] string tableCollectionName)
        {
            EnsureLocalizationSettings();
            Locale defaultLocale = EnsureLocale();

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableCollectionName);
            if (ReferenceEquals(collection, null))
            {
                string tablesDirectory = GENERATED_DIRECTORY + "/String Tables";
                EnsureDirectory(tablesDirectory);
                List<Locale> locales = new();
                IList<Locale> configuredLocales = LocalizationEditorSettings.GetLocales();
                for (int localeIndex = 0; localeIndex < configuredLocales.Count; localeIndex++)
                {
                    locales.Add(configuredLocales[localeIndex]);
                }

                if (locales.Count == 0) locales.Add(defaultLocale);
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    tableCollectionName,
                    tablesDirectory,
                    locales);
            }

            if (ReferenceEquals(collection.GetTable(defaultLocale.Identifier), null))
            {
                collection.AddNewTable(defaultLocale.Identifier);
                EditorUtility.SetDirty(collection);
            }

            if (!HasCsvExtension(collection))
            {
                CsvExtension csvExtension = new CsvExtension();
                collection.AddExtension(csvExtension);
                EditorUtility.SetDirty(collection);
            }

            AssetDatabase.SaveAssets();
            return collection;
        }

        [NotNull] private static Locale EnsureLocale()
        {
            Locale locale = LocalizationEditorSettings.GetLocale(DEFAULT_LOCALE_CODE);
            if (!ReferenceEquals(locale, null)) return locale;

            string localesDirectory = GENERATED_DIRECTORY + "/Locales";
            EnsureDirectory(localesDirectory);
            locale = Locale.CreateLocale(DEFAULT_LOCALE_CODE);
            AssetDatabase.CreateAsset(locale, localesDirectory + "/English.asset");
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        private static bool HasCsvExtension([NotNull] StringTableCollection collection)
        {
            for (int extensionIndex = 0; extensionIndex < collection.Extensions.Count; extensionIndex++)
            {
                if (collection.Extensions[extensionIndex] is CsvExtension) return true;
            }

            return false;
        }

        private static void EnsureDirectory([NotNull] string directory)
        {
            if (AssetDatabase.IsValidFolder(directory)) return;

            string parentDirectory = System.IO.Path.GetDirectoryName(directory)?.Replace('\\', '/');
            string directoryName = System.IO.Path.GetFileName(directory);
            if (string.IsNullOrEmpty(parentDirectory) || string.IsNullOrEmpty(directoryName)) return;

            EnsureDirectory(parentDirectory);
            AssetDatabase.CreateFolder(parentDirectory, directoryName);
        }
    }
}
#endif
