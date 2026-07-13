using System;
using System.Collections.Generic;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Data;
using Random = UnityEngine.Random;

namespace Systems.SimpleLoot.Abstract.Generator
{
    public abstract class EqualLootDropGenerator<TSelf, TLoot>
        : LootDropGeneratorBase<TSelf, TLoot>
        where TSelf : EqualLootDropGenerator<TSelf, TLoot>, new()
    {
        public override ROListAccess<TLoot> GenerateDrops(in LootGenerationContext<TLoot> context)
        {
            IReadOnlyList<LootTableEntry<TLoot>> entries = context.table.Entries;
            RWListAccess<TLoot> result = RWListAccess<TLoot>.Create();

            if (entries.Count == 0 || context.budget <= 0)
            {
                OnLootGenerationFailed(context);
                return result.ToReadOnly();
            }

            Span<int> eligibleIndices = stackalloc int[entries.Count];
            int eligibleCount = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (IsItemAllowed(entries[i], context))
                    eligibleIndices[eligibleCount++] = i;
            }

            if (eligibleCount == 0)
            {
                OnLootGenerationFailed(context);
                return result.ToReadOnly();
            }

            List<TLoot> refList = result.List;
            for (long roll = 0; roll < context.budget; roll++)
                refList.Add(entries[eligibleIndices[Random.Range(0, eligibleCount)]].Item);

            OnLootGenerated(result.List, context);
            return result.ToReadOnly();
        }
    }
}
