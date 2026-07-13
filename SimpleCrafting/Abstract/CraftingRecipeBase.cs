using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Data;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Data.Runtime;
using Systems.SimpleCrafting.Operations;
using UnityEngine;

namespace Systems.SimpleCrafting.Abstract
{
    [AutoCreate("CraftingRecipes", CraftingRecipeDatabase.LABEL)]
    public abstract class CraftingRecipeBase : ScriptableObject
    {
        /// <summary>
        ///     Performs recipe-specific validation, including permissions, skill checks, and custom conditions.
        /// </summary>
        protected internal virtual OperationResult CanCraft(in CraftingContext context)
            => CraftingOperations.Permitted();

        /// <summary>
        ///     Checks whether all recipe-specific ingredients can be consumed.
        /// </summary>
        protected internal virtual OperationResult CanConsumeCraftingIngredients(in CraftingContext context)
            => CraftingOperations.Permitted();

        /// <summary>
        ///     Consumes all recipe-specific ingredients atomically.
        /// </summary>
        protected internal virtual OperationResult TryConsumeCraftingIngredients(in CraftingContext context)
            => CraftingOperations.Permitted();

        /// <summary>
        ///     Returns ingredients consumed by this craft. This must be atomic when possible.
        /// </summary>
        protected internal virtual OperationResult TryRefundCraftingIngredients(in CraftingContext context)
            => CraftingOperations.Permitted();

        /// <summary>
        ///     Checks whether the crafting result can be granted.
        /// </summary>
        protected internal virtual OperationResult CanGrantCraftingResult(in CraftingContext context)
            => CraftingOperations.Permitted();

        /// <summary>
        ///     Grants the complete crafting result atomically.
        /// </summary>
        protected internal virtual OperationResult TryGrantCraftingResult(in CraftingContext context)
            => CraftingOperations.Completed();

        protected internal virtual void OnCraftingStarted(CraftingInstance instance) { }

        protected internal virtual void OnCraftingStartFailed(
            in CraftingContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnCraftingCompleted(
            in CraftingContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnCraftingCompletionFailed(
            in CraftingContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnCraftingCancelled(
            CraftingInstance instance,
            in OperationResult result)
        {
        }
    }
}
