#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract.Platforms;
using Systems.SimpleAchievements.Data.Databases;
using Systems.SimpleAchievements.Data.Settings;
using Systems.SimpleCore.Storage.Lists;
using UnityEditor;

namespace Systems.SimpleAchievements.Editor
{
    /// <summary>
    ///     Registers the <c>Project Settings &gt; Achievements</c> panel.
    ///     Renders general settings and iterates all registered
    ///     <see cref="AchievementPlatformBase"/> assets, delegating platform-specific
    ///     UI to each platform's <see cref="AchievementPlatformBase.DrawSettings"/> method.
    /// </summary>
    internal static class AchievementsSettingsEditor
    {
        private const string MENU_PATH   = "Project/Achievements";
        private const string EDITOR_NAME = "Achievements";

        private static SerializedObject _settingsSO;

        [SettingsProvider] [NotNull]
        public static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new SettingsProvider(MENU_PATH, SettingsScope.Project)
            {
                label           = EDITOR_NAME,
                activateHandler = OnActivate,
                guiHandler      = DrawGUI,
                keywords        = new HashSet<string>
                {
                    "achievements",
                    "steam",
                    "epic",
                    "platform",
                    "unlock"
                }
            };

            return provider;
        }

        private static void OnActivate(string searchContext,
            UnityEngine.UIElements.VisualElement rootElement)
        {
            // Invalidate cache on re-activation so stale serialized objects are not reused.
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Platforms", EditorStyles.boldLabel);

            DrawPlatformSettings();
        }

        private static void DrawPlatformSettings()
        {
            ROListAccess<AchievementPlatformBase> access =
                AchievementPlatformDatabase.GetAll<AchievementPlatformBase>();
            IReadOnlyList<AchievementPlatformBase> platforms = access.List;

            if (platforms.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No platform assets found in AchievementPlatformDatabase.\n" +
                    "Create a platform asset (e.g. SteamAchievementPlatform) and assign the " +
                    $"'{AchievementPlatformDatabase.LABEL}' Addressable label.",
                    MessageType.Info);
                access.Release();
                return;
            }

            for (int i = 0; i < platforms.Count; i++)
            {
                AchievementPlatformBase platform = platforms[i];
                if (!platform) continue;

                EditorGUILayout.LabelField(platform.PlatformName, EditorStyles.boldLabel);

                SerializedObject platformSO = new SerializedObject(platform);
                platformSO.Update();
                platform.DrawSettings(platformSO);
                platformSO.ApplyModifiedProperties();

                EditorGUILayout.Space();
            }

            access.Release();
        }
    }
}
#endif
