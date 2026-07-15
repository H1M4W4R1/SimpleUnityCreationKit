#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleAchievements.Data.Settings;
using UnityEditor;

namespace Systems.SimpleAchievements.Editor
{
    /// <summary>
    ///     Registers the <c>Project Settings &gt; Achievements</c> panel.
    ///     Platform integration settings are provided by SimpleIntegration.
    /// </summary>
    internal static class AchievementsSettingsEditor
    {
        private const string MENU_PATH = "Project/Achievements";
        private const string EDITOR_NAME = "Achievements";

        private static SerializedObject _settingsSO;

        [SettingsProvider]
        [NotNull]
        public static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new SettingsProvider(MENU_PATH, SettingsScope.Project)
            {
                label = EDITOR_NAME,
                activateHandler = OnActivate,
                guiHandler = DrawGUI,
                keywords = new HashSet<string>
                {
                    "achievements",
                    "unlock",
                    "save"
                }
            };

            return provider;
        }

        private static void OnActivate(string searchContext,
            UnityEngine.UIElements.VisualElement rootElement)
        {
            _settingsSO = null;
        }

        private static void DrawGUI(string searchContext)
        {
            AchievementsSettings settings = AchievementsSettings.Instance;
            if (ReferenceEquals(_settingsSO, null) || _settingsSO.targetObject != settings)
                _settingsSO = new SerializedObject(settings);

            _settingsSO.Update();
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("_autoSaveOnUnlock"));
            EditorGUILayout.PropertyField(_settingsSO.FindProperty("_saveFileName"));
            _settingsSO.ApplyModifiedProperties();
        }
    }
}
#endif
