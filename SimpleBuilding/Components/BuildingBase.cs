using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Base component for a placed building instance.
    /// </summary>
    public abstract class BuildingBase : MonoBehaviour
    {
        private readonly List<BuildingSlot> _occupiedSlots = new List<BuildingSlot>();
        [CanBeNull] private BuildingEntryBase _entry;
        private Transform _cachedTransform;

        [CanBeNull] public BuildingEntryBase Entry => _entry;
        [NotNull] public IReadOnlyList<BuildingSlot> OccupiedSlots => _occupiedSlots;

        protected Transform CachedTransform
        {
            get
            {
                if (ReferenceEquals(_cachedTransform, null) || !_cachedTransform)
                    _cachedTransform = transform;

                return _cachedTransform;
            }
        }

        /// <summary>
        ///     Attempts to demolish this building through the common transaction API.
        /// </summary>
        public OperationResult TryDemolish(
            [CanBeNull] IBuildingUser user = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingDemolitionContext context = new BuildingDemolitionContext(this, user, null, actionSource);
            return BuildingAPI.TryDemolish(in context);
        }

        /// <summary>
        ///     Performs instance-specific demolition validation.
        /// </summary>
        protected internal virtual OperationResult CanBeDemolished(in BuildingDemolitionContext context)
            => BuildingOperations.Permitted();

        protected internal virtual void OnBuildingPlaced(
            in BuildingPlacementContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnBuildingDemolished(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
        }

        protected internal virtual void OnBuildingDemolitionFailed(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
        }

        protected virtual void OnDestroy()
        {
            ReleaseOccupiedSlots();
        }

        internal void Initialize([NotNull] BuildingEntryBase entry)
        {
            _entry = entry;
        }

        internal bool TryAssignSlots([CanBeNull] IReadOnlyList<BuildingSlot> slots)
        {
            if (ReferenceEquals(slots, null)) return true;

            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                BuildingSlot slot = slots[slotIndex];
                if (ReferenceEquals(slot, null) || !slot || !slot.TryOccupy(this))
                {
                    ReleaseOccupiedSlots();
                    return false;
                }

                _occupiedSlots.Add(slot);
            }

            return true;
        }

        internal void ReleaseOccupiedSlots()
        {
            for (int slotIndex = _occupiedSlots.Count - 1; slotIndex >= 0; slotIndex--)
            {
                BuildingSlot slot = _occupiedSlots[slotIndex];
                if (!ReferenceEquals(slot, null) && slot) slot.Release(this);
            }

            _occupiedSlots.Clear();
        }
    }
}
