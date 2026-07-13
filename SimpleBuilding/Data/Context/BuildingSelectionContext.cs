using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleCore.Utility.Enums;

namespace Systems.SimpleBuilding.Data.Context
{
    /// <summary>
    ///     Data supplied when a building entry is selected or displayed.
    /// </summary>
    public readonly ref struct BuildingSelectionContext
    {
        [CanBeNull] public readonly BuildingEntryBase entry;
        [CanBeNull] public readonly IBuildingUser user;
        [CanBeNull] public readonly BuildingRaycasterBase raycaster;
        public readonly ActionSource actionSource;

        public BuildingSelectionContext(
            [CanBeNull] BuildingEntryBase entry,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            ActionSource actionSource = ActionSource.External)
        {
            this.entry = entry;
            this.user = user;
            this.raycaster = raycaster;
            this.actionSource = actionSource;
        }
    }
}
