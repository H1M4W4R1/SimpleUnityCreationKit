using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleSettings.Saving
{
    /// <summary>
    ///     Static utility for reading and writing settings files to disk.
    ///     Files are stored as JSON under <c>Application.persistentDataPath/Settings/</c>.
    /// </summary>
    public static class SettingsFileIO
    {
        private static string Dir =>
            Path.Combine(Application.persistentDataPath, "Settings");

        private static string GetPath([NotNull] string fileName) =>
            Path.Combine(Dir, $"{fileName}.json");

        // ─────────────── Single-group file (PerGroup mode) ───────────────

        /// <summary>
        ///     Serializes <paramref name="file"/> to JSON and writes it to
        ///     <c>{persistentDataPath}/Settings/{fileName}.json</c>.
        /// </summary>
        public static void WriteGroup([NotNull] SettingsSaveFile file,
                                      [NotNull] string fileName)
        {
            EnsureDirectoryExists();
            string json = JsonUtility.ToJson(file, prettyPrint: true);
            File.WriteAllText(GetPath(fileName), json);
        }

        /// <summary>
        ///     Attempts to read and deserialize a single-group file.
        /// </summary>
        /// <returns><c>true</c> if the file existed and was parsed successfully.</returns>
        public static bool TryReadGroup([NotNull] string fileName,
                                        [CanBeNull] out SettingsSaveFile file)
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
            {
                file = null;
                return false;
            }

            try
            {
                file = JsonUtility.FromJson<SettingsSaveFile>(File.ReadAllText(path));
                return file != null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SimpleSettings] Failed to read '{path}': {e.Message}");
                file = null;
                return false;
            }
        }

        // ────────────── Combined file (SingleFile mode) ────────────────

        /// <summary>
        ///     Serializes <paramref name="file"/> (which contains all groups) to JSON and
        ///     writes it to <c>{persistentDataPath}/Settings/{fileName}.json</c>.
        /// </summary>
        public static void WriteCombined([NotNull] CombinedSettingsSaveFile file,
                                         [NotNull] string fileName)
        {
            EnsureDirectoryExists();
            string json = JsonUtility.ToJson(file, prettyPrint: true);
            File.WriteAllText(GetPath(fileName), json);
        }

        /// <summary>
        ///     Attempts to read and deserialize a combined (all-groups) file.
        /// </summary>
        /// <returns><c>true</c> if the file existed and was parsed successfully.</returns>
        public static bool TryReadCombined([NotNull] string fileName,
                                           [CanBeNull] out CombinedSettingsSaveFile file)
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
            {
                file = null;
                return false;
            }

            try
            {
                file = JsonUtility.FromJson<CombinedSettingsSaveFile>(File.ReadAllText(path));
                return file != null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SimpleSettings] Failed to read '{path}': {e.Message}");
                file = null;
                return false;
            }
        }

        // ─────────────────────── Helpers ─────────────────────────────────

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
}
