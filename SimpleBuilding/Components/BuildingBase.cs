using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Base component for a placed building instance. It owns placement and demolition lifecycle work.
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
        ///     Validates a placement using the completed building prefab and its required slots.
        /// </summary>
        public static OperationResult CanBuild(in BuildingPlacementContext context)
        {
            if (ReferenceEquals(context.entry, null)) return BuildingOperations.EntryIsNull();
            if (!context.entry) return BuildingOperations.EntryIsNull();

            BuildingEntryBase entry = context.entry;
            BuildingRegistry.RegisterEntry(entry);
            BuildingBase prefab = entry.GetPrefab();
            if (ReferenceEquals(prefab, null) || !prefab) return BuildingOperations.PrefabMissing();

            BuildingSelectionContext selectionContext = new BuildingSelectionContext(
                entry, context.user, context.raycaster);
            OperationResult result = entry.IsAvailable(in selectionContext);
            if (!result) return result;

            result = ValidateSlots(prefab, context.slots);
            if (!result) return result;

            return entry.CanBuild(in context);
        }

        /// <summary>
        ///     Consumes placement resources, creates the building instance, and reserves its slots.
        /// </summary>
        public static OperationResult TryBuild(
            in BuildingPlacementContext context,
            [CanBeNull] out BuildingBase building)
        {
            building = null;
            OperationResult result = CanBuild(in context);
            if (!result)
            {
                NotifyPlacementFailed(in context, in result);
                return result;
            }

            BuildingEntryBase entry = context.entry;
            result = entry.TryConsumeResources(in context);
            if (!result)
            {
                NotifyPlacementFailed(in context, in result);
                return result;
            }

            BuildingBase prefab = entry.GetPrefab();
            BuildingBase instance = Object.Instantiate(prefab, context.position, context.rotation, context.parent);
            instance.Initialize(entry);

            if (!instance.TryAssignSlots(context.slots))
            {
                BuildingDemolitionContext refundContext = new BuildingDemolitionContext(
                    instance, context.user, context.raycaster);
                OperationResult refundResult = entry.TryRefundResources(in refundContext);
                BuildingRegistry.UnregisterBuilding(instance);
                DestroyGameObject(instance.gameObject);
                OperationResult finalResult = refundResult
                    ? BuildingOperations.SlotOccupied()
                    : BuildingOperations.RefundFailed();
                NotifyPlacementFailed(in context, in finalResult);
                return finalResult;
            }

            building = instance;
            OperationResult placedResult = BuildingOperations.Placed();
            entry.OnBuildingPlaced(in context, instance, in placedResult);
            instance.OnBuildingPlaced(in context, in placedResult);
            return placedResult;
        }

        /// <summary>
        ///     Recreates a building from saved world state without consuming resources.
        /// </summary>
        internal static OperationResult TryRestore(
            in BuildingPlacementContext context,
            [CanBeNull] out BuildingBase building)
        {
            building = null;
            if (ReferenceEquals(context.entry, null)) return BuildingOperations.EntryIsNull();
            if (!context.entry) return BuildingOperations.EntryIsNull();

            BuildingEntryBase entry = context.entry;
            BuildingBase prefab = entry.GetPrefab();
            if (ReferenceEquals(prefab, null) || !prefab)
            {
                OperationResult result = BuildingOperations.PrefabMissing();
                NotifyPlacementFailed(in context, in result);
                return result;
            }

            OperationResult slotsResult = ValidateSlots(prefab, context.slots);
            if (!slotsResult)
            {
                NotifyPlacementFailed(in context, in slotsResult);
                return slotsResult;
            }

            BuildingBase instance = Object.Instantiate(prefab, context.position, context.rotation, context.parent);
            instance.Initialize(entry);
            if (!instance.TryAssignSlots(context.slots))
            {
                BuildingRegistry.UnregisterBuilding(instance);
                DestroyGameObject(instance.gameObject);
                OperationResult result = BuildingOperations.SlotOccupied();
                NotifyPlacementFailed(in context, in result);
                return result;
            }

            building = instance;
            OperationResult placedResult = BuildingOperations.Placed();
            entry.OnBuildingPlaced(in context, instance, in placedResult);
            instance.OnBuildingPlaced(in context, in placedResult);
            return placedResult;
        }

        /// <summary>
        ///     Attempts to demolish this building while applying its resource transaction.
        /// </summary>
        public OperationResult TryDemolish(
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null)
        {
            BuildingDemolitionContext context = new BuildingDemolitionContext(this, user, raycaster);
            OperationResult result = CanDemolish(in context);
            if (!result)
            {
                NotifyDemolitionFailed(in context, in result);
                return result;
            }

            BuildingEntryBase entry = _entry;
            result = entry.TryRefundResources(in context);
            if (!result)
            {
                NotifyDemolitionFailed(in context, in result);
                return result;
            }

            ReleaseOccupiedSlots();
            BuildingRegistry.UnregisterBuilding(this);
            OperationResult demolishedResult = BuildingOperations.Demolished();
            entry.OnBuildingDemolished(in context, in demolishedResult);
            OnBuildingDemolished(in context, in demolishedResult);
            DestroyGameObject(gameObject);
            return demolishedResult;
        }

        /// <summary>
        ///     Removes this building while applying a save file, without refunding resources.
        /// </summary>
        internal void ClearForSave(in BuildingDemolitionContext context)
        {
            ReleaseOccupiedSlots();
            BuildingRegistry.UnregisterBuilding(this);
            OperationResult demolishedResult = BuildingOperations.Demolished();
            if (!ReferenceEquals(_entry, null) && _entry)
                _entry.OnBuildingDemolished(in context, in demolishedResult);
            OnBuildingDemolished(in context, in demolishedResult);
            DestroyGameObject(gameObject);
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
            BuildingRegistry.UnregisterBuilding(this);
        }

        internal void Initialize([NotNull] BuildingEntryBase entry)
        {
            _entry = entry;
            BuildingRegistry.RegisterBuilding(this, entry);
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

        private OperationResult CanDemolish(in BuildingDemolitionContext context)
        {
            if (!this) return BuildingOperations.BuildingAlreadyDestroyed();
            if (ReferenceEquals(_entry, null) || !_entry) return BuildingOperations.EntryMissing();

            OperationResult result = _entry.CanDemolish(in context);
            if (!result) return result;
            return CanBeDemolished(in context);
        }

        private static OperationResult ValidateSlots(
            [NotNull] BuildingBase prefab,
            [CanBeNull] IReadOnlyList<BuildingSlot> slots)
        {
            if (prefab is not ISlotBuilding slotBuilding) return BuildingOperations.Permitted();
            if (slotBuilding.SlotCount <= 0) return BuildingOperations.SlotCountInvalid();
            if (ReferenceEquals(slots, null) || slots.Count != slotBuilding.SlotCount)
                return BuildingOperations.SlotMissing();

            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                BuildingSlot slot = slots[slotIndex];
                if (ReferenceEquals(slot, null) || !slot) return BuildingOperations.SlotMissing();
                if (slot.IsOccupied) return BuildingOperations.SlotOccupied();

                for (int previousSlotIndex = 0; previousSlotIndex < slotIndex; previousSlotIndex++)
                {
                    if (ReferenceEquals(slot, slots[previousSlotIndex]))
                        return BuildingOperations.SlotMissing();
                }
            }

            return BuildingOperations.Permitted();
        }

        private static void NotifyPlacementFailed(
            in BuildingPlacementContext context,
            in OperationResult result)
        {
            if (ReferenceEquals(context.entry, null) || !context.entry) return;
            context.entry.OnBuildingPlacementFailed(in context, in result);
        }

        private void NotifyDemolitionFailed(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
            if (ReferenceEquals(_entry, null) || !_entry) return;
            _entry.OnBuildingDemolitionFailed(in context, in result);
            OnBuildingDemolitionFailed(in context, in result);
        }

        private static void DestroyGameObject([NotNull] GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(gameObject);
                return;
            }
#endif

            Object.Destroy(gameObject);
        }
    }
}
