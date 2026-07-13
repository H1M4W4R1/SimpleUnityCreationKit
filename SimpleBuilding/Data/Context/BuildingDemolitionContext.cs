using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;

namespace Systems.SimpleBuilding.Data.Context
{
    /// <summary>
    ///     Complete target and ownership data for a demolition attempt.
    /// </summary>
    public readonly ref struct BuildingDemolitionContext
    {
        [CanBeNull] public readonly BuildingBase building;
        [CanBeNull] public readonly IBuildingUser user;
        [CanBeNull] public readonly BuildingRaycasterBase raycaster;
        /// <summary>
        ///     Whether this demolition was requested while restoring or clearing a building save.
        /// </summary>
        public readonly bool isSaveSystemRequest;

        public BuildingDemolitionContext(
            [CanBeNull] BuildingBase building,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            bool isSaveSystemRequest = false)
        {
            this.building = building;
            this.user = user;
            this.raycaster = raycaster;
            this.isSaveSystemRequest = isSaveSystemRequest;
        }
    }
}
