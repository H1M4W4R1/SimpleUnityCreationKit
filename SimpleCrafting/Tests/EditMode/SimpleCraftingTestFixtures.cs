using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Data;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Data.Runtime;
using Systems.SimpleCrafting.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleCrafting.Tests
{
    public abstract class SimpleCraftingTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            CraftingRecipeDatabase.ClearForTests();

            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                Object createdObject = _createdObjects[i];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestRecipe CreateRecipe(float durationSeconds = 0f)
        {
            TestRecipe recipe = Track(ScriptableObject.CreateInstance<TestRecipe>());
            recipe.DurationSecondsValue = durationSeconds;
            return recipe;
        }

        protected TestStation CreateStation()
        {
            GameObject stationObject = Track(new GameObject("Test Crafting Station"));
            return stationObject.AddComponent<TestStation>();
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(OperationResult.AreSimilar(expected, actual));
        }
    }

    public sealed class TestCraftingUser : ICraftingUser
    {
    }

    public sealed class TestRecipe : CraftingRecipeBase, ITimedCrafting
    {
        public bool RejectCraft { get; set; }
        public bool RejectIngredientAvailability { get; set; }
        public bool RejectOutputAvailability { get; set; }
        public bool FailConsume { get; set; }
        public bool FailRefund { get; set; }
        public bool FailGrant { get; set; }
        public float DurationSecondsValue { get; set; }
        public int ConsumedCount { get; private set; }
        public int RefundedCount { get; private set; }
        public int GrantedCount { get; private set; }
        public int StartedCount { get; private set; }
        public int StartFailedCount { get; private set; }
        public int CompletedCount { get; private set; }
        public int CompletionFailedCount { get; private set; }
        public int CancelledCount { get; private set; }
        public ICraftingUser LastUser { get; private set; }

        float ITimedCrafting.DurationSeconds => DurationSecondsValue;

        protected internal override OperationResult CanCraft(in CraftingContext context)
        {
            LastUser = context.user;
            return RejectCraft ? CraftingOperations.Denied() : CraftingOperations.Permitted();
        }

        protected internal override OperationResult CanConsumeCraftingIngredients(in CraftingContext context)
            => RejectIngredientAvailability ? CraftingOperations.Denied() : CraftingOperations.Permitted();

        protected internal override OperationResult TryConsumeCraftingIngredients(in CraftingContext context)
        {
            if (FailConsume) return CraftingOperations.Denied();
            ConsumedCount++;
            return CraftingOperations.Permitted();
        }

        protected internal override OperationResult TryRefundCraftingIngredients(in CraftingContext context)
        {
            if (FailRefund) return CraftingOperations.Denied();
            RefundedCount++;
            return CraftingOperations.Permitted();
        }

        protected internal override OperationResult CanGrantCraftingResult(in CraftingContext context)
            => RejectOutputAvailability ? CraftingOperations.Denied() : CraftingOperations.Permitted();

        protected internal override OperationResult TryGrantCraftingResult(in CraftingContext context)
        {
            if (FailGrant) return CraftingOperations.Denied();
            GrantedCount++;
            return CraftingOperations.Completed();
        }

        protected internal override void OnCraftingStarted(CraftingInstance instance)
        {
            StartedCount++;
        }

        protected internal override void OnCraftingStartFailed(
            in CraftingContext context,
            in OperationResult result)
        {
            StartFailedCount++;
        }

        protected internal override void OnCraftingCompleted(
            in CraftingContext context,
            in OperationResult result)
        {
            CompletedCount++;
        }

        protected internal override void OnCraftingCompletionFailed(
            in CraftingContext context,
            in OperationResult result)
        {
            CompletionFailedCount++;
        }

        protected internal override void OnCraftingCancelled(
            CraftingInstance instance,
            in OperationResult result)
        {
            CancelledCount++;
        }
    }

    public sealed class TestStation : CraftingStationBase
    {
        public bool RejectUse { get; set; }

        protected internal override OperationResult CanUseStation(in CraftingContext context)
            => RejectUse ? CraftingOperations.Denied() : CraftingOperations.Permitted();
    }
}
