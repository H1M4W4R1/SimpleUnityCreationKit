using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleSettings.Tests
{
    public abstract class SimpleSettingsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();
        private readonly List<string> _settingsFileNames = new List<string>();

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();

            for (int fileIndex = 0; fileIndex < _settingsFileNames.Count; fileIndex++)
            {
                string path = GetSettingsPath(_settingsFileNames[fileIndex]);
                if (File.Exists(path)) File.Delete(path);
            }

            _settingsFileNames.Clear();
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected string CreateSettingsFileName(string prefix)
        {
            string fileName = prefix + "_" + Guid.NewGuid().ToString("N");
            _settingsFileNames.Add(fileName);
            return fileName;
        }

        protected static string GetSettingsPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, "Settings", fileName + ".json");
        }

        protected SettingsManager CreateManager(SaveMode saveMode, string sharedFileName)
        {
            GameObject gameObject = Track(new GameObject("SettingsManagerTests"));
            gameObject.SetActive(false);

            SettingsManager manager = gameObject.AddComponent<SettingsManager>();
            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("_enableGraphics").boolValue = false;
            serializedManager.FindProperty("_enableAudio").boolValue = false;
            serializedManager.FindProperty("_enableControls").boolValue = false;
            serializedManager.FindProperty("_enableLocalization").boolValue = false;
            serializedManager.FindProperty("_saveMode").enumValueIndex = (int)saveMode;
            serializedManager.FindProperty("_sharedFileName").stringValue = sharedFileName;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            gameObject.SetActive(true);
            manager.AwakeForTests();
            return manager;
        }

        protected static int CountSettings(SettingGroupBase group)
        {
            int count = 0;
            IEnumerator<ISetting> enumerator = group.GetAllSettings().GetEnumerator();
            try
            {
                while (enumerator.MoveNext()) count++;
            }
            finally
            {
                enumerator.Dispose();
            }

            return count;
        }
    }

    public enum TestSettingsMode
    {
        Low,
        High,
    }

    public sealed class NamedIntSetting : Setting<int>
    {
        public int ApplyCount { get; private set; }
        public int PreviewCount { get; private set; }
        public int LastAppliedValue { get; private set; }
        public int LastPreviewValue { get; private set; }

        public NamedIntSetting(string key, int defaultValue = 0) : base(defaultValue)
        {
            Key = key;
        }

        protected override void OnApplyInternal(int value)
        {
            ApplyCount++;
            LastAppliedValue = value;
        }

        protected override void OnCurrentValueChanged(int value)
        {
            PreviewCount++;
            LastPreviewValue = value;
        }
    }

    public sealed class TextSetting : Setting<string>
    {
        public string LastAppliedValue { get; private set; }

        public TextSetting(string key, string defaultValue = "") : base(defaultValue)
        {
            Key = key;
        }

        protected override void OnApplyInternal(string value)
        {
            LastAppliedValue = value;
        }
    }

    public sealed class ModeSetting : Setting<TestSettingsMode>
    {
        public TestSettingsMode LastAppliedValue { get; private set; }

        public ModeSetting(string key, TestSettingsMode defaultValue = TestSettingsMode.Low) : base(defaultValue)
        {
            Key = key;
        }

        protected override void OnApplyInternal(TestSettingsMode value)
        {
            LastAppliedValue = value;
        }
    }

    public sealed class TestSettingsGroup : SettingGroupBase
    {
        private readonly ISetting[] _settings;

        public TestSettingsGroup(string groupId, string saveFileName, params ISetting[] settings)
        {
            GroupIdValue = groupId;
            SaveFileNameValue = saveFileName;
            _settings = settings;
            RegisterSettings(_settings);
        }

        public string GroupIdValue { get; }
        public string SaveFileNameValue { get; }
        public int AppliedCount { get; private set; }

        public override string GroupId => GroupIdValue;
        public override string SaveFileName => SaveFileNameValue;

        protected override IEnumerable<ISetting> GetSettings()
        {
            return _settings;
        }

        protected override void OnGroupApplied()
        {
            AppliedCount++;
        }
    }
}
