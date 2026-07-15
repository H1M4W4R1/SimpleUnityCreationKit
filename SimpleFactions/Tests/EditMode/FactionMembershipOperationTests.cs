using NUnit.Framework;
using Systems.SimpleCore.Operations;
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

            OperationResult result = FactionAPI.Join<TestFaction, TestFactionHolder>(membership);

            AssertSimilar(FactionOperations.Joined(), result);
            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.IsTrue(membership.IsMember(faction));
            Assert.AreEqual(1, faction.JoinCheckCount);
            Assert.AreEqual(1, membership.JoinCheckCount);
            Assert.AreEqual(1, faction.JoinedCount);
            Assert.AreEqual(1, membership.JoinedCount);
            Assert.AreSame(faction, membership.LastFaction);
        }

        [Test]
        public void JoinFaction_WhenRejected_FiresFailureCallbacksWithoutChangingState()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            faction.RejectJoin = true;
            TestFactionMembership membership = CreateMembership();

            OperationResult result = membership.JoinFaction<TestFaction>();

            AssertSimilar(FactionOperations.Denied(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.AreEqual(1, faction.JoinFailedCount);
            Assert.AreEqual(1, membership.JoinFailedCount);
            Assert.AreEqual(0, membership.JoinCheckCount);
        }

        [Test]
        public void LeaveFaction_WhenPermitted_ClearsMembershipAndFiresCallbacks()
        {
            TestFaction faction = CreateRegisteredFaction<TestFaction>();
            TestFactionMembership membership = CreateMembership();
            membership.JoinFaction<TestFaction>();

            OperationResult result = FactionAPI.Leave<TestFaction, TestFactionHolder>(membership);

            AssertSimilar(FactionOperations.Left(), result);
            Assert.IsFalse(membership.IsMemberOf<TestFaction>());
            Assert.IsFalse(membership.IsMember(faction));
            Assert.AreEqual(1, faction.LeftCount);
            Assert.AreEqual(1, membership.LeftCount);
        }

        [Test]
        public void MembershipState_IsIndependentForEachFaction()
        {
            CreateRegisteredFaction<TestFaction>();
            CreateRegisteredFaction<OtherTestFaction>();
            TestFactionMembership membership = CreateMembership();

            membership.JoinFaction<TestFaction>();

            Assert.IsTrue(membership.IsMemberOf<TestFaction>());
            Assert.IsFalse(membership.IsMemberOf<OtherTestFaction>());
        }
    }
}
