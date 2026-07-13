using NUnit.Framework;
using Systems.SimpleAchievements.Components;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Operations;
using UnityEngine;

namespace Systems.SimpleAchievements.Tests
{
    public sealed class AchievementRegistryTests : SimpleAchievementsTestBase
    {
        [Test]
        public void Registry_WhenCreated_InitialisesRegisteredPlatforms()
        {
            TestAchievementPlatform platform = CreateRegisteredPlatform();

            AchievementRegistry registry = CreateRegistry();

            Assert.IsTrue(registry);
            Assert.AreEqual(1, platform.InitialiseCount);

            registry.ShutdownForTests();
            Assert.AreEqual(1, platform.ShutdownCount);
            Object.DestroyImmediate(registry.gameObject);
        }

        [Test]
        public void Unlock_WithManualAchievement_RecordsUnlockNotifiesAchievementAndPlatform()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_MANUAL");
            TestAchievementPlatform platform = CreateRegisteredPlatform();
            CreateRegistry();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            OperationResult result = AchievementAPI.Unlock(in context);

            AssertSimilar(AchievementOperations.Unlocked(), result);
            Assert.IsTrue(AchievementAPI.IsUnlocked(achievement));
            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_MANUAL"));
            Assert.AreEqual(1, achievement.UnlockNotificationCount);
            Assert.AreEqual(1, platform.UnlockedIds.Count);
            Assert.AreEqual("ACH_MANUAL", platform.UnlockedIds[0]);
        }

        [Test]
        public void Unlock_WhenAlreadyUnlocked_ReturnsAlreadyUnlockedWithoutDuplicateNotifications()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_DUPLICATE");
            TestAchievementPlatform platform = CreateRegisteredPlatform();
            CreateRegistry();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            OperationResult firstResult = AchievementAPI.Unlock(in context);
            OperationResult secondResult = AchievementAPI.Unlock(in context);

            AssertSimilar(AchievementOperations.Unlocked(), firstResult);
            AssertSimilar(AchievementOperations.AlreadyUnlocked(), secondResult);
            Assert.AreEqual(1, achievement.UnlockNotificationCount);
            Assert.AreEqual(1, platform.UnlockedIds.Count);
        }

        [Test]
        public void Unlock_WhenAchievementIsInvalid_ReturnsInvalidAchievement()
        {
            CreateRegistry();

            AchievementUnlockContext nullContext = new AchievementUnlockContext(null);
            OperationResult nullResult = AchievementAPI.Unlock(in nullContext);

            TestAchievement emptyId = CreateAchievement(string.Empty);
            AchievementUnlockContext emptyIdContext = new AchievementUnlockContext(emptyId);
            OperationResult emptyIdResult = AchievementAPI.Unlock(in emptyIdContext);

            AssertSimilar(AchievementOperations.InvalidAchievement(), nullResult);
            AssertSimilar(AchievementOperations.InvalidAchievement(), emptyIdResult);
            Assert.IsFalse(AchievementAPI.IsUnlocked(emptyId));
            Assert.IsFalse(AchievementAPI.IsUnlocked((string)null));
        }

        [Test]
        public void Unlock_WhenConditionalConditionIsNotMet_FailsUnlessForced()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_CONDITIONAL", true, false);
            CreateRegistry();

            AchievementUnlockContext normalContext = new AchievementUnlockContext(achievement);
            OperationResult normalResult = AchievementAPI.Unlock(in normalContext);

            AchievementUnlockContext forcedContext = new AchievementUnlockContext(achievement, true);
            OperationResult forcedResult = AchievementAPI.Unlock(in forcedContext);

            AssertSimilar(AchievementOperations.ConditionNotMet(), normalResult);
            AssertSimilar(AchievementOperations.Unlocked(), forcedResult);
            Assert.AreEqual(1, achievement.UnlockNotificationCount);
            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_CONDITIONAL"));
        }

        [Test]
        public void Unlock_WhenCustomValidationRejects_ReturnsValidationResult()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_REJECTED");
            achievement.RejectUnlock = true;
            CreateRegistry();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            OperationResult result = AchievementAPI.Unlock(in context);

            AssertSimilar(AchievementOperations.InvalidAchievement(), result);
            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_REJECTED"));
            Assert.AreEqual(0, achievement.UnlockNotificationCount);
        }

        [Test]
        public void TickForTests_WhenConditionalAchievementBecomesReady_UnlocksIt()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_AUTO", true, false);
            TestAchievementPlatform platform = CreateRegisteredPlatform();
            AchievementRegistry registry = CreateRegistry();

            registry.TickForTests(0.1f);
            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_AUTO"));
            Assert.AreEqual(1, achievement.ConditionCheckCount);

            achievement.ConditionMet = true;
            registry.TickForTests(0.1f);
            registry.TickForTests(0.1f);

            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_AUTO"));
            Assert.AreEqual(1, achievement.UnlockNotificationCount);
            Assert.AreEqual(1, platform.UnlockedIds.Count);
            Assert.AreEqual(2, achievement.ConditionCheckCount);
        }
    }
}
