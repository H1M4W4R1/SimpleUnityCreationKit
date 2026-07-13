using System;

namespace Systems.SimpleFactions.Data
{
    /// <summary>
    ///     Runtime state for a single faction membership tracked by
    ///     <see cref="Abstract.FactionMembershipBase{THolder}"/>.
    ///     One instance is stored per faction type in the component's internal dictionary.
    /// </summary>
    [Serializable]
    internal sealed class FactionMemberState
    {
        /// <summary>Current reputation with this faction.</summary>
        internal long reputation;

        /// <summary>
        ///     Index into <c>FactionBase.Levels</c> for the current reputation level.
        ///     <c>-1</c> means no level has been assigned.
        /// </summary>
        internal int currentLevelIndex = -1;

        /// <summary>Whether the tracked object is currently a member of this faction.</summary>
        internal bool isMember;
    }
}
