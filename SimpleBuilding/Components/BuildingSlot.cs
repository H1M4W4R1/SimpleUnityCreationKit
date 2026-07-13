using JetBrains.Annotations;
using Systems.SimpleBuilding.Utility;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     An optional reserved world position for buildings that implement <c>ISlotBuilding</c>.
    /// </summary>
    public sealed class BuildingSlot : MonoBehaviour
    {
        [SerializeField] private string _saveIdentifier;
        [CanBeNull] private BuildingBase _occupyingBuilding;
        private Transform _cachedTransform;

        /// <summary>
        ///     Stable identifier written to building save files for slot reservations.
        ///     Set a unique value before shipping; the GameObject name is used only as a backwards-compatible fallback.
        /// </summary>
        [NotNull]
        public string SaveIdentifier => string.IsNullOrWhiteSpace(_saveIdentifier) ? gameObject.name : _saveIdentifier;

        /// <summary>
        ///     Sets the stable identifier used to resolve this slot from a building save file.
        /// </summary>
        /// <param name="saveIdentifier">Identifier unique among active building slots.</param>
        public void SetSaveIdentifier([NotNull] string saveIdentifier)
        {
            _saveIdentifier = saveIdentifier;
        }

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

        private void OnEnable()
        {
            BuildingRegistry.RegisterSlot(this);
        }

        private void OnDisable()
        {
            BuildingRegistry.UnregisterSlot(this);
        }

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
