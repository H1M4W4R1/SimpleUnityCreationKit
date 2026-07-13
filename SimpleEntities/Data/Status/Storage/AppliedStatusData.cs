using System;
using JetBrains.Annotations;
using Systems.SimpleEntities.Data.Status.Abstract;
using UnityEngine;

namespace Systems.SimpleEntities.Data.Status.Storage
{
    /// <summary>
    ///     Contains data about applied status.
    ///     This is a mutable struct stored in a List. When modifying stackCount,
    ///     you must copy-back the struct to the list (e.g. _appliedStatuses[i] = modified).
    ///     This is intentional for performance -- avoiding class allocation overhead.
    /// </summary>
    [Serializable]
    public struct AppliedStatusData
    {
        /// <summary>
        ///     Status that is applied
        /// </summary>
        [NotNull] [SerializeReference] public readonly StatusBase status;
        
        /// <summary>
        ///     Current stack count
        /// </summary>
        public int stackCount;

        public AppliedStatusData([NotNull] StatusBase status, int stackCount)
        {
            this.status = status;
            this.stackCount = stackCount;
        }
    }
}