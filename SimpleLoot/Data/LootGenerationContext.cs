using JetBrains.Annotations;
using Systems.SimpleLoot.Abstract.LootTable;

namespace Systems.SimpleLoot.Data
{
    public readonly ref struct LootGenerationContext<TLoot>
    {
        public readonly LootTableBase<TLoot> table;
        public readonly long budget;
        public readonly LootGenerationFlags flags;

        internal LootGenerationContext(
            [NotNull] LootTableBase<TLoot> table, long budget, LootGenerationFlags flags)
        {
            this.table = table;
            this.budget = budget;
            this.flags = flags;
        }
    }
}
