using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Operations;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionOperationResultTests
    {
        [Test]
        public void SuccessFactories_UseFactionSystemAndExpectedCodes()
        {
            AssertSuccess(FactionOperations.Permitted(), OperationResult.SUCCESS_PERMITTED);
            AssertSuccess(FactionOperations.Joined(), FactionOperations.SUCCESS_JOINED);
            AssertSuccess(FactionOperations.Left(), FactionOperations.SUCCESS_LEFT);
            AssertSuccess(FactionOperations.ReputationChanged(), FactionOperations.SUCCESS_REPUTATION_CHANGED);
            AssertSuccess(FactionOperations.LevelAssigned(), FactionOperations.SUCCESS_LEVEL_ASSIGNED);
            AssertSuccess(FactionOperations.LevelCleared(), FactionOperations.SUCCESS_LEVEL_CLEARED);
        }

        [Test]
        public void ErrorFactories_UseFactionSystemAndExpectedCodes()
        {
            AssertError(FactionOperations.Denied(), OperationResult.ERROR_DENIED);
            AssertError(FactionOperations.FactionNotFound(), FactionOperations.ERROR_FACTION_NOT_FOUND);
            AssertError(FactionOperations.AlreadyMember(), FactionOperations.ERROR_ALREADY_MEMBER);
            AssertError(FactionOperations.NotAMember(), FactionOperations.ERROR_NOT_A_MEMBER);
            AssertError(FactionOperations.InvalidReputation(), FactionOperations.ERROR_INVALID_REPUTATION);
            AssertError(FactionOperations.LevelNotInFaction(), FactionOperations.ERROR_LEVEL_NOT_IN_FACTION);
            AssertError(FactionOperations.PromotionDenied(), FactionOperations.ERROR_PROMOTION_DENIED);
            AssertError(FactionOperations.DemotionDenied(), FactionOperations.ERROR_DEMOTION_DENIED);
        }

        private static void AssertSuccess(OperationResult result, ushort expectedResultCode)
        {
            Assert.IsTrue(OperationResult.IsSuccess(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, FactionOperations.SYSTEM_FACTION));
            Assert.AreEqual(FactionOperations.SYSTEM_FACTION, result.systemCode);
            Assert.AreEqual(expectedResultCode, result.resultCode);
        }

        private static void AssertError(OperationResult result, ushort expectedResultCode)
        {
            Assert.IsTrue(OperationResult.IsError(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, FactionOperations.SYSTEM_FACTION));
            Assert.AreEqual((ushort)(FactionOperations.SYSTEM_FACTION | (1 << 15)), result.systemCode);
            Assert.AreEqual(expectedResultCode, result.resultCode);
        }
    }
}
