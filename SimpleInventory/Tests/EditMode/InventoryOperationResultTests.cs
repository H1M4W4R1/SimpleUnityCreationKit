using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleInventory.Operations;

namespace Systems.SimpleInventory.Tests
{
    public sealed class InventoryOperationResultTests : SimpleInventoryTestBase
    {
        [Test]
        public void InventoryFactories_UseInventorySystemCodes()
        {
            OperationResult added = InventoryOperations.ItemsAdded();
            OperationResult notEnoughSpace = InventoryOperations.NotEnoughSpace();

            Assert.IsTrue(OperationResult.IsSuccess(added));
            Assert.AreEqual(InventoryOperations.SYSTEM_INVENTORY, added.systemCode);
            Assert.AreEqual(InventoryOperations.SUCCESS_ITEMS_ADDED, added.resultCode);

            Assert.IsTrue(OperationResult.IsError(notEnoughSpace));
            Assert.IsTrue(OperationResult.IsFromSystem(notEnoughSpace, InventoryOperations.SYSTEM_INVENTORY));
            Assert.AreEqual(InventoryOperations.ERROR_NOT_ENOUGH_SPACE, notEnoughSpace.resultCode);
        }

        [Test]
        public void EquipmentFactories_UseEquipmentSystemCodes()
        {
            OperationResult equipped = EquipmentOperations.Equipped();
            OperationResult noFreeSlots = EquipmentOperations.NoFreeSlots();

            Assert.IsTrue(OperationResult.IsSuccess(equipped));
            Assert.AreEqual(EquipmentOperations.SYSTEM_EQUIPMENT, equipped.systemCode);
            Assert.AreEqual(EquipmentOperations.SUCCESS_EQUIPPED, equipped.resultCode);

            Assert.IsTrue(OperationResult.IsError(noFreeSlots));
            Assert.IsTrue(OperationResult.IsFromSystem(noFreeSlots, EquipmentOperations.SYSTEM_EQUIPMENT));
            Assert.AreEqual(EquipmentOperations.ERROR_NO_FREE_SLOTS, noFreeSlots.resultCode);
        }
    }
}
