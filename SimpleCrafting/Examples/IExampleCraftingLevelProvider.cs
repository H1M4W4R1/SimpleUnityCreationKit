using Systems.SimpleCrafting.Abstract;

namespace Systems.SimpleCrafting.Examples
{
    public interface IExampleCraftingLevelProvider : ICraftingUser
    {
        int CraftingLevel { get; }
    }
}
