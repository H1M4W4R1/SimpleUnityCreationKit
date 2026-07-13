using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using Systems.SimpleEntities.Data.Enums;
using Systems.SimpleEntities.Operations;

namespace Systems.SimpleEntities.Tests
{
    public sealed class EntityStatusTests : SimpleEntitiesTestBase
    {
        [Test]
        public void ApplyStatus_WhenNew_ClampsToMaxStackAndFiresAppliedCallback()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(3);

            OperationResult result = entity.ApplyStatus(status, 5);

            AssertSimilar(StatusOperations.StatusApplied(), result);
            Assert.IsTrue(entity.HasStatus(status));
            Assert.AreEqual(3, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.CanApplyCount);
            Assert.AreEqual(1, status.AppliedCount);
            Assert.AreEqual(3, status.LastExpectedStackCount);
            Assert.AreEqual(3, status.LastCurrentStackCount);
        }

        [Test]
        public void ApplyStatus_WhenExisting_StacksUntilMaxThenReportsMaxReached()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(3);
            entity.ApplyStatus(status, 2);

            OperationResult stackResult = entity.ApplyStatus(status, 5);
            OperationResult maxResult = entity.ApplyStatus(status, 1);

            AssertSimilar(StatusOperations.StatusStackChanged(), stackResult);
            Assert.AreEqual(3, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.StackChangedCount);
            Assert.AreEqual(1, status.LastExpectedStackCount);
            Assert.AreEqual(3, status.LastCurrentStackCount);

            AssertSimilar(StatusOperations.MaxStackReached(), maxResult);
            Assert.AreEqual(3, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.ApplyFailedCount);
        }

        [Test]
        public void ApplyStatus_WithIgnoreStackLimit_AllowsStacksAboveMax()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(2);

            OperationResult applied = entity.ApplyStatus(
                status,
                5,
                StatusModificationFlags.IgnoreStackLimit);
            OperationResult stacked = entity.ApplyStatus(
                status,
                4,
                StatusModificationFlags.IgnoreStackLimit);

            AssertSimilar(StatusOperations.StatusApplied(), applied);
            AssertSimilar(StatusOperations.StatusStackChanged(), stacked);
            Assert.AreEqual(9, entity.GetStatusStackCount(status));
            Assert.AreEqual(4, status.LastExpectedStackCount);
            Assert.AreEqual(9, status.LastCurrentStackCount);
        }

        [Test]
        public void ApplyStatus_WithRejectedStatus_FailsUnlessConditionsAreIgnored()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(3);
            status.RejectApply = true;

            OperationResult rejected = entity.ApplyStatus(status, 2);

            AssertSimilar(StatusOperations.InvalidStatus(), rejected);
            Assert.AreEqual(0, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.ApplyFailedCount);

            OperationResult ignored = entity.ApplyStatus(status, 2, StatusModificationFlags.IgnoreConditions);

            AssertSimilar(StatusOperations.StatusApplied(), ignored);
            Assert.AreEqual(2, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.AppliedCount);
        }

        [Test]
        public void ApplyStatus_WithInternalSource_SuppressesCallbacks()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(3);

            OperationResult result = entity.ApplyStatus(status, 2, actionSource: ActionSource.Internal);

            AssertSimilar(StatusOperations.StatusApplied(), result);
            Assert.AreEqual(2, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.CanApplyCount);
            Assert.AreEqual(0, status.AppliedCount);
        }

        [Test]
        public void RemoveStatus_ReducesStacksThenRemovesWhenZero()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(5);
            entity.ApplyStatus(status, 4);

            OperationResult reduceResult = entity.RemoveStatus(status, 2);

            AssertSimilar(StatusOperations.StatusStackChanged(), reduceResult);
            Assert.AreEqual(2, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.StackChangedCount);
            Assert.AreEqual(-2, status.LastExpectedStackCount);
            Assert.AreEqual(2, status.LastCurrentStackCount);

            OperationResult removeResult = entity.RemoveStatus(status, 2);

            AssertSimilar(StatusOperations.StatusRemoved(), removeResult);
            Assert.IsFalse(entity.HasStatus(status));
            Assert.AreEqual(0, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.RemovedCount);
            Assert.AreEqual(0, status.LastExpectedStackCount);
        }

        [Test]
        public void RemoveStatus_WhenNotAppliedOrNotEnoughStacks_FailsWithoutMutation()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(5);

            OperationResult notApplied = entity.RemoveStatus(status, 1);
            entity.ApplyStatus(status, 2);
            OperationResult notEnough = entity.RemoveStatus(status, 3);

            AssertSimilar(StatusOperations.NotApplied(), notApplied);
            AssertSimilar(StatusOperations.NotEnoughStacks(), notEnough);
            Assert.AreEqual(2, entity.GetStatusStackCount(status));
            Assert.AreEqual(2, status.RemoveFailedCount);
        }

        [Test]
        public void StatusOperations_WithNonPositiveStackCountFailWithoutMutation()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(5);

            OperationResult applyResult = entity.ApplyStatus(status, 0);
            OperationResult removeResult = entity.RemoveStatus(status, -1);

            AssertSimilar(StatusOperations.InvalidStackCount(), applyResult);
            AssertSimilar(StatusOperations.InvalidStackCount(), removeResult);
            Assert.IsFalse(entity.HasStatus(status));
            Assert.AreEqual(0, status.CanApplyCount);
            Assert.AreEqual(0, status.CanRemoveCount);
        }

        [Test]
        public void RemoveStatus_WithIgnoreStackLimit_ClampsRemovalToAvailableStacks()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(5);
            entity.ApplyStatus(status, 2);

            OperationResult result = entity.RemoveStatus(status, 5, StatusModificationFlags.IgnoreStackLimit);

            AssertSimilar(StatusOperations.StatusRemoved(), result);
            Assert.IsFalse(entity.HasStatus(status));
            Assert.AreEqual(1, status.RemovedCount);
            Assert.AreEqual(0, status.LastExpectedStackCount);
        }

        [Test]
        public void RemoveStatus_WithRejectedStatus_FailsUnlessConditionsAreIgnored()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(5);
            entity.ApplyStatus(status, 2);
            status.RejectRemove = true;

            OperationResult rejected = entity.RemoveStatus(status, 1);

            AssertSimilar(StatusOperations.InvalidStatus(), rejected);
            Assert.AreEqual(2, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.RemoveFailedCount);

            OperationResult ignored = entity.RemoveStatus(status, 1, StatusModificationFlags.IgnoreConditions);

            AssertSimilar(StatusOperations.StatusStackChanged(), ignored);
            Assert.AreEqual(1, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.StackChangedCount);
        }

        [Test]
        public void RemoveStatus_WithInternalSource_SuppressesCallbacks()
        {
            TestEntity entity = CreateEntity();
            TestStatus status = CreateStatus<TestStatus>(3);
            entity.ApplyStatus(status, 2, actionSource: ActionSource.Internal);

            OperationResult result = entity.RemoveStatus(status, 1, actionSource: ActionSource.Internal);

            AssertSimilar(StatusOperations.StatusStackChanged(), result);
            Assert.AreEqual(1, entity.GetStatusStackCount(status));
            Assert.AreEqual(1, status.CanRemoveCount);
            Assert.AreEqual(0, status.StackChangedCount);
            Assert.AreEqual(0, status.RemovedCount);
        }

        [Test]
        public void Tick_InvokesStatusTicksAndAllowsCurrentStatusToRemoveItself()
        {
            TestEntity entity = CreateEntity();
            TestStatus removingStatus = CreateStatus<TestStatus>(5);
            TestOtherStatus stableStatus = CreateStatus<TestOtherStatus>(5);
            removingStatus.RemoveSelfOnTick = true;
            entity.ApplyStatus(removingStatus, 2);
            entity.ApplyStatus(stableStatus, 1);

            entity.TickForTests(0.25f);

            Assert.AreEqual(1, entity.TickCount);
            Assert.AreEqual(1, removingStatus.TickCount);
            Assert.AreEqual(0.25f, removingStatus.LastDeltaTime);
            Assert.AreEqual(1, stableStatus.TickCount);
            Assert.IsFalse(entity.HasStatus(removingStatus));
            Assert.IsTrue(entity.HasStatus(stableStatus));
        }
    }
}
