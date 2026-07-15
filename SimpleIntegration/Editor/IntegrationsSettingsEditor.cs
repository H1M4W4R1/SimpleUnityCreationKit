#if UNITY_EDITOR
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Data.Databases;
using UnityEditor;

namespace Systems.SimpleIntegration.Editor
{
    /// <summary>Registers the project settings page for configured platform integrations.</summary>
    internal static class IntegrationsSettingsEditor
    {
        private const string MENU_PATH = "Project/Integrations";
        private const string EDITOR_NAME = "Integrations";

        [SettingsProvider]
        [NotNull]
        public static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new SettingsProvider(MENU_PATH, SettingsScope.Project)
            {
                label = EDITOR_NAME,
                guiHandler = DrawGUI,
                keywords = new HashSet<string>
                {
                    "integrations",
                    "steam",
                    "epic",
                    "platform"
                }
            };

            return provider;
        }

        private static void DrawGUI(string searchContext)
        {
            ROListAccess<IntegratedPlatformBase> platforms = IntegratedPlatformDatabase.GetAllPlatforms();
            if (platforms.List.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No platform integration assets were found. Create a concrete IntegratedPlatformBase " +
                    "implementation to generate an addressable integration asset.",
                    MessageType.Info);
                platforms.Release();
                return;
            }

            for (int platformIndex = 0; platformIndex < platforms.List.Count; platformIndex++)
            {
                IntegratedPlatformBase platform = platforms.List[platformIndex];
                if (!platform) continue;

                EditorGUILayout.LabelField(platform.PlatformName, EditorStyles.boldLabel);
                SerializedObject serializedPlatform = new SerializedObject(platform);
                serializedPlatform.Update();
                platform.DrawSettings(serializedPlatform);
                serializedPlatform.ApplyModifiedProperties();
                EditorGUILayout.Space();
            }

            platforms.Release();
        }
    }
}
#endif
