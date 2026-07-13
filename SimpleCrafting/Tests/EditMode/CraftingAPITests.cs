using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Data;
using Systems.SimpleCrafting.Data.Enums;
using Systems.SimpleCrafting.Data.Runtime;
using Systems.SimpleCrafting.Operations;
using Systems.SimpleCrafting.Utility;

namespace Systems.SimpleCrafting.Tests
{
    public sealed class CraftingAPITests : SimpleCraftingTestBase
    {
        [Test]
        public void CanCraft_WhenRecipeOverrideRejectsIngredients_ReturnsFailureWithoutSideEffects()
        {
            TestRecipe recipe = CreateRecipe();
            recipe.RejectIngredientAvailability = true;

            OperationResult result = CraftingAPI.CanCraft(recipe, user: new TestCraftingUser());

            AssertSimilar(CraftingOperations.Denied(), result);
            Assert.AreEqual(0, recipe.ConsumedCount);
            Assert.AreEqual(0, recipe.GrantedCount);
        }

        [Test]
        public void TryStartCrafting_ForInstantRecipe_UsesRecipeOverrides()
        {
            TestRecipe recipe = CreateRecipe();
            TestCraftingUser user = new TestCraftingUser();

            OperationResult result = CraftingAPI.TryStartCrafting(recipe, out CraftingInstance instance, user: user);

            AssertSimilar(CraftingOperations.Completed(), result);
            Assert.IsNull(instance);
            Assert.AreEqual(1, recipe.ConsumedCount);
            Assert.AreEqual(1, recipe.GrantedCount);
            Assert.AreSame(user, recipe.LastUser);
            Assert.AreEqual(1, recipe.CompletedCount);
        }

        [Test]
        public void TryStartCrafting_ForTimedRecipe_CompletesAfterAdvance()
        {
            TestRecipe recipe = CreateRecipe(5f);

            OperationResult startResult = CraftingAPI.TryStartCrafting(recipe, out CraftingInstance instance);
            OperationResult earlyCompleteResult = CraftingAPI.TryCompleteCrafting(instance);
            OperationResult advanceResult = CraftingAPI.AdvanceCrafting(instance, 5f);
            OperationResult completeResult = CraftingAPI.TryCompleteCrafting(instance);

            AssertSimilar(CraftingOperations.Started(), startResult);
            Assert.IsNotNull(instance);
            AssertSimilar(CraftingOperations.InstanceNotReady(), earlyCompleteResult);
            AssertSimilar(CraftingOperations.Ready(), advanceResult);
            AssertSimilar(CraftingOperations.Completed(), completeResult);
            Assert.AreEqual(CraftingInstanceState.Completed, instance.State);
            Assert.AreEqual(1, recipe.ConsumedCount);
            Assert.AreEqual(1, recipe.GrantedCount);
            Assert.AreEqual(1, recipe.StartedCount);
            Assert.AreEqual(1, recipe.CompletedCount);
        }

        [Test]
        public void AdvanceAllCrafting_CompletesRegisteredTimedInstances()
        {
            TestRecipe recipe = CreateRecipe(2f);
            OperationResult startResult = CraftingAPI.TryStartCrafting(recipe, out CraftingInstance instance);

            OperationResult tickResult = CraftingAPI.AdvanceAllCrafting(2f);

            AssertSimilar(CraftingOperations.Started(), startResult);
            AssertSimilar(CraftingOperations.Completed(), tickResult);
            Assert.AreEqual(CraftingInstanceState.Completed, instance!.State);
            Assert.AreEqual(1, recipe.GrantedCount);
        }

        [Test]
        public void TryCancelCrafting_RefundsThroughRecipeOverride()
        {
            TestRecipe recipe = CreateRecipe(10f);

            CraftingAPI.TryStartCrafting(recipe, out CraftingInstance instance);
            OperationResult cancelResult = CraftingAPI.TryCancelCrafting(instance);

            AssertSimilar(CraftingOperations.Cancelled(), cancelResult);
            Assert.AreEqual(CraftingInstanceState.Cancelled, instance!.State);
            Assert.AreEqual(1, recipe.ConsumedCount);
            Assert.AreEqual(1, recipe.RefundedCount);
            Assert.AreEqual(1, recipe.CancelledCount);
        }

        [Test]
        public void TryStartCrafting_WhenResultGrantFails_RefundsThroughRecipeOverride()
        {
            TestRecipe recipe = CreateRecipe();
            recipe.FailGrant = true;

            OperationResult result = CraftingAPI.TryStartCrafting(recipe, out CraftingInstance instance);

            AssertSimilar(CraftingOperations.Denied(), result);
            Assert.IsNull(instance);
            Assert.AreEqual(1, recipe.ConsumedCount);
            Assert.AreEqual(1, recipe.RefundedCount);
            Assert.AreEqual(1, recipe.CompletionFailedCount);
        }

        [Test]
        public void CanCraft_WithStationComponent_UsesStationOverride()
        {
            TestRecipe recipe = CreateRecipe();
            TestStation station = CreateStation();
            station.RejectUse = true;

            OperationResult rejectedResult = CraftingAPI.CanCraft(recipe, station);
            station.RejectUse = false;
            OperationResult allowedResult = CraftingAPI.CanCraft(recipe, station);

            AssertSimilar(CraftingOperations.Denied(), rejectedResult);
            AssertSimilar(CraftingOperations.Permitted(), allowedResult);
        }

        [Test]
        public void GenericRecipeOverloads_ResolveTheRegisteredRecipe()
        {
            TestRecipe recipe = CreateRecipe();
            CraftingRecipeDatabase.RegisterForTests(recipe);
            TestCraftingUser user = new TestCraftingUser();

            OperationResult canCraftResult = CraftingAPI.CanCraft<TestRecipe>(user: user);
            OperationResult startResult = CraftingAPI.TryStartCrafting<TestRecipe>(out CraftingInstance instance, user: user);

            AssertSimilar(CraftingOperations.Permitted(), canCraftResult);
            AssertSimilar(CraftingOperations.Completed(), startResult);
            Assert.IsNull(instance);
            Assert.AreEqual(1, recipe.ConsumedCount);
            Assert.AreEqual(1, recipe.GrantedCount);
        }
    }
}
