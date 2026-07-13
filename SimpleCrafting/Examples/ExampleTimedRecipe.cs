using Systems.SimpleCrafting.Abstract;

namespace Systems.SimpleCrafting.Examples
{
    public sealed class ExampleTimedRecipe : CraftingRecipeBase, ITimedCrafting
    {
        public float DurationSeconds => 5f;
    }
}
