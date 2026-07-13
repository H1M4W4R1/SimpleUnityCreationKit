using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Operations;
using Systems.SimpleFactions.Utility;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionMembershipOperationTests : SimpleFactionsTestBase
    {
        [Test]
        public void JoinFaction_WhenFactionMissing_ReturnsFactionNotFound()
        {
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.FactionNotFound(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
        }

        [Test]
        public void JoinFaction_WhenPermitted_TracksMembershipAndFiresCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.Joined(), result);
            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.IsTrue(membership.IsMember(faction));
            Assert.AreEqual(1, faction.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinedCount);
            Assert.AreEqual(1, faction.JoinedCount);
            Assert.AreSame(faction, membership.LastFaction);
            Assert.AreSame(membership.GetComponent<TestFactionHolder>(), membership.LastMember);
        }

        [Test]
        public void JoinFaction_WhenAlreadyMember_DoesNotFireFailureCallbacks()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();

            membership.JoinFaction<TestFaction>();
            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.AlreadyMember(), result);
            Assert.AreEqual(1, membership.JoinedCount);
            Assert.AreEqual(0, membership.JoinFailedCount);
        }

        [Test]
        public void JoinFaction_WhenFactionRejects_ShortCircuitsMemberCheck()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            faction.RejectJoin = true;
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(1, faction.JoinCheckCount);
            Assert.AreEqual(0, membership.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinFailedCount);
            Assert.AreEqual(1, faction.JoinFailedCount);
        }

        [Test]
        public void JoinFaction_WhenMemberRejects_ReturnsDeniedAndFiresFailureCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.RejectJoin = true;

            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(1, faction.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinFailedCount);
            Assert.AreEqual(1, faction.JoinFailedCount);
        }

        [Test]
        public void LeaveFaction_WhenNotMember_ReturnsNotAMember()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.LeaveFaction<TestFaction>();

            AssertSimilar(FactionOperations.NotAMember(), result);
            Assert.AreEqual(0, membership.LeaveFailedCount);
        }

        [Test]
        public void LeaveFaction_WhenPermitted_ClearsMembershipAndFiresCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.LeaveFaction<TestFaction>();

            AssertSimilar(FactionOperations.Left(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.IsFalse(membership.IsMember(faction));
            Assert.AreEqual(1, faction.LeaveCheckCount);
            Assert.AreEqual(1, membership.LeaveCheckCount);
            Assert.AreEqual(1, membership.LeftCount);
            Assert.AreEqual(1, faction.LeftCount);
        }

        [Test]
        public void LeaveFaction_WhenFactionRejects_DoesNotClearMembership()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            faction.RejectLeave = true;

            OperationResult result = membership.LeaveFaction<TestFaction>();

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(1, membership.LeaveFailedCount);
            Assert.AreEqual(1, faction.LeaveFailedCount);
        }

        [Test]
        public void LeaveFaction_WhenMemberRejects_DoesNotClearMembership()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.RejectLeave = true;

            OperationResult result = membership.LeaveFaction<TestFaction>();

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(1, membership.LeaveFailedCount);
        }

        [Test]
        public void ActionSourceInternal_SuppressesCallbacksButStillMutatesState()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();

            OperationResult joinResult = membership.JoinFaction<TestFaction>(ActionSource.Internal);
            OperationResult leaveResult = membership.LeaveFaction<TestFaction>(ActionSource.Internal);

            AssertSimilar(FactionOperations.Joined(), joinResult);
            AssertSimilar(FactionOperations.Left(), leaveResult);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(0, membership.JoinedCount);
            Assert.AreEqual(0, faction.JoinedCount);
            Assert.AreEqual(0, membership.LeftCount);
            Assert.AreEqual(0, faction.LeftCount);
        }

        [Test]
        public void ChangeReputation_WhenZero_ReturnsInvalidReputation()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.ChangeReputation<TestFaction>(0L);

            AssertSimilar(FactionOperations.InvalidReputation(), result);
            Assert.AreEqual(0L, membership.GetReputation<TestFaction>());
        }

        [Test]
        public void ChangeReputation_WhenNotMember_ReturnsNotAMember()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.ChangeReputation<TestFaction>(10L);

            AssertSimilar(FactionOperations.NotAMember(), result);
        }

        [Test]
        public void ChangeReputation_WhenPermitted_UpdatesReputationAndFiresCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = FactionAPI.ChangeReputation<TestFaction, TestFactionHolder>(membership, 25L);

            AssertSimilar(FactionOperations.ReputationChanged(), result);
            Assert.AreEqual(25L, membership.GetReputation<TestFaction>());
            Assert.AreEqual(25L, membership.GetReputation(faction));
            Assert.AreEqual(1, faction.ReputationCheckCount);
            Assert.AreEqual(1, membership.ReputationCheckCount);
            Assert.AreEqual(1, membership.ReputationChangedCount);
            Assert.AreEqual(1, faction.ReputationChangedCount);
            Assert.AreEqual(0L, membership.LastPreviousReputation);
            Assert.AreEqual(25L, membership.LastAmountRequested);
        }

        [Test]
        public void ChangeReputation_WhenFactionRejects_PreservesReputation()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            faction.RejectReputationChange = true;
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = membership.ChangeReputation<TestFaction>(10L);

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.AreEqual(0L, membership.GetReputation<TestFaction>());
            Assert.AreEqual(1, membership.ReputationFailedCount);
            Assert.AreEqual(1, faction.ReputationFailedCount);
            Assert.AreEqual(0, membership.ReputationCheckCount);
        }

        [Test]
        public void ChangeReputation_WhenMemberRejects_PreservesReputation()
        {
            CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();
            membership.RejectReputationChange = true;

            OperationResult result = membership.ChangeReputation<TestFaction>(10L);

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.AreEqual(0L, membership.GetReputation<TestFaction>());
            Assert.AreEqual(1, membership.ReputationFailedCount);
            Assert.AreEqual(1, membership.ReputationCheckCount);
        }

        [Test]
        public void MultipleFactions_KeepMembershipAndReputationStateIndependent()
        {
            CreateRegisteredFaction<TestFaction>();
            CreateRegisteredFaction<OtherTestFaction>();
            TestFactionMembership membership = CreateMembership();

            membership.JoinFaction<TestFaction>();
            membership.ChangeReputation<TestFaction>(40L);

            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.IsFalse(membership.IsMemberOf<OtherTestFaction>());
            Assert.AreEqual(40L, membership.GetReputation<TestFaction>());
            Assert.AreEqual(0L, membership.GetReputation<OtherTestFaction>());
        }

        [Test]
        public void FactionAPI_ForwardsJoinLeaveAndLevelQueries()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestReputationLevel level = CreateRegisteredLevel<TestReputationLevel>();
            faction.AssignLevel(level);
            TestFactionMembership membership = CreateMembership();

            OperationResult joinResult = FactionAPI.Join<TestFaction, TestFactionHolder>(membership);
            OperationResult assignResult = FactionAPI.AssignLevel<TestFaction, TestFactionHolder>(membership, level);
            ReputationLevelBase currentLevel = FactionAPI.GetLevel<TestFaction, TestFactionHolder>(membership);
            bool isAtLeastLevel = FactionAPI.IsAtLeastLevel<TestFaction, TestFactionHolder>(membership, level);
            OperationResult leaveResult = FactionAPI.Leave<TestFaction, TestFactionHolder>(membership);

            AssertSimilar(FactionOperations.Joined(), joinResult);
            AssertSimilar(FactionOperations.LevelAssigned(), assignResult);
            Assert.AreSame(level, currentLevel);
            Assert.IsTrue(isAtLeastLevel);
            AssertSimilar(FactionOperations.Left(), leaveResult);
        }
    }
}
