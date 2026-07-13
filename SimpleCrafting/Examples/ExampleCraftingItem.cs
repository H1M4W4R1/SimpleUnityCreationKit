using UnityEngine;

namespace Systems.SimpleCrafting.Examples
{
    [CreateAssetMenu(
        fileName = "Example Crafting Item",
        menuName = "Simple Crafting/Examples/Item")]
    public sealed class ExampleCraftingItem : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; } = "Item";
    }
}
