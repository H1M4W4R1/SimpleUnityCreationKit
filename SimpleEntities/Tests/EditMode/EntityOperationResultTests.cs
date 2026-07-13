using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleEntities.Operations;

namespace Systems.SimpleEntities.Tests
{
    public sealed class EntityOperationResultTests
    {
        [Test]
        public void EntityOperations_ReturnExpectedSystemsAndResultCodes()
        {
            OperationResult permitted = EntityOperations.Permitted();
            OperationResult denied = EntityOperations.NotPermitted();
            OperationResult damaged = EntityOperations.Damaged();
            OperationResult healed = EntityOperations.Healed();
            OperationResult killed = EntityOperations.Killed();
            OperationResult saved = EntityOperations.SavedFromDeath();

            Assert.IsTrue(OperationResult.IsSuccess(permitted));
            Assert.IsTrue(OperationResult.IsError(denied));
            Assert.IsTrue(OperationResult.IsFromSystem(damaged, EntityOperations.SYSTEM_ENTITY));
            Assert.AreEqual(EntityOperations.SUCCESS_ENTITY_DAMAGED, damaged.resultCode);
            Assert.AreEqual(EntityOperations.SUCCESS_ENTITY_HEALED, healed.resultCode);
            Assert.AreEqual(EntityOperations.SUCCESS_ENTITY_KILLED, killed.resultCode);
            Assert.AreEqual(EntityOperations.SUCCESS_ENTITY_SAVED_FROM_DEATH, saved.resultCode);
            Assert.IsFalse(OperationResult.AreSimilar(damaged, saved));
        }

        [Test]
        public void StatusOperations_ReturnExpectedSystemsAndResultCodes()
        {
            OperationResult applied = StatusOperations.StatusApplied();
            OperationResult removed = StatusOperations.StatusRemoved();
            OperationResult changed = StatusOperations.StatusStackChanged();
            OperationResult invalid = StatusOperations.InvalidStatus();
            OperationResult maxStack = StatusOperations.MaxStackReached();
            OperationResult notEnough = StatusOperations.NotEnoughStacks();
            OperationResult notApplied = StatusOperations.NotApplied();

            Assert.IsTrue(OperationResult.IsSuccess(applied));
            Assert.IsTrue(OperationResult.IsSuccess(removed));
            Assert.IsTrue(OperationResult.IsSuccess(changed));
            Assert.IsTrue(OperationResult.IsError(invalid));
            Assert.IsTrue(OperationResult.IsError(maxStack));
            Assert.IsTrue(OperationResult.IsError(notEnough));
            Assert.IsTrue(OperationResult.IsError(notApplied));
            Assert.IsTrue(OperationResult.IsFromSystem(applied, StatusOperations.SYSTEM_STATUS));
            Assert.AreEqual(StatusOperations.SUCCESS_STATUS_APPLIED, applied.resultCode);
            Assert.AreEqual(StatusOperations.SUCCESS_STATUS_REMOVED, removed.resultCode);
            Assert.AreEqual(StatusOperations.SUCCESS_STATUS_STACK_CHANGED, changed.resultCode);
            Assert.AreEqual(StatusOperations.ERROR_INVALID_STATUS, invalid.resultCode);
        }
    }
}
