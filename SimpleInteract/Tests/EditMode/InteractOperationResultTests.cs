using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleInteract.Operations;

namespace Systems.SimpleInteract.Tests
{
    public sealed class InteractOperationResultTests : SimpleInteractTestBase
    {
        [Test]
        public void SuccessFactories_ReturnInteractionSuccesses()
        {
            AssertSuccess(InteractOperations.Permitted(), OperationResult.SUCCESS_PERMITTED);
            AssertSuccess(InteractOperations.Interacted(), InteractOperations.SUCCESS_INTERACTED);
        }

        [Test]
        public void ErrorFactories_ReturnInteractionErrors()
        {
            AssertError(InteractOperations.Denied(), OperationResult.ERROR_DENIED);
            AssertError(InteractOperations.NoObjectsInRange(), InteractOperations.ERROR_NO_OBJECTS_IN_RANGE);
        }

        private static void AssertSuccess(OperationResult result, ushort expectedCode)
        {
            Assert.IsTrue(result);
            Assert.IsTrue(OperationResult.IsSuccess(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, InteractOperations.SYSTEM_INTERACTION));
            Assert.AreEqual(expectedCode, result.resultCode);
        }

        private static void AssertError(OperationResult result, ushort expectedCode)
        {
            Assert.IsFalse(result);
            Assert.IsTrue(OperationResult.IsError(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, InteractOperations.SYSTEM_INTERACTION));
            Assert.AreEqual(expectedCode, result.resultCode);
        }
    }
}
