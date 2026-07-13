using JetBrains.Annotations;
using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data.Context
{
    /// <summary>
    ///     Context passed to <see cref="ReputationLevelBase"/> level-change events and to the
    ///     non-generic tier of <see cref="FactionBase"/> level-change events.
    /// </summary>
    public readonly ref struct FactionLevelChangeContext
    {
        /// <summary>The membership component whose level is changing.</summary>
        public readonly FactionMembershipBase membership;

        /// <summary>The faction in which the level change is occurring.</summary>
        public readonly FactionBase faction;

        /// <summary>
        ///     The level that was active before this change, or <c>null</c> if none was assigned.
        /// </summary>
        [CanBeNull] public readonly ReputationLevelBase previousLevel;

        /// <summary>
        ///     The level that is now active after this change, or <c>null</c> if the level was cleared.
        /// </summary>
        [CanBeNull] public readonly ReputationLevelBase newLevel;

        /// <summary>
        ///     Index of <see cref="previousLevel"/> in <c>FactionBase.Levels</c>.
        ///     <c>-1</c> if there was no previous level.
        /// </summary>
        public readonly int previousLevelIndex;

        /// <summary>
        ///     Index of <see cref="newLevel"/> in <c>FactionBase.Levels</c>.
        ///     <c>-1</c> if the level was cleared.
        /// </summary>
        public readonly int newLevelIndex;

        internal FactionLevelChangeContext(
            FactionMembershipBase membership,
            FactionBase faction,
            [CanBeNull] ReputationLevelBase previousLevel,
            [CanBeNull] ReputationLevelBase newLevel,
            int previousLevelIndex,
            int newLevelIndex)
        {
            this.membership = membership;
            this.faction = faction;
            this.previousLevel = previousLevel;
            this.newLevel = newLevel;
            this.previousLevelIndex = previousLevelIndex;
            this.newLevelIndex = newLevelIndex;
        }
    }

    /// <summary>
    ///     Typed context passed to <see cref="FactionBase{TFactionObject}"/> level-change events
    ///     and to the member-level checks in
    ///     <see cref="FactionMembershipBase{THolder}"/>.
    ///     Provides a strongly-typed <see cref="member"/> reference for custom logic.
    /// </summary>
    /// <typeparam name="TFactionObject">Type of the faction member holder.</typeparam>
    public readonly ref struct FactionLevelChangeContext<TFactionObject> where TFactionObject : class
    {
        /// <summary>
        ///     The typed holder of the membership component.
        ///     May be <c>null</c> if the holder could not be resolved via
        ///     <c>IHolderProvider&lt;TFactionObject&gt;</c>.
        /// </summary>
        [CanBeNull] public readonly TFactionObject member;

        /// <summary>The faction in which the level change is occurring.</summary>
        public readonly FactionBase faction;

        /// <summary>
        ///     The level that was active before this change, or <c>null</c> if none was assigned.
        /// </summary>
        [CanBeNull] public readonly ReputationLevelBase previousLevel;

        /// <summary>
        ///     The level that is now active after this change, or <c>null</c> if the level was cleared.
        /// </summary>
        [CanBeNull] public readonly ReputationLevelBase newLevel;

        /// <summary>
        ///     Index of <see cref="previousLevel"/> in <c>FactionBase.Levels</c>.
        ///     <c>-1</c> if there was no previous level.
        /// </summary>
        public readonly int previousLevelIndex;

        /// <summary>
        ///     Index of <see cref="newLevel"/> in <c>FactionBase.Levels</c>.
        ///     <c>-1</c> if the level was cleared.
        /// </summary>
        public readonly int newLevelIndex;

        internal FactionLevelChangeContext(
            [CanBeNull] TFactionObject member,
            FactionBase faction,
            [CanBeNull] ReputationLevelBase previousLevel,
            [CanBeNull] ReputationLevelBase newLevel,
            int previousLevelIndex,
            int newLevelIndex)
        {
            this.member = member;
            this.faction = faction;
            this.previousLevel = previousLevel;
            this.newLevel = newLevel;
            this.previousLevelIndex = previousLevelIndex;
            this.newLevelIndex = newLevelIndex;
        }
    }
}
