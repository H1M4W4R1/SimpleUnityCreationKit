using System;
using JetBrains.Annotations;
using Systems.SimpleLoot.Abstract.Rarity;
using UnityEngine;

namespace Systems.SimpleLoot.Data
{
    [Serializable]
    public sealed class LootTableEntry<TLoot>
    {
        [SerializeField] private TLoot _item;
        [SerializeField] [CanBeNull] private RarityBase _rarityOverride;

        public TLoot Item => _item;
        [CanBeNull] public RarityBase RarityOverride => _rarityOverride;
    }
}
