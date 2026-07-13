using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Data.SaveFiles;
using Systems.SimpleCore.Saving.Abstract;
using Systems.SimpleCore.Saving.Utility;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Utility.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Utility
{
    /// <summary>
    ///     Coordinates validation, resource transactions, instantiation, slot reservation, and demolition.
    /// </summary>
    public static class BuildingAPI
    {
        /// <summary>
        ///     Registers an entry so saved buildings can resolve it during a later load.
        /// </summary>
        /// <param name="entry">Entry with a unique <see cref="BuildingEntryBase.SaveIdentifier"/>.</param>
        public static void RegisterEntry([NotNull] BuildingEntryBase entry)
            => BuildingRegistry.RegisterEntry(entry);

        /// <summary>
        ///     Registers entries so saved buildings can resolve them during a later load.
        /// </summary>
        /// <param name="entries">Entries with unique <see cref="BuildingEntryBase.SaveIdentifier"/> values.</param>
        public static void RegisterEntries([NotNull] IReadOnlyList<BuildingEntryBase> entries)
        {
            if (ReferenceEquals(entries, null)) return;

            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
                BuildingRegistry.RegisterEntry(entries[entryIndex]);
        }

        /// <summary>
        ///     Saves all API-placed buildings through the SimpleCore save API.
        /// </summary>
        /// <returns>An in-memory building save file, or <c>null</c> if serialization fails.</returns>
        [CanBeNull]
        public static SaveFileBase SaveToMemory()
        {
            BuildingSaveData saveData = new BuildingSaveData();
            return SaveAPI.Save(saveData);
        }

        /// <summary>
        ///     Restores API-placed buildings from a SimpleCore save file.
        /// </summary>
        /// <remarks>
        ///     Register all available entries with <see cref="RegisterEntry"/> or <see cref="RegisterEntries"/>
        ///     before loading. Active <see cref="BuildingSlot"/> components register themselves automatically.
        /// </remarks>
        /// <param name="saveFile">Save file previously produced by <see cref="SaveToMemory"/>.</param>
        public static void Load([NotNull] SaveFileBase saveFile)
        {
            BuildingSaveData saveData = new BuildingSaveData();
            SaveAPI.Load(saveData, saveFile);
        }

        public static OperationResult CanSelect(
            [CanBeNull] BuildingEntryBase entry,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingSelectionContext context = new BuildingSelectionContext(entry, user, raycaster, actionSource);
            return CanSelect(in context);
        }

        public static OperationResult CanSelect(in BuildingSelectionContext context)
        {
            if (ReferenceEquals(context.entry, null)) return BuildingOperations.EntryIsNull();
            if (!context.entry) return BuildingOperations.EntryIsNull();
            BuildingRegistry.RegisterEntry(context.entry);
            return context.entry.IsAvailable(in context);
        }

        public static OperationResult CanBuild(
            [CanBeNull] BuildingEntryBase entry,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            [CanBeNull] Transform parent = null,
            [CanBeNull] IReadOnlyList<BuildingSlot> slots = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingPlacementContext context = new BuildingPlacementContext(
                entry, position, rotation, user, raycaster, parent, slots, actionSource);
            return CanBuild(in context);
        }

        public static OperationResult CanBuild(in BuildingPlacementContext context)
        {
            if (ReferenceEquals(context.entry, null)) return BuildingOperations.EntryIsNull();
            if (!context.entry) return BuildingOperations.EntryIsNull();

            BuildingEntryBase entry = context.entry;
            BuildingRegistry.RegisterEntry(entry);
            BuildingBase prefab = entry.GetPrefab();
            if (ReferenceEquals(prefab, null) || !prefab) return BuildingOperations.PrefabMissing();

            BuildingSelectionContext selectionContext = new BuildingSelectionContext(
                entry, context.user, context.raycaster, context.actionSource);
            OperationResult result = entry.IsAvailable(in selectionContext);
            if (!result) return result;

            result = ValidateSlots(prefab, context.slots);
            if (!result) return result;

            return entry.CanBuild(in context);
        }

        public static OperationResult TryBuild(
            [CanBeNull] BuildingEntryBase entry,
            Vector3 position,
            Quaternion rotation,
            [CanBeNull] out BuildingBase building,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            [CanBeNull] Transform parent = null,
            [CanBeNull] IReadOnlyList<BuildingSlot> slots = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingPlacementContext context = new BuildingPlacementContext(
                entry, position, rotation, user, raycaster, parent, slots, actionSource);
            return TryBuild(in context, out building);
        }

        public static OperationResult TryBuild(
            in BuildingPlacementContext context,
            [CanBeNull] out BuildingBase building)
        {
            building = null;
            OperationResult result = CanBuild(in context);
            if (!result)
            {
                NotifyPlacementFailed(in context, result);
                return result;
            }

            BuildingEntryBase entry = context.entry;
            result = entry.TryConsumeResources(in context);
            if (!result)
            {
                NotifyPlacementFailed(in context, result);
                return result;
            }

            BuildingBase prefab = entry.GetPrefab();
            BuildingBase instance = Object.Instantiate(prefab, context.position, context.rotation, context.parent);
            instance.Initialize(entry);

            if (!instance.TryAssignSlots(context.slots))
            {
                BuildingDemolitionContext refundContext = new BuildingDemolitionContext(
                    instance, context.user, context.raycaster, context.actionSource);
                OperationResult refundResult = entry.TryRefundResources(in refundContext);
                DestroyGameObject(instance.gameObject);
                OperationResult finalResult = refundResult
                    ? BuildingOperations.SlotOccupied()
                    : BuildingOperations.RefundFailed();
                NotifyPlacementFailed(in context, finalResult);
                return finalResult;
            }

            building = instance;
            OperationResult placedResult = BuildingOperations.Placed();
            entry.OnBuildingPlaced(in context, instance, placedResult);
            instance.OnBuildingPlaced(in context, placedResult);
            return placedResult;
        }

        /// <summary>
        ///     Recreates a building from saved world state without consuming resources or invoking placement callbacks.
        /// </summary>
        /// <remarks>
        ///     This is reserved for the building save adapter so loading a save cannot be treated as a new
        ///     player placement by game-specific rules.
        /// </remarks>
        internal static OperationResult TryRestore(
            in BuildingPlacementContext context,
            [CanBeNull] out BuildingBase building)
        {
            building = null;
            if (ReferenceEquals(context.entry, null)) return BuildingOperations.EntryIsNull();
            if (!context.entry) return BuildingOperations.EntryIsNull();

            BuildingEntryBase entry = context.entry;
            BuildingBase prefab = entry.GetPrefab();
            if (ReferenceEquals(prefab, null) || !prefab) return BuildingOperations.PrefabMissing();

            OperationResult slotsResult = ValidateSlots(prefab, context.slots);
            if (!slotsResult) return slotsResult;

            BuildingBase instance = Object.Instantiate(prefab, context.position, context.rotation, context.parent);
            instance.Initialize(entry);
            if (!instance.TryAssignSlots(context.slots))
            {
                DestroyGameObject(instance.gameObject);
                return BuildingOperations.SlotOccupied();
            }

            building = instance;
            return BuildingOperations.Placed();
        }

        /// <summary>
        ///     Removes a building while applying a save file without refunds or demolition callbacks.
        /// </summary>
        /// <remarks>
        ///     This is reserved for the building save adapter. Its context carries
        ///     <see cref="BuildingDemolitionContext.isSaveSystemRequest"/> to distinguish this removal from a
        ///     gameplay demolition.
        /// </remarks>
        internal static void ClearForSave(in BuildingDemolitionContext context)
        {
            if (ReferenceEquals(context.building, null) || !context.building) return;

            BuildingBase building = context.building;
            BuildingRegistry.UnregisterBuilding(building);
            building.ReleaseOccupiedSlots();
            DestroyGameObject(building.gameObject);
        }

        public static OperationResult CanDemolish(
            [CanBeNull] BuildingBase building,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingDemolitionContext context = new BuildingDemolitionContext(building, user, raycaster, actionSource);
            return CanDemolish(in context);
        }

        public static OperationResult CanDemolish(in BuildingDemolitionContext context)
        {
            if (ReferenceEquals(context.building, null)) return BuildingOperations.BuildingIsNull();
            if (!context.building) return BuildingOperations.BuildingAlreadyDestroyed();

            BuildingEntryBase entry = context.building.Entry;
            if (ReferenceEquals(entry, null) || !entry) return BuildingOperations.EntryMissing();

            OperationResult result = entry.CanDemolish(in context);
            if (!result) return result;
            return context.building.CanBeDemolished(in context);
        }

        public static OperationResult TryDemolish(
            [CanBeNull] BuildingBase building,
            [CanBeNull] IBuildingUser user = null,
            [CanBeNull] BuildingRaycasterBase raycaster = null,
            ActionSource actionSource = ActionSource.External)
        {
            BuildingDemolitionContext context = new BuildingDemolitionContext(building, user, raycaster, actionSource);
            return TryDemolish(in context);
        }

        public static OperationResult TryDemolish(in BuildingDemolitionContext context)
        {
            OperationResult result = CanDemolish(in context);
            if (!result)
            {
                NotifyDemolitionFailed(in context, result);
                return result;
            }

            BuildingBase building = context.building;
            BuildingEntryBase entry = building.Entry;
            result = entry.TryRefundResources(in context);
            if (!result)
            {
                NotifyDemolitionFailed(in context, result);
                return result;
            }

            building.ReleaseOccupiedSlots();
            OperationResult demolishedResult = BuildingOperations.Demolished();
            entry.OnBuildingDemolished(in context, demolishedResult);
            building.OnBuildingDemolished(in context, demolishedResult);
            DestroyGameObject(building.gameObject);
            return demolishedResult;
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

        private static void NotifyPlacementFailed(in BuildingPlacementContext context, in OperationResult result)
        {
            if (ReferenceEquals(context.entry, null) || !context.entry) return;
            context.entry.OnBuildingPlacementFailed(in context, result);
        }

        private static void NotifyDemolitionFailed(in BuildingDemolitionContext context, in OperationResult result)
        {
            if (ReferenceEquals(context.building, null) || !context.building) return;

            BuildingEntryBase entry = context.building.Entry;
            if (ReferenceEquals(entry, null) || !entry) return;
            entry.OnBuildingDemolitionFailed(in context, result);
            context.building.OnBuildingDemolitionFailed(in context, result);
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
