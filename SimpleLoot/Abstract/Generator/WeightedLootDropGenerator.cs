using System;
using System.Collections.Generic;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Data;
using Random = UnityEngine.Random;

namespace Systems.SimpleLoot.Abstract.Generator
{
    public abstract class WeightedLootDropGenerator<TSelf, TLoot>
        : LootDropGeneratorBase<TSelf, TLoot>
        where TSelf : WeightedLootDropGenerator<TSelf, TLoot>, new()
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

            Span<float> weights = stackalloc float[entries.Count];
            Span<int> indices   = stackalloc int[entries.Count];
            float totalWeight   = 0f;
            int validCount      = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (!IsItemAllowed(entries[i], context)) continue;
                float w = ResolveChance(entries[i]);
                if (w < 0f || float.IsNaN(w) || float.IsInfinity(w)) continue;
                weights[validCount] = w;
                indices[validCount] = i;
                totalWeight += w;
                validCount++;
            }

            if (validCount == 0)
            {
                OnLootGenerationFailed(context);
                return result.ToReadOnly();
            }

            if (totalWeight <= 0f)
            {
                for (int i = 0; i < validCount; i++) weights[i] = 1f;
                totalWeight = validCount;
            }

            List<TLoot> refList = result.List;
            for (long roll = 0; roll < context.budget; roll++)
            {
                float rand = Random.Range(0f, totalWeight);
                float cumulative = 0f;
                int selected = indices[validCount - 1];

                for (int i = 0; i < validCount; i++)
                {
                    cumulative += weights[i];
                    if (rand <= cumulative)
                    {
                        selected = indices[i];
                        break;
                    }
                }

                refList.Add(entries[selected].Item);
            }

            OnLootGenerated(result.List, context);
            return result.ToReadOnly();
        }
    }
}
