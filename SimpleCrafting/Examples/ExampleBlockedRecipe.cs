using Systems.SimpleCore.Operations;
using Systems.SimpleCrafting.Abstract;
using Systems.SimpleCrafting.Data.Context;
using Systems.SimpleCrafting.Operations;

namespace Systems.SimpleCrafting.Examples
{
    public sealed class ExampleBlockedRecipe : CraftingRecipeBase
    {
        protected internal override OperationResult CanCraft(in CraftingContext context)
            => CraftingOperations.Denied();
    }
}
