using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Operations;
using UnityEngine;

namespace Systems.SimpleLoot.Examples.Scripts
{
    /// <summary>
    ///     Conditional generator example.
    ///     Demonstrates <see cref="CanGenerateItem"/> — locked items are blocked from dropping.
    ///     Pass <see cref="LootGenerationFlags.IgnoreConditions"/> to bypass the lock check,
    ///     e.g., for admin commands or debug scenarios.
    /// </summary>
    public sealed class ExampleConditionalItemGenerator
        : WeightedLootDropGenerator<ExampleConditionalItemGenerator, ExampleLootItem>
    {
        protected override OperationResult CanGenerateItem(
            LootTableEntry<ExampleLootItem> entry,
            in LootGenerationContext<ExampleLootItem> context)
        {
            if (entry.Item.IsLocked)
                return LootOperations.ItemConditionFailed();

            return LootOperations.Permitted();
        }

        protected override void OnLootGenerated(
            IReadOnlyList<ExampleLootItem> loot, in LootGenerationContext<ExampleLootItem> context)
        {
            for (int i = 0; i < loot.Count; i++)
                Debug.Log($"[ConditionalGenerator] Dropped: {loot[i].DisplayName}");
        }

        protected override void OnLootGenerationFailed(in LootGenerationContext<ExampleLootItem> context)
        {
            Debug.LogWarning("[ConditionalGenerator] No valid items — all may be locked.");
        }
    }
}
