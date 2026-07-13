using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleProgression.Components;
using Systems.SimpleProgression.Operations;
using Systems.SimpleProgression.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleProgression.Tests
{
    public sealed class ProgressionControllerTests
    {
        private GameObject _gameObject;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Progression Test Object");
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject) Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void ExperienceController_AddTakeAndHasExperience()
        {
            TestExperienceController controller = _gameObject.AddComponent<TestExperienceController>();

            OperationResult addResult = controller.AddExperience(25);
            OperationResult takeResult = controller.TakeExperience(5);

            Assert.IsTrue(addResult);
            Assert.IsTrue(takeResult);
            Assert.AreEqual(20UL, controller.Experience);
            Assert.IsTrue(controller.HasExperience(20));
            Assert.IsFalse(controller.HasExperience(21));
            Assert.AreEqual(2, controller.ChangedCount);
            Assert.AreEqual(1, controller.AddedCount);
            Assert.AreEqual(1, controller.TakenCount);
        }

        [Test]
        public void ExperienceController_RejectsInvalidAmountsAndInsufficientExperience()
        {
            TestExperienceController controller = _gameObject.AddComponent<TestExperienceController>();

            OperationResult zeroAddResult = controller.AddExperience(0);
            OperationResult zeroTakeResult = controller.TakeExperience(0);
            OperationResult takeResult = controller.TakeExperience(1);

            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.InvalidExperienceAmount(), zeroAddResult));
            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.InvalidExperienceAmount(), zeroTakeResult));
            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.NotEnoughExperience(), takeResult));
            Assert.AreEqual(0UL, controller.Experience);
        }

        [Test]
        public void LevelController_DerivesLevelsAndCallsCallbacks()
        {
            TestLevelController controller = _gameObject.AddComponent<TestLevelController>();

            OperationResult result = controller.AddExperience(25);

            Assert.IsTrue(result);
            Assert.AreEqual(2, controller.GetCurrentLevel());
            Assert.AreEqual(2, controller.LevelIncreasedCount);
            Assert.AreEqual(1, controller.LevelChangedCount);
            Assert.AreEqual(2, controller.LastLevel);
        }

        [Test]
        public void LevelController_IncreaseLevelUsesExperienceCurveAndHonoursMaximum()
        {
            TestLevelController controller = _gameObject.AddComponent<TestLevelController>();

            OperationResult increaseResult = controller.IncreaseLevel(2);
            OperationResult blockedResult = controller.IncreaseLevel(1);

            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.LevelIncreased(), increaseResult));
            Assert.AreEqual(20UL, controller.Experience);
            Assert.AreEqual(2, controller.GetCurrentLevel());
            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.MaxLevelReached(), blockedResult));
        }

        [Test]
        public void ProgressionAPI_UsesControllersOnGameObject()
        {
            TestLevelController controller = _gameObject.AddComponent<TestLevelController>();

            OperationResult experienceResult = ProgressionAPI.IncreaseExperience(_gameObject, 10);
            OperationResult levelResult = ProgressionAPI.IncreaseLevel(_gameObject, 1);

            Assert.IsTrue(experienceResult);
            Assert.IsTrue(levelResult);
            Assert.AreEqual(20UL, controller.Experience);
            Assert.AreEqual(2, controller.GetCurrentLevel());
        }

        [Test]
        public void ProgressionAPI_ReturnsErrorsForMissingControllersAndObjects()
        {
            GameObject emptyObject = new GameObject("Empty Progression Object");

            OperationResult missingExperience = ProgressionAPI.AddExperience(emptyObject, 1);
            OperationResult missingLevel = ProgressionAPI.IncreaseLevel(emptyObject, 1);
            OperationResult missingObject = ProgressionAPI.AddExperience(null, 1);

            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.ExperienceControllerNotFound(), missingExperience));
            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.LevelControllerNotFound(), missingLevel));
            Assert.IsTrue(OperationResult.AreSimilar(
                ProgressionOperations.InvalidGameObject(), missingObject));

            Object.DestroyImmediate(emptyObject);
        }
    }

    public sealed class TestExperienceController : ExperienceControllerBase
    {
        public int ChangedCount { get; private set; }
        public int AddedCount { get; private set; }
        public int TakenCount { get; private set; }

        protected override void OnExperienceChanged(ulong previousExperience, ulong newExperience)
        {
            ChangedCount++;
        }

        protected override void OnExperienceAdded(ulong experienceAmount)
        {
            AddedCount++;
        }

        protected override void OnExperienceTaken(ulong experienceAmount)
        {
            TakenCount++;
        }
    }

    public sealed class TestLevelController : LevelControllerBase
    {
        public int LevelIncreasedCount { get; private set; }
        public int LevelChangedCount { get; private set; }
        public int LastLevel { get; private set; }

        public override int GetMaxLevel()
        {
            return 2;
        }

        protected override ulong GetExperienceForLevel(int level)
        {
            return (ulong)(level * 10);
        }

        protected override void OnLevelIncreased(int newLevel)
        {
            LevelIncreasedCount++;
            LastLevel = newLevel;
        }

        protected override void OnLevelChanged(int previousLevel, int newLevel)
        {
            LevelChangedCount++;
        }
    }
}
