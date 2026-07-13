using System.IO;
using NUnit.Framework;
using Systems.SimpleAchievements.Components;
using Systems.SimpleAchievements.Data.SaveFiles;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Saving.Abstract;

namespace Systems.SimpleAchievements.Tests
{
    public sealed class AchievementPersistenceTests : SimpleAchievementsTestBase
    {
        [Test]
        public void SaveToMemoryAndLoad_RoundTripsUnlockedPlatformIds()
        {
            TestAchievement first = CreateRegisteredAchievement("ACH_FIRST");
            TestAchievement second = CreateRegisteredAchievement("ACH_SECOND");
            CreateRegistry();

            AchievementUnlockContext firstContext = new AchievementUnlockContext(first);
            AchievementAPI.Unlock(in firstContext);

            SaveFileBase memorySave = AchievementAPI.SaveToMemory();

            AchievementSaveFile replacement = new AchievementSaveFile
            {
                UnlockedPlatformIds = new[] { "ACH_SECOND", "", null }
            };
            AchievementAPI.Load(replacement);

            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_FIRST"));
            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_SECOND"));

            AchievementAPI.Load(memorySave);

            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_FIRST"));
            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_SECOND"));
            Assert.AreEqual(0, second.UnlockNotificationCount);
        }

        [Test]
        public void BuildSaveFile_DeduplicatesAndStoresUnlockedIds()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_SAVE");
            AchievementRegistry registry = CreateRegistry();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            AchievementAPI.Unlock(in context);

            AchievementSaveFile file = registry.BuildSaveFile();

            Assert.AreEqual(1, file.UnlockedPlatformIds.Length);
            Assert.AreEqual("ACH_SAVE", file.UnlockedPlatformIds[0]);
        }

        [Test]
        public void ParseSaveFile_RestoresStateWithoutReplayingCallbacksOrPlatformUnlocks()
        {
            TestAchievement first = CreateRegisteredAchievement("ACH_FIRST");
            TestAchievement second = CreateRegisteredAchievement("ACH_SECOND");
            TestAchievementPlatform platform = CreateRegisteredPlatform();
            AchievementRegistry registry = CreateRegistry();

            AchievementUnlockContext firstContext = new AchievementUnlockContext(first);
            AchievementAPI.Unlock(in firstContext);

            AchievementSaveFile file = new AchievementSaveFile
            {
                UnlockedPlatformIds = new[] { "ACH_SECOND", "", null }
            };
            registry.ParseSaveFile(file);

            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_FIRST"));
            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_SECOND"));
            Assert.AreEqual(1, first.UnlockNotificationCount);
            Assert.AreEqual(0, second.UnlockNotificationCount);
            Assert.AreEqual(1, platform.UnlockedIds.Count);
        }

        [Test]
        public void ParseSaveFile_WithNullIdsTreatsSaveAsEmpty()
        {
            TestAchievement achievement = CreateRegisteredAchievement("ACH_NULL_SAVE");
            AchievementRegistry registry = CreateRegistry();
            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            AchievementAPI.Unlock(in context);

            AchievementSaveFile file = new AchievementSaveFile {UnlockedPlatformIds = null};
            registry.ParseSaveFile(file);

            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_NULL_SAVE"));
        }

        [Test]
        public void DiskSaveAndLoad_RoundTripsConfiguredSaveFile()
        {
            string fileName = CreateAchievementFileName("disk");
            ConfigureSettings(false, fileName);
            TestAchievement achievement = CreateRegisteredAchievement("ACH_DISK");
            AchievementRegistry registry = CreateRegistry();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement);
            AchievementAPI.Unlock(in context);
            AchievementAPI.Save();

            Assert.IsTrue(File.Exists(GetAchievementPath(fileName)));

            registry.ParseSaveFile(new AchievementSaveFile());
            Assert.IsFalse(AchievementAPI.IsUnlocked("ACH_DISK"));

            AchievementAPI.Load();

            Assert.IsTrue(AchievementAPI.IsUnlocked("ACH_DISK"));
        }
    }
}
