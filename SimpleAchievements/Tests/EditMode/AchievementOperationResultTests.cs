using NUnit.Framework;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleCore.Operations;

namespace Systems.SimpleAchievements.Tests
{
    public sealed class AchievementOperationResultTests : SimpleAchievementsTestBase
    {
        [Test]
        public void Factories_ReturnExpectedSystemAndResultCodes()
        {
            Assert.AreEqual(
                AchievementOperations.SYSTEM_ACHIEVEMENTS,
                AchievementOperations.Permitted().systemCode);
            Assert.AreEqual(
                OperationResult.SUCCESS_PERMITTED,
                AchievementOperations.Permitted().resultCode);
            Assert.AreEqual(
                AchievementOperations.SUCCESS_UNLOCKED,
                AchievementOperations.Unlocked().resultCode);
            Assert.AreEqual(
                AchievementOperations.SUCCESS_PROGRESS_UPDATED,
                AchievementOperations.ProgressUpdated().resultCode);
            Assert.AreEqual(
                AchievementOperations.ERROR_ALREADY_UNLOCKED,
                AchievementOperations.AlreadyUnlocked().resultCode);
            Assert.AreEqual(
                AchievementOperations.ERROR_INVALID,
                AchievementOperations.InvalidAchievement().resultCode);
            Assert.AreEqual(
                AchievementOperations.ERROR_CONDITION_NOT_MET,
                AchievementOperations.ConditionNotMet().resultCode);
            Assert.AreEqual(
                AchievementOperations.ERROR_NOT_PROGRESSIBLE,
                AchievementOperations.NotProgressible().resultCode);
            Assert.IsFalse(AchievementOperations.ConditionNotMet());
            Assert.IsFalse(AchievementOperations.NotProgressible());
        }
    }
}
