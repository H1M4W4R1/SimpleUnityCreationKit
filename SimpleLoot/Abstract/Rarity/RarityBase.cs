using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleLoot.Abstract.Interfaces;
using Systems.SimpleLoot.Data;
using UnityEngine;

namespace Systems.SimpleLoot.Abstract.Rarity
{
    [AutoCreate("Rarity", RarityDatabase.LABEL)]
    public abstract class RarityBase : ScriptableObject, IWithChance
    {
        public abstract float Chance { get; }
    }
}
