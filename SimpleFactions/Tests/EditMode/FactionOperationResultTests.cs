using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleFactions.Operations;

namespace Systems.SimpleFactions.Tests
{
    public sealed class FactionOperationResultTests
    {
        [Test]
        public void Factories_UseFactionSystemAndMembershipCodes()
        {
            AssertSuccess(FactionOperations.Permitted(), OperationResult.SUCCESS_PERMITTED);
            AssertSuccess(FactionOperations.Joined(), FactionOperations.SUCCESS_JOINED);
            AssertSuccess(FactionOperations.Left(), FactionOperations.SUCCESS_LEFT);
            AssertError(FactionOperations.Denied(), OperationResult.ERROR_DENIED);
            AssertError(FactionOperations.FactionNotFound(), FactionOperations.ERROR_FACTION_NOT_FOUND);
            AssertError(FactionOperations.AlreadyMember(), FactionOperations.ERROR_ALREADY_MEMBER);
            AssertError(FactionOperations.NotAMember(), FactionOperations.ERROR_NOT_A_MEMBER);
        }

        private static void AssertSuccess(OperationResult result, ushort expectedResultCode)
        {
            Assert.IsTrue(OperationResult.IsSuccess(result));
            Assert.AreEqual(FactionOperations.SYSTEM_FACTION, result.systemCode);
            Assert.AreEqual(expectedResultCode, result.resultCode);
        }

        private static void AssertError(OperationResult result, ushort expectedResultCode)
        {
            Assert.IsTrue(OperationResult.IsError(result));
            Assert.AreEqual(expectedResultCode, result.resultCode);
        }
    }
}
