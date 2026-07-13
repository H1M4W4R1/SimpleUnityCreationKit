using System.Collections.Generic;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Data;

namespace Systems.SimpleLoot.Examples.Scripts
{
    /// <summary>
    ///     Minimal weighted generator example.
    ///     Items are selected proportionally to their <see cref="ExampleLootItem.Chance"/> values.
    ///     <para>
    ///         This is the baseline implementation — override <see cref="LootDropGeneratorBase{TSelf,TLoot}.CanGenerateItem"/> or
    ///         <see cref="LootDropGeneratorBase{TSelf,TLoot}.GenerateDrops"/> to add custom logic (see other example generators).
    ///     </para>
    /// </summary>
    public sealed class ExampleWeightedItemGenerator
        : WeightedLootDropGenerator<ExampleWeightedItemGenerator, ExampleLootItem>
    {
        protected override void OnLootGenerated(
            IReadOnlyList<ExampleLootItem> loot, in LootGenerationContext<ExampleLootItem> context)
        {
            // Called after each successful GenerateDrops. Hook UI updates, analytics, or sound here.
        }

        protected override void OnLootGenerationFailed(in LootGenerationContext<ExampleLootItem> context)
        {
            // Called when no valid items could be selected (empty table, all blocked, budget <= 0).
        }
    }
}
