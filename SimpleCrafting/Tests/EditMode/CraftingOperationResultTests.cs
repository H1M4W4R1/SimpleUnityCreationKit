using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Operations;

namespace Systems.SimpleCrafting.Tests
{
    public sealed class CraftingOperationResultTests : SimpleCraftingTestBase
    {
        [Test]
        public void CraftingFactories_UseCraftingSystemCodes()
        {
            OperationResult completed = CraftingOperations.Completed();
            OperationResult stationMissing = CraftingOperations.StationMissing();

            Assert.IsTrue(OperationResult.IsSuccess(completed));
            Assert.AreEqual(CraftingOperations.SYSTEM_CRAFTING, completed.systemCode);
            Assert.AreEqual(CraftingOperations.SUCCESS_COMPLETED, completed.resultCode);

            Assert.IsTrue(OperationResult.IsError(stationMissing));
            Assert.IsTrue(OperationResult.IsFromSystem(stationMissing, CraftingOperations.SYSTEM_CRAFTING));
            Assert.AreEqual(CraftingOperations.ERROR_STATION_MISSING, stationMissing.resultCode);
        }
    }
}
