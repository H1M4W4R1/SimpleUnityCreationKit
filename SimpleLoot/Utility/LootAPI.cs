using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Abstract.Generator;
using Systems.SimpleLoot.Abstract.LootTable;
using Systems.SimpleLoot.Data;
using UnityEngine;

namespace Systems.SimpleLoot.Utility
{
    public static class LootAPI
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() { }  

        public static ROListAccess<TLoot> GenerateLoot<TLootGenerator, TLoot>(
            [NotNull] LootTableBase<TLoot> table,
            long budget,
            LootGenerationFlags flags = default)
            where TLootGenerator : LootDropGeneratorBase<TLootGenerator, TLoot>, new()
        {
            TLootGenerator generator = LootDropGeneratorBase<TLootGenerator, TLoot>.Instance;

            if (!generator)
            {
                Debug.LogError(
                    $"[LootAPI] {typeof(TLootGenerator).Name} not found in LootGeneratorDatabase. " +
                    "Ensure the type is sealed and has [AutoCreate] in its hierarchy.");
                return RWListAccess<TLoot>.Create().ToReadOnly();
            }

            LootGenerationContext<TLoot> context = new(table, budget, flags);
            return generator.GenerateDrops(context);
        }
    }
}
