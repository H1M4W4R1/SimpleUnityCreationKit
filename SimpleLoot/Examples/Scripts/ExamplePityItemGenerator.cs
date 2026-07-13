using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Operations;
using UnityEngine;

namespace Systems.SimpleLoot.Examples.Scripts
{
    /// <summary>
    ///     Pity system generator example.
    ///     Tracks how many rolls have passed without a rare item dropping.
    ///     Once <see cref="PityThreshold"/> is reached, all non-rare items are blocked
    ///     for the next roll, guaranteeing a rare drop.
    ///     The counter resets after a rare item appears in the results.
    ///     <para>
    ///         Implementation notes:
    ///         <list type="bullet">
    ///             <item>
    ///                 <see cref="GenerateDrops"/> sets the <c>_isPityActive</c> flag before
    ///                 delegating to <c>base.GenerateDrops</c> so that <see cref="CanGenerateItem"/>
    ///                 can act on it during the same generation call.
    ///             </item>
    ///             <item>
    ///                 <see cref="OnLootGenerated"/> updates the counter after results are known.
    ///             </item>
    ///             <item>
    ///                 <c>_rollsSinceLastRare</c> survives Play mode entry because it lives on the
    ///                 ScriptableObject asset. Reset it via <see cref="ResetStaticState"/> or
    ///                 add a public <c>ResetPity()</c> method if needed.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    public sealed class ExamplePityItemGenerator
        : WeightedLootDropGenerator<ExamplePityItemGenerator, ExampleLootItem>
    {
        private const int PityThreshold = 10;

        private int _rollsSinceLastRare;
        private bool _isPityActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            // Instance is cached per closed generic type — clear on domain reload.
            // The pity counter itself lives on the SO asset and is not reset here.
        }

        public override ROListAccess<ExampleLootItem> GenerateDrops(
            in LootGenerationContext<ExampleLootItem> context)
        {
            _isPityActive = _rollsSinceLastRare >= PityThreshold;
            return base.GenerateDrops(context);
        }

        protected override OperationResult CanGenerateItem(
            LootTableEntry<ExampleLootItem> entry,
            in LootGenerationContext<ExampleLootItem> context)
        {
            // When pity is active, block all non-rare items so the next roll is guaranteed rare.
            if (_isPityActive && !entry.Item.IsRare)
                return LootOperations.ItemConditionFailed();

            return LootOperations.Permitted();
        }

        protected override void OnLootGenerated(
            IReadOnlyList<ExampleLootItem> loot,
            in LootGenerationContext<ExampleLootItem> context)
        {
            bool rareDropped = false;
            for (int i = 0; i < loot.Count; i++)
            {
                if (!loot[i].IsRare) continue;
                rareDropped = true;
                break;
            }

            if (rareDropped)
            {
                _rollsSinceLastRare = 0;
                _isPityActive = false;
                Debug.Log("[PityGenerator] Rare item dropped — pity counter reset.");
            }
            else
            {
                _rollsSinceLastRare += (int)context.budget;
                Debug.Log($"[PityGenerator] No rare drop. Pity: {_rollsSinceLastRare}/{PityThreshold}");
            }
        }

        protected override void OnLootGenerationFailed(in LootGenerationContext<ExampleLootItem> context)
        {
            Debug.LogWarning("[PityGenerator] Generation failed — no valid items in table.");
        }
    }
}
