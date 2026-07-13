using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using UnityEngine;

namespace Systems.SimpleBuilding.Data.Context
{
    /// <summary>
    ///     Complete location, ownership, and selection data for a placement attempt.
    /// </summary>
    public readonly ref struct BuildingPlacementContext
    {
        [CanBeNull] public readonly BuildingEntryBase entry;
        [CanBeNull] public readonly IBuildingUser user;
        [CanBeNull] public readonly BuildingRaycasterBase raycaster;
        [CanBeNull] public readonly Transform parent;
        [CanBeNull] public readonly IReadOnlyList<BuildingSlot> slots;
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        /// <summary>
        ///     Whether this placement was requested while restoring a building save.
        /// </summary>
        public readonly bool isSaveSystemRequest;

        public BuildingPlacementContext(
            [CanBeNull] BuildingEntryBase entry,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            [CanBeNull] Transform parent = null,
            [CanBeNull] IReadOnlyList<BuildingSlot> slots = null,
            bool isSaveSystemRequest = false)
        {
            this.entry = entry;
            this.position = position;
            this.rotation = rotation;
            this.user = user;
            this.raycaster = raycaster;
            this.parent = parent;
            this.slots = slots;
            this.isSaveSystemRequest = isSaveSystemRequest;
        }
    }
}
