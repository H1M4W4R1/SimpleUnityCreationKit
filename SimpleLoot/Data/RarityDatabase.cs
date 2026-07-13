using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleLoot.Abstract.Rarity;

namespace Systems.SimpleLoot.Data
{
    public sealed class RarityDatabase : AddressableDatabase<RarityDatabase, RarityBase>
    {
        public const string LABEL = "SimpleLoot.Rarities";
        protected override string AddressableLabel => LABEL;
    }
}
