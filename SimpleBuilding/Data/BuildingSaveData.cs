using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Data.SaveFiles;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleSaving.Abstract;
using UnityEngine;

namespace Systems.SimpleBuilding.Data
{
    /// <summary>
    ///     Transient SimpleSaving adapter for the static SimpleBuilding runtime registry.
    /// </summary>
    public sealed class BuildingSaveData : ISaveData<BuildingSaveFile>
    {
        [CanBeNull] private BuildingSaveFile _loadedSaveFile;
        [NotNull] private readonly List<BuildingBase> _registeredBuildings = new List<BuildingBase>();
        [NotNull] private readonly List<BuildingSaveFile.SavedBuilding> _collectedBuildings =
            new List<BuildingSaveFile.SavedBuilding>();
        [NotNull] private readonly List<BuildingSlot> _resolvedSlots = new List<BuildingSlot>();

        /// <inheritdoc />
        public void CollectData()
        {
            _collectedBuildings.Clear();
            BuildingRegistry.CopyBuildings(_registeredBuildings);

            for (int buildingIndex = 0; buildingIndex < _registeredBuildings.Count; buildingIndex++)
            {
                BuildingBase building = _registeredBuildings[buildingIndex];
                BuildingEntryBase entry = building.Entry;
                if (ReferenceEquals(entry, null) || !entry) continue;

                string entryIdentifier = entry.SaveIdentifier;
                if (string.IsNullOrWhiteSpace(entryIdentifier))
                {
                    Debug.LogWarning("Skipping a building with an empty save identifier.", building);
                    continue;
                }

                IReadOnlyList<BuildingSlot> occupiedSlots = building.OccupiedSlots;
                string[] slotIdentifiers = new string[occupiedSlots.Count];
                bool hasInvalidSlotIdentifier = false;
                for (int slotIndex = 0; slotIndex < occupiedSlots.Count; slotIndex++)
                {
                    BuildingSlot slot = occupiedSlots[slotIndex];
                    if (ReferenceEquals(slot, null) || !slot || string.IsNullOrWhiteSpace(slot.SaveIdentifier))
                    {
                        hasInvalidSlotIdentifier = true;
                        break;
                    }

                    slotIdentifiers[slotIndex] = slot.SaveIdentifier;
                }

                if (hasInvalidSlotIdentifier)
                {
                    Debug.LogWarning("Skipping a building with a slot that has an empty save identifier.", building);
                    continue;
                }

                Transform buildingTransform = building.transform;
                BuildingSaveFile.SavedBuilding savedBuilding = new BuildingSaveFile.SavedBuilding
                {
                    EntryIdentifier = entryIdentifier,
                    Position = buildingTransform.position,
                    Rotation = buildingTransform.rotation,
                    LocalScale = buildingTransform.localScale,
                    SlotIdentifiers = slotIdentifiers
                };
                _collectedBuildings.Add(savedBuilding);
            }
        }

        /// <inheritdoc />
        [NotNull]
        public BuildingSaveFile BuildSaveFile()
        {
            BuildingSaveFile.SavedBuilding[] buildings = _collectedBuildings.ToArray();
            BuildingSaveFile saveFile = new BuildingSaveFile { Buildings = buildings };
            return saveFile;
        }

        /// <inheritdoc />
        public void ParseSaveFile([NotNull] BuildingSaveFile saveFile)
        {
            _loadedSaveFile = saveFile;
        }

        /// <inheritdoc />
        public void DistributeData()
        {
            BuildingSaveFile saveFile = _loadedSaveFile;
            if (ReferenceEquals(saveFile, null)) return;

            BuildingRegistry.RegisterActiveSlots();
            ClearAPIPlacedBuildings();
            BuildingSaveFile.SavedBuilding[] savedBuildings = saveFile.Buildings;
            if (ReferenceEquals(savedBuildings, null)) return;

            for (int buildingIndex = 0; buildingIndex < savedBuildings.Length; buildingIndex++)
            {
                BuildingSaveFile.SavedBuilding savedBuilding = savedBuildings[buildingIndex];
                if (ReferenceEquals(savedBuilding, null)) continue;

                if (!BuildingRegistry.TryGetEntry(savedBuilding.EntryIdentifier, out BuildingEntryBase entry))
                {
                    Debug.LogWarning($"Skipped saved building because entry '{savedBuilding.EntryIdentifier}' is not registered.");
                    continue;
                }

                if (!TryResolveSlots(savedBuilding.SlotIdentifiers))
                {
                    Debug.LogWarning($"Skipped saved building '{savedBuilding.EntryIdentifier}' because one or more slots are not active.");
                    continue;
                }

                BuildingPlacementContext context = new BuildingPlacementContext(
                    entry,
                    savedBuilding.Position,
                    savedBuilding.Rotation,
                    slots: _resolvedSlots,
                    isSaveSystemRequest: true);
                if (!BuildingBase.TryRestore(in context, out BuildingBase building)) continue;

                building.transform.localScale = savedBuilding.LocalScale;
            }

            _loadedSaveFile = null;
        }

        private bool TryResolveSlots([CanBeNull] string[] slotIdentifiers)
        {
            _resolvedSlots.Clear();
            if (ReferenceEquals(slotIdentifiers, null)) return true;

            for (int slotIdentifierIndex = 0; slotIdentifierIndex < slotIdentifiers.Length; slotIdentifierIndex++)
            {
                if (!BuildingRegistry.TryGetSlot(slotIdentifiers[slotIdentifierIndex], out BuildingSlot slot))
                    return false;

                _resolvedSlots.Add(slot);
            }

            return true;
        }

        private void ClearAPIPlacedBuildings()
        {
            BuildingRegistry.CopyBuildings(_registeredBuildings);
            for (int buildingIndex = 0; buildingIndex < _registeredBuildings.Count; buildingIndex++)
            {
                BuildingBase building = _registeredBuildings[buildingIndex];
                BuildingDemolitionContext context = new BuildingDemolitionContext(
                    building,
                    isSaveSystemRequest: true);
                building.ClearForSave(in context);
            }

            _registeredBuildings.Clear();
        }
    }
}
