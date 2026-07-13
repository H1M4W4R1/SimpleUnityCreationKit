using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Abstract.Platforms;
using Systems.SimpleAchievements.Components;
using Systems.SimpleAchievements.Data.Databases;
using Systems.SimpleAchievements.Data.Settings;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Timing;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleAchievements.Tests
{
    public abstract class SimpleAchievementsTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();
        private readonly List<string> _achievementFileNames = new List<string>();
        private bool _originalAutoSaveOnUnlock;
        private string _originalSaveFileName;

        [SetUp]
        public void SetUp()
        {
            DestroyRegistriesAndTickSystems();
            AchievementDatabase.ClearForTests();
            AchievementPlatformDatabase.ClearForTests();
            StoreAndConfigureSettings(false, CreateAchievementFileName("setup"));
        }

        [TearDown]
        public void TearDown()
        {
            DestroyRegistriesAndTickSystems();
            AchievementDatabase.ClearForTests();
            AchievementPlatformDatabase.ClearForTests();
            RestoreSettings();

            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();

            for (int fileIndex = 0; fileIndex < _achievementFileNames.Count; fileIndex++)
            {
                string path = GetAchievementPath(_achievementFileNames[fileIndex]);
                if (File.Exists(path)) File.Delete(path);
            }

            _achievementFileNames.Clear();
        }

        protected TestAchievement CreateAchievement(
            string platformId,
            bool conditional = false,
            bool conditionMet = false)
        {
            TestAchievement achievement = Track(ScriptableObject.CreateInstance<TestAchievement>());
            achievement.name = platformId;
            achievement.ConditionalValue = conditional;
            achievement.ConditionMet = conditionMet;
            SetAchievementText(achievement, platformId, platformId + " Name", platformId + " Description");
            return achievement;
        }

        protected TestAchievement CreateRegisteredAchievement(
            string platformId,
            bool conditional = false,
            bool conditionMet = false)
        {
            TestAchievement achievement = CreateAchievement(platformId, conditional, conditionMet);
            AchievementDatabase.RegisterForTests(achievement);
            return achievement;
        }

        protected TestAchievementPlatform CreateRegisteredPlatform()
        {
            TestAchievementPlatform platform = Track(ScriptableObject.CreateInstance<TestAchievementPlatform>());
            AchievementPlatformDatabase.RegisterForTests(platform);
            return platform;
        }

        protected AchievementRegistry CreateRegistry()
        {
            AchievementRegistry registry = AchievementRegistry.Instance;
            registry.AwakeForTests();
            return registry;
        }

        protected string CreateAchievementFileName(string prefix)
        {
            string fileName = prefix + "_" + Guid.NewGuid().ToString("N") + ".json";
            _achievementFileNames.Add(fileName);
            return fileName;
        }

        protected void ConfigureSettings(bool autoSaveOnUnlock, string saveFileName)
        {
            SetSettings(autoSaveOnUnlock, saveFileName);
        }

        protected static string GetAchievementPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected + " but received " + actual);
        }

        private void StoreAndConfigureSettings(bool autoSaveOnUnlock, string saveFileName)
        {
            AchievementsSettings settings = AchievementsSettings.Instance;
            SerializedObject serializedSettings = new SerializedObject(settings);
            _originalAutoSaveOnUnlock = serializedSettings.FindProperty("_autoSaveOnUnlock").boolValue;
            _originalSaveFileName = serializedSettings.FindProperty("_saveFileName").stringValue;
            SetSettings(autoSaveOnUnlock, saveFileName);
        }

        private void RestoreSettings()
        {
            SetSettings(_originalAutoSaveOnUnlock, _originalSaveFileName);
        }

        private static void SetSettings(bool autoSaveOnUnlock, string saveFileName)
        {
            AchievementsSettings settings = AchievementsSettings.Instance;
            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("_autoSaveOnUnlock").boolValue = autoSaveOnUnlock;
            serializedSettings.FindProperty("_saveFileName").stringValue = saveFileName;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetAchievementText(
            AchievementData achievement,
            string platformId,
            string displayName,
            string description)
        {
            SerializedObject serializedAchievement = new SerializedObject(achievement);
            serializedAchievement.FindProperty("_platformId").stringValue = platformId;
            serializedAchievement.FindProperty("_displayName").stringValue = displayName;
            serializedAchievement.FindProperty("_description").stringValue = description;
            serializedAchievement.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DestroyRegistriesAndTickSystems()
        {
            AchievementRegistry[] registries =
                Object.FindObjectsByType<AchievementRegistry>(FindObjectsInactive.Include);
            for (int registryIndex = 0; registryIndex < registries.Length; registryIndex++)
            {
                AchievementRegistry registry = registries[registryIndex];
                if (ReferenceEquals(registry, null)) continue;
                Object.DestroyImmediate(registry.gameObject);
            }

            TickSystem[] tickSystems =
                Object.FindObjectsByType<TickSystem>(FindObjectsInactive.Include);
            for (int tickSystemIndex = 0; tickSystemIndex < tickSystems.Length; tickSystemIndex++)
            {
                TickSystem tickSystem = tickSystems[tickSystemIndex];
                if (ReferenceEquals(tickSystem, null)) continue;
                Object.DestroyImmediate(tickSystem.gameObject);
            }
        }
    }

    public sealed class TestAchievement : AchievementData
    {
        public bool ConditionalValue { get; set; }
        public bool ConditionMet { get; set; }
        public bool RejectUnlock { get; set; }
        public int ConditionCheckCount { get; private set; }
        public int UnlockNotificationCount { get; private set; }

        public override bool IsConditional => ConditionalValue;

        protected override bool EvaluateCondition()
        {
            ConditionCheckCount++;
            return ConditionMet;
        }

        protected override OperationResult CanUnlock(in AchievementUnlockContext context)
        {
            OperationResult baseResult = base.CanUnlock(in context);
            if (!baseResult) return baseResult;
            if (RejectUnlock) return AchievementOperations.InvalidAchievement();
            return baseResult;
        }

        protected override void OnUnlocked()
        {
            UnlockNotificationCount++;
        }
    }

    public sealed class TestAchievementPlatform : AchievementPlatformBase
    {
        private readonly List<string> _unlockedIds = new List<string>();

        public override string PlatformName => "Test Platform";

        public int InitialiseCount { get; private set; }
        public int ShutdownCount { get; private set; }
        public IReadOnlyList<string> UnlockedIds => _unlockedIds;

        public override void Initialise()
        {
            InitialiseCount++;
        }

        public override void Shutdown()
        {
            ShutdownCount++;
        }

        public override void UnlockAchievement(string platformId)
        {
            _unlockedIds.Add(platformId);
        }

#if UNITY_EDITOR
        public override void DrawSettings(SerializedObject serializedObject)
        {
        }
#endif
    }
}
