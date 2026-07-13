using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Operations;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionLevelOperationTests : SimpleFactionsTestBase
    {
        [Test]
        public void AssignLevel_WhenNotMember_ReturnsNotAMember()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.AssignLevel<TestFaction>(level);

            AssertSimilar(FactionOperations.NotAMember(), result);
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
        }

        [Test]
        public void AssignLevel_WhenLevelDoesNotBelongToFaction_ReturnsLevelNotInFaction()
        {
            CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.AssignLevel<TestFaction>(level);

            AssertSimilar(FactionOperations.LevelNotInFaction(), result);
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
        }

        [Test]
        public void AssignLevel_WhenLevelBelongsToFaction_AssignsAndFiresCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.AssignLevel<TestFaction>(level);

            AssertSimilar(FactionOperations.LevelAssigned(), result);
            Assert.AreSame(level, membership.GetCurrentLevel<TestFaction>());
            Assert.AreSame(level, membership.GetCurrentLevel(faction));
            Assert.IsTrue(membership.IsAtLeastLevel<TestFaction>(level));
            Assert.AreEqual(1, level.ChangedCount);
            Assert.AreEqual(1, level.AchievedCount);
            Assert.AreEqual(1, level.IncreasedCount);
            Assert.AreEqual(1, faction.LevelChangedCount);
            Assert.IsNull(faction.LastPreviousLevel);
            Assert.AreSame(level, faction.LastNewLevel);
        }

        [Test]
        public void AssignLevel_WhenNull_ClearsCurrentLevelAndFiresFactionCallbackOnly()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.AssignLevel<TestFaction>(level);

            OperationResult result = membership.AssignLevel<TestFaction>(null);

            AssertSimilar(FactionOperations.LevelCleared(), result);
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(2, faction.LevelChangedCount);
            Assert.AreSame(level, faction.LastPreviousLevel);
            Assert.IsNull(faction.LastNewLevel);
            Assert.AreEqual(1, level.ChangedCount);
        }

        [Test]
        public void AssignLevel_WithInternalAction_ChangesLevelWithoutCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>(ActionSource.Internal);

            OperationResult result = membership.AssignLevel<TestFaction>(level, ActionSource.Internal);

            AssertSimilar(FactionOperations.LevelAssigned(), result);
            Assert.AreSame(level, membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(0, faction.LevelChangedCount);
            Assert.AreEqual(0, level.ChangedCount);
        }

        [Test]
        public void FactionAssignLevel_AddsOnceAndSortsByPromotionThreshold()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel highLevel =
                CreateRegisteredLevel<TestReputationLevel>(automaticPromotion: true, promotionThreshold: 100L);
            OtherTestReputationLevel lowLevel =
                CreateRegisteredLevel<OtherTestReputationLevel>(automaticPromotion: true, promotionThreshold: 10L);

            bool firstAssignmentChanged = faction.AssignLevel(highLevel);
            bool secondAssignmentChanged = faction.AssignLevel(lowLevel);
            bool duplicateAssignmentChanged = faction.AssignLevel(highLevel);

            Assert.IsTrue(firstAssignmentChanged);
            Assert.IsTrue(secondAssignmentChanged);
            Assert.IsFalse(duplicateAssignmentChanged);
            Assert.AreEqual(2, faction.Levels.Count);
            Assert.AreSame(lowLevel, faction.Levels[0]);
            Assert.AreSame(highLevel, faction.Levels[1]);
            Assert.AreEqual(1, faction.GetLevelIndex(highLevel));
            Assert.AreEqual(0, faction.GetLevelIndex(lowLevel));
        }

        [Test]
        public void ChangeReputation_WhenThresholdCrossed_AutomaticallyPromotesToBestLevel()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel lowLevel =
                CreateRegisteredLevel<TestReputationLevel>(automaticPromotion: true, promotionThreshold: 10L);
            OtherTestReputationLevel highLevel =
                CreateRegisteredLevel<OtherTestReputationLevel>(automaticPromotion: true, promotionThreshold: 100L);
            faction.AssignLevel(highLevel);
            faction.AssignLevel(lowLevel);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.ChangeReputation<TestFaction>(150L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreSame(highLevel, membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, faction.PromotionCheckCount);
            Assert.AreEqual(1, membership.PromotionCheckCount);
            Assert.AreEqual(1, highLevel.CanPromoteToCount);
            Assert.AreEqual(1, highLevel.ChangedCount);
            Assert.AreEqual(1, highLevel.AchievedCount);
            Assert.AreEqual(0, lowLevel.ChangedCount);
        }

        [Test]
        public void ChangeReputation_WhenPromotionDenied_LeavesLevelUnchangedButKeepsReputation()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level =
                CreateRegisteredLevel<TestReputationLevel>(automaticPromotion: true, promotionThreshold: 10L);
            level.RejectPromoteTo = true;
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.ChangeReputation<TestFaction>(25L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreEqual(25L, membership.GetReputation<TestFaction>());
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, level.CanPromoteToCount);
            Assert.AreEqual(0, membership.PromotionCheckCount);
            Assert.AreEqual(0, faction.LevelChangedCount);
        }

        [Test]
        public void ChangeReputation_WhenMemberPromotionDenied_LeavesLevelUnchanged()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level =
                CreateRegisteredLevel<TestReputationLevel>(automaticPromotion: true, promotionThreshold: 10L);
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.RejectPromotion = true;
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.ChangeReputation<TestFaction>(25L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, membership.PromotionCheckCount);
            Assert.AreEqual(0, faction.LevelChangedCount);
        }

        [Test]
        public void ChangeReputation_WhenReputationDropsBelowThreshold_DemotesToPreviousLevel()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel lowLevel =
                CreateRegisteredLevel<TestReputationLevel>(
                    automaticPromotion: true,
                    promotionThreshold: 10L,
                    automaticDemotion: true,
                    demotionThreshold: 10L);
            OtherTestReputationLevel highLevel =
                CreateRegisteredLevel<OtherTestReputationLevel>(
                    automaticPromotion: true,
                    promotionThreshold: 100L,
                    automaticDemotion: true,
                    demotionThreshold: 100L);
            faction.AssignLevel(highLevel);
            faction.AssignLevel(lowLevel);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.ChangeReputation<TestFaction>(150L);

            OperationResult result = membership.ChangeReputation<TestFaction>(-75L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreSame(lowLevel, membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, faction.DemotionCheckCount);
            Assert.AreEqual(1, membership.DemotionCheckCount);
            Assert.AreEqual(1, highLevel.CanDemoteFromCount);
            Assert.AreEqual(1, lowLevel.CanDemoteToCount);
            Assert.AreEqual(1, lowLevel.DecreasedCount);
            Assert.AreEqual(2, faction.LevelChangedCount);
        }

        [Test]
        public void ChangeReputation_WhenLowestLevelDropsBelowThreshold_DemotesToNoLevel()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level =
                CreateRegisteredLevel<TestReputationLevel>(
                    automaticPromotion: true,
                    promotionThreshold: 10L,
                    automaticDemotion: true,
                    demotionThreshold: 10L);
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.ChangeReputation<TestFaction>(25L);

            OperationResult result = membership.ChangeReputation<TestFaction>(-20L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, level.CanDemoteFromCount);
            Assert.AreEqual(2, faction.LevelChangedCount);
            Assert.AreSame(level, faction.LastPreviousLevel);
            Assert.IsNull(faction.LastNewLevel);
        }

        [Test]
        public void ChangeReputation_LargeLossDemotesThroughAllLevels()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel lowLevel = CreateRegisteredLevel<TestReputationLevel>(
                automaticPromotion: true, promotionThreshold: 10L,
                automaticDemotion: true, demotionThreshold: 10L);
            TestReputationLevel middleLevel = CreateRegisteredLevel<TestReputationLevel>(
                automaticPromotion: true, promotionThreshold: 50L,
                automaticDemotion: true, demotionThreshold: 50L);
            TestReputationLevel highLevel = CreateRegisteredLevel<TestReputationLevel>(
                automaticPromotion: true, promotionThreshold: 100L,
                automaticDemotion: true, demotionThreshold: 100L);
            faction.AssignLevel(lowLevel);
            faction.AssignLevel(middleLevel);
            faction.AssignLevel(highLevel);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.ChangeReputation<TestFaction>(150L);

            membership.ChangeReputation<TestFaction>(-150L);

            Assert.IsNull(membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(4, faction.LevelChangedCount);
        }

        [Test]
        public void ChangeReputation_WhenDemotionDenied_LeavesCurrentLevel()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level =
                CreateRegisteredLevel<TestReputationLevel>(
                    automaticPromotion: true,
                    promotionThreshold: 10L,
                    automaticDemotion: true,
                    demotionThreshold: 10L);
            level.RejectDemoteFrom = true;
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.ChangeReputation<TestFaction>(25L);

            OperationResult result = membership.ChangeReputation<TestFaction>(-20L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreSame(level, membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, level.CanDemoteFromCount);
            Assert.AreEqual(1, faction.LevelChangedCount);
        }

        [Test]
        public void ChangeReputation_WithInternalAction_ChangesAutomaticLevelWithoutCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level =
                CreateRegisteredLevel<TestReputationLevel>(automaticPromotion: true, promotionThreshold: 10L);
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>(ActionSource.Internal);

            OperationResult result = membership.ChangeReputation<TestFaction>(25L, ActionSource.Internal);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreSame(level, membership.GetCurrentLevel<TestFaction>());
            Assert.AreEqual(1, faction.PromotionCheckCount);
            Assert.AreEqual(1, level.CanPromoteToCount);
            Assert.AreEqual(0, faction.LevelChangedCount);
            Assert.AreEqual(0, level.ChangedCount);
        }
    }
}
