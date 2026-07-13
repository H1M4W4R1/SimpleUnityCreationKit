using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data.Context
{
    /// <summary>
    ///     Context passed to <see cref="FactionBase"/> reputation-change checks and events.
    ///     Holds untyped references suitable for the non-generic faction tier.
    /// </summary>
    public readonly ref struct ReputationChangeContext
    {
        /// <summary>The membership component whose reputation is changing.</summary>
        public readonly FactionMembershipBase membership;

        /// <summary>The faction in which reputation is changing.</summary>
        public readonly FactionBase faction;

        /// <summary>The reputation delta that was requested (positive = gain, negative = loss).</summary>
        public readonly long amountRequested;

        /// <summary>Reputation value before the change was applied.</summary>
        public readonly long previousReputation;

        internal ReputationChangeContext(
            FactionMembershipBase membership,
            FactionBase faction,
            long amountRequested,
            long previousReputation)
        {
            this.membership = membership;
            this.faction = faction;
            this.amountRequested = amountRequested;
            this.previousReputation = previousReputation;
        }
    }

    /// <summary>
    ///     Typed context passed to <see cref="FactionBase{TFactionObject}"/> reputation-change
    ///     checks and events.
    ///     Provides a strongly-typed <see cref="member"/> reference for custom logic.
    /// </summary>
    /// <typeparam name="TFactionObject">Type of the faction member holder.</typeparam>
    public readonly ref struct ReputationChangeContext<TFactionObject> where TFactionObject : class
    {
        /// <summary>
        ///     The typed holder of the membership component.
        ///     May be <c>null</c> if the holder could not be resolved via
        ///     <c>IHolderProvider&lt;TFactionObject&gt;</c>.
        /// </summary>
        public readonly TFactionObject member;

        /// <summary>The faction in which reputation is changing.</summary>
        public readonly FactionBase faction;

        /// <summary>The reputation delta that was requested (positive = gain, negative = loss).</summary>
        public readonly long amountRequested;

        /// <summary>Reputation value before the change was applied.</summary>
        public readonly long previousReputation;

        internal ReputationChangeContext(
            TFactionObject member,
            FactionBase faction,
            long amountRequested,
            long previousReputation)
        {
            this.member = member;
            this.faction = faction;
            this.amountRequested = amountRequested;
            this.previousReputation = previousReputation;
        }
    }
}
