using JetBrains.Annotations;
using Systems.SimpleEntities.Components;
using Systems.SimpleEntities.Data.Affinity;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Systems.SimpleEntities.Data.Context
{
    /// <summary>
    ///     Context for healing
    /// </summary>
    public readonly ref struct HealContext
    {
        /// <summary>
        ///     Source of the healing
        /// </summary>
        [CanBeNull] public readonly object source;

        /// <summary>
        ///     Target of the healing
        /// </summary>
        [NotNull] public readonly AliveEntityBase target;

        /// <summary>
        ///     Healing affinity
        /// </summary>
        [CanBeNull] public readonly AffinityType affinityType;

        /// <summary>
        ///     Resistance value
        /// </summary>
        public readonly float resistanceValue;

        /// <summary>
        ///     Amount of healing
        /// </summary>
        public readonly long amount;
        
        public HealContext(
            [NotNull] AliveEntityBase target,
            [CanBeNull] object source,
            [CanBeNull] AffinityType affinityType,
            float resistanceValue,
            long amount)
        {
            Assert.IsTrue(amount >= 0, "Amount of healing must be greater than or equal to zero");
            Assert.IsNotNull(target, "Target cannot be null");
            this.target = target;
            this.source = source;
            this.affinityType = affinityType;
            this.resistanceValue = resistanceValue;
            
            // Resistance amplifies healing (0 = base, positive = more healing).
            // e.g. fire-resistant enemies gain more health from fire-based healing spells.
            // Negative resistance reduces healing; at -1 the result is 0 HP (events still fire).
            // Clamped at 0 to prevent healing from becoming negative (damage).
            this.amount = (long) (amount * math.max(0, 1 + resistanceValue));
        }

        /// <summary>
        ///     Creates a HealContext for the given affinity type.
        ///     Note: The new() constraint on TDamageAffinity is required by the database lookup
        ///     (AddressableDatabase.GetExact) for hash identification -- it must NOT be used
        ///     to construct AffinityType instances directly, as they are ScriptableObjects.
        /// </summary>
        public static HealContext Create<TDamageAffinity>(
            [NotNull] AliveEntityBase target,
            [CanBeNull] object source,
            long amount)
            where TDamageAffinity : AffinityType, new()
        {
            Assert.IsNotNull(target, "Target cannot be null");
            Assert.IsTrue(amount >= 0, "Amount of healing must be greater than or equal to zero");

            float resistanceValue = target.GetResistance<TDamageAffinity>();
            return new HealContext(target, source, AffinityDatabase.GetExact<TDamageAffinity>(), resistanceValue,
                amount);
        }
    }
}