using Systems.SimpleAchievements.Utility;
using UnityEngine;

namespace Systems.SimpleAchievements.Data.Settings
{
    /// <summary>
    ///     Project-wide configuration for the SimpleAchievements system.
    ///     Stored as a ScriptableObject in <c>Assets/Resources/AchievementsSettings.asset</c>
    ///     and auto-created on first access in the editor.
    ///     Configure in <c>Edit &gt; Project Settings &gt; Achievements</c>.
    /// </summary>
    public sealed class AchievementsSettings : ScriptableObject
    {
        private const string RESOURCE_PATH = "AchievementsSettings";
        private const string DEFAULT_SAVE_FILE_NAME = "achievements.json";

        private static AchievementsSettings _instance;

        /// <summary>Singleton accessor. Auto-creates the asset in the editor if missing.</summary>
        public static AchievementsSettings Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = Resources.Load<AchievementsSettings>(RESOURCE_PATH);

#if UNITY_EDITOR
                if (ReferenceEquals(_instance, null))
                    _instance = CreateAndSaveDefault();
#endif

                if (ReferenceEquals(_instance, null))
                    _instance = CreateInstance<AchievementsSettings>();

                return _instance;
            }
        }

        [SerializeField] private bool _autoSaveOnUnlock = true;

        [SerializeField] private string _saveFileName = DEFAULT_SAVE_FILE_NAME;

        /// <summary>
        ///     When <c>true</c> the registry automatically writes a save file to disk each time an
        ///     achievement is unlocked. Disable if you prefer to call 
        ///     <see cref="AchievementAPI.Save"/> manually.
        /// </summary>
        public bool AutoSaveOnUnlock => _autoSaveOnUnlock;

        /// <summary>
        ///     File name (not path) of the JSON save file written to
        ///     <c>Application.persistentDataPath</c>.
        ///     Defaults to <c>achievements.json</c>.
        /// </summary>
        public string SaveFileName =>
            string.IsNullOrWhiteSpace(_saveFileName) ? DEFAULT_SAVE_FILE_NAME : _saveFileName;

#if UNITY_EDITOR
        private static AchievementsSettings CreateAndSaveDefault()
        {
            AchievementsSettings settings = CreateInstance<AchievementsSettings>();

            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");

            UnityEditor.AssetDatabase.CreateAsset(
                settings, $"Assets/Resources/{RESOURCE_PATH}.asset");
            UnityEditor.AssetDatabase.SaveAssets();

            return settings;
        }
#endif
    }
}
