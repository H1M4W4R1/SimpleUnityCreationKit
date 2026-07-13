using Systems.SimpleLoot.Abstract.LootTable;
using UnityEngine;

namespace Systems.SimpleLoot.Examples.Scripts
{
    /// <summary>
    ///     Concrete loot table for <see cref="ExampleLootItem"/>.
    ///     Create via right-click → SimpleLoot/Examples/Item Loot Table.
    ///     Add entries in the inspector; each entry can optionally override
    ///     the item's own chance with a specific <see cref="Abstract.Rarity.RarityBase"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ExampleItemLootTable", menuName = "SimpleLoot/Examples/Item Loot Table")]
    public sealed class ExampleItemLootTable : LootTableBase<ExampleLootItem> { }
}
