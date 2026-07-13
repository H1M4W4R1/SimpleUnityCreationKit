using System.Collections.Generic;
using Systems.SimpleLoot.Data;
using UnityEngine;

namespace Systems.SimpleLoot.Abstract.LootTable
{
    public abstract class LootTableBase : ScriptableObject { }

    public abstract class LootTableBase<TLoot> : LootTableBase
    {
        [SerializeField] private List<LootTableEntry<TLoot>> _entries = new();
        public IReadOnlyList<LootTableEntry<TLoot>> Entries => _entries;
    }
}
