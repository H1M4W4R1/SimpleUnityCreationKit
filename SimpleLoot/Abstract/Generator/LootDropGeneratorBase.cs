using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleLoot.Abstract.Interfaces;
using Systems.SimpleLoot.Data;
using Systems.SimpleLoot.Operations;
using UnityEngine;

namespace Systems.SimpleLoot.Abstract.Generator
{
    [AutoCreate("LootGenerators", LootGeneratorDatabase.LABEL)]
    public abstract class LootDropGeneratorBase : ScriptableObject { }

    public abstract class LootDropGeneratorBase<TSelf, TLoot> : LootDropGeneratorBase
        where TSelf : LootDropGeneratorBase<TSelf, TLoot>, new()
    {
        private static TSelf _instance;

        [CanBeNull] public static TSelf Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = LootGeneratorDatabase.GetExact<TSelf>();
                return _instance;
            }
        }

        protected abstract void OnLootGenerated(
            [NotNull] IReadOnlyList<TLoot> loot, in LootGenerationContext<TLoot> context);

        protected abstract void OnLootGenerationFailed(in LootGenerationContext<TLoot> context);

        protected virtual OperationResult CanGenerateItem(
            [NotNull] LootTableEntry<TLoot> entry, in LootGenerationContext<TLoot> context)
            => LootOperations.Permitted();

        protected bool IsItemAllowed(
            [NotNull] LootTableEntry<TLoot> entry, in LootGenerationContext<TLoot> context)
        {
            if ((context.flags & LootGenerationFlags.IgnoreConditions) != 0) return true;
            return CanGenerateItem(entry, context);
        }

        protected static float ResolveChance([NotNull] LootTableEntry<TLoot> entry)
        {
            if (entry.RarityOverride) return entry.RarityOverride.Chance;
            if (entry.Item is IWithChance withChance) return withChance.Chance;
            if (entry.Item is IWithRarity withRarity && withRarity.Rarity != null)
                return withRarity.Rarity.Chance;
            return 0f;
        }

        public abstract ROListAccess<TLoot> GenerateDrops(in LootGenerationContext<TLoot> context);
    }
}
