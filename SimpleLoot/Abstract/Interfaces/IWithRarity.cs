using Systems.SimpleLoot.Abstract.Rarity;

namespace Systems.SimpleLoot.Abstract.Interfaces
{
    public interface IWithRarity
    {
        RarityBase Rarity { get; }
    }
}
