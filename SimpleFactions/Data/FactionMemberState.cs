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
        /// <summary>Whether the tracked object is currently a member of this faction.</summary>
        internal bool isMember;
    }
}
