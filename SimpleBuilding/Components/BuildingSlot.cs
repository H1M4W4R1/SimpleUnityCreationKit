using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     An optional reserved world position for buildings that implement <c>ISlotBuilding</c>.
    /// </summary>
    public sealed class BuildingSlot : MonoBehaviour
    {
        [CanBeNull] private BuildingBase _occupyingBuilding;
        private Transform _cachedTransform;

        /// <summary>
        ///     Transform used by slot-snapping buildings for their placement position and base rotation.
        /// </summary>
        [NotNull]
        public Transform SnapTransform
        {
            get
            {
                if (ReferenceEquals(_cachedTransform, null) || !_cachedTransform)
                    _cachedTransform = transform;

                return _cachedTransform;
            }
        }

        [CanBeNull]
        public BuildingBase OccupyingBuilding
        {
            get
            {
                if (!ReferenceEquals(_occupyingBuilding, null) && !_occupyingBuilding)
                    _occupyingBuilding = null;

                return _occupyingBuilding;
            }
        }

        public bool IsOccupied => !ReferenceEquals(OccupyingBuilding, null);

        internal bool TryOccupy([NotNull] BuildingBase building)
        {
            BuildingBase occupiedBuilding = OccupyingBuilding;
            if (!ReferenceEquals(occupiedBuilding, null) && !ReferenceEquals(occupiedBuilding, building))
                return false;

            _occupyingBuilding = building;
            return true;
        }

        internal void Release([NotNull] BuildingBase building)
        {
            if (ReferenceEquals(_occupyingBuilding, building)) _occupyingBuilding = null;
        }
    }
}
