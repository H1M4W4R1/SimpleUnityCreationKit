using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Utility
{
    /// <summary>
    ///     Static runtime index of API-placed buildings, available entries, and active building slots.
    /// </summary>
    /// <remarks>
    ///     This registry owns no GameObject lifetime. Buildings and slots register from their own lifecycle methods,
    ///     while host code registers entries before loading a building save.
    /// </remarks>
    internal static class BuildingRegistry
    {
        [NotNull] private static readonly List<BuildingBase> Buildings = new List<BuildingBase>();
        [NotNull] private static readonly List<BuildingEntryBase> Entries = new List<BuildingEntryBase>();
        [NotNull] private static readonly List<BuildingSlot> Slots = new List<BuildingSlot>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Buildings.Clear();
            Entries.Clear();
            Slots.Clear();
        }

        internal static void RegisterBuilding(
            [NotNull] BuildingBase building,
            [NotNull] BuildingEntryBase entry)
        {
            RegisterEntry(entry);
            if (!Buildings.Contains(building)) Buildings.Add(building);
        }

        internal static void UnregisterBuilding([NotNull] BuildingBase building)
        {
            Buildings.Remove(building);
        }

        internal static void RegisterEntry([CanBeNull] BuildingEntryBase entry)
        {
            if (ReferenceEquals(entry, null) || !entry) return;
            if (!Entries.Contains(entry)) Entries.Add(entry);
        }

        internal static void RegisterSlot([CanBeNull] BuildingSlot slot)
        {
            if (ReferenceEquals(slot, null) || !slot) return;
            if (!Slots.Contains(slot)) Slots.Add(slot);
        }

        internal static void UnregisterSlot([NotNull] BuildingSlot slot)
        {
            Slots.Remove(slot);
        }

        /// <summary>
        ///     Re-registers active scene slots after a domain reset that can precede their normal lifecycle callbacks.
        /// </summary>
        internal static void RegisterActiveSlots()
        {
            BuildingSlot[] activeSlots = Object.FindObjectsByType<BuildingSlot>();
            for (int slotIndex = 0; slotIndex < activeSlots.Length; slotIndex++)
                RegisterSlot(activeSlots[slotIndex]);
        }

        internal static void CopyBuildings([NotNull] List<BuildingBase> destination)
        {
            destination.Clear();
            for (int buildingIndex = Buildings.Count - 1; buildingIndex >= 0; buildingIndex--)
            {
                BuildingBase building = Buildings[buildingIndex];
                if (!building)
                {
                    Buildings.RemoveAt(buildingIndex);
                    continue;
                }

                destination.Add(building);
            }
        }

        internal static bool TryGetEntry(
            [CanBeNull] string saveIdentifier,
            [NotNull] out BuildingEntryBase entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(saveIdentifier)) return false;

            for (int entryIndex = Entries.Count - 1; entryIndex >= 0; entryIndex--)
            {
                BuildingEntryBase candidate = Entries[entryIndex];
                if (!candidate)
                {
                    Entries.RemoveAt(entryIndex);
                    continue;
                }

                if (!string.Equals(candidate.SaveIdentifier, saveIdentifier, StringComparison.Ordinal)) continue;
                entry = candidate;
                return true;
            }

            return false;
        }

        internal static bool TryGetSlot(
            [CanBeNull] string saveIdentifier,
            [NotNull] out BuildingSlot slot)
        {
            slot = null;
            if (string.IsNullOrWhiteSpace(saveIdentifier)) return false;

            for (int slotIndex = Slots.Count - 1; slotIndex >= 0; slotIndex--)
            {
                BuildingSlot candidate = Slots[slotIndex];
                if (!candidate)
                {
                    Slots.RemoveAt(slotIndex);
                    continue;
                }

                if (!string.Equals(candidate.SaveIdentifier, saveIdentifier, StringComparison.Ordinal)) continue;
                slot = candidate;
                return true;
            }

            return false;
        }
    }
}
