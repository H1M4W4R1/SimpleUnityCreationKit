using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data.Context
{
    /// <summary>
    ///     Context passed to <see cref="FactionBase"/> leave checks and events.
    ///     Holds untyped references suitable for the non-generic faction tier.
    /// </summary>
    public readonly ref struct LeaveFactionContext
    {
        /// <summary>The membership component initiating the leave.</summary>
        public readonly FactionMembershipBase membership;

        /// <summary>The faction being left.</summary>
        public readonly FactionBase faction;

        internal LeaveFactionContext(FactionMembershipBase membership, FactionBase faction)
        {
            this.membership = membership;
            this.faction = faction;
        }
    }

    /// <summary>
    ///     Typed context passed to <see cref="FactionBase{TFactionObject}"/> leave checks and events.
    ///     Provides a strongly-typed <see cref="member"/> reference for custom logic.
    /// </summary>
    /// <typeparam name="TFactionObject">Type of the faction member holder.</typeparam>
    public readonly ref struct LeaveFactionContext<TFactionObject> where TFactionObject : class
    {
        /// <summary>
        ///     The typed holder of the membership component.
        ///     May be <c>null</c> if the holder could not be resolved via
        ///     <c>IHolderProvider&lt;TFactionObject&gt;</c>.
        /// </summary>
        public readonly TFactionObject member;

        /// <summary>The faction being left.</summary>
        public readonly FactionBase faction;

        internal LeaveFactionContext(TFactionObject member, FactionBase faction)
        {
            this.member = member;
            this.faction = faction;
        }
    }
}
