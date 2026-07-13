using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data.Context
{
    /// <summary>
    ///     Context passed to <see cref="FactionBase"/> join checks and events.
    ///     Holds untyped references suitable for the non-generic faction tier.
    /// </summary>
    public readonly ref struct JoinFactionContext
    {
        /// <summary>The membership component initiating the join.</summary>
        public readonly FactionMembershipBase membership;

        /// <summary>The faction being joined.</summary>
        public readonly FactionBase faction;

        internal JoinFactionContext(FactionMembershipBase membership, FactionBase faction)
        {
            this.membership = membership;
            this.faction = faction;
        }
    }

    /// <summary>
    ///     Typed context passed to <see cref="FactionBase{TFactionObject}"/> join checks and events.
    ///     Provides a strongly-typed <see cref="member"/> reference for custom logic.
    /// </summary>
    /// <typeparam name="TFactionObject">Type of the faction member holder.</typeparam>
    public readonly ref struct JoinFactionContext<TFactionObject> where TFactionObject : class
    {
        /// <summary>
        ///     The typed holder of the membership component.
        ///     May be <c>null</c> if the holder could not be resolved via
        ///     <c>IHolderProvider&lt;TFactionObject&gt;</c>.
        /// </summary>
        public readonly TFactionObject member;

        /// <summary>The faction being joined.</summary>
        public readonly FactionBase faction;

        internal JoinFactionContext(TFactionObject member, FactionBase faction)
        {
            this.member = member;
            this.faction = faction;
        }
    }
}
