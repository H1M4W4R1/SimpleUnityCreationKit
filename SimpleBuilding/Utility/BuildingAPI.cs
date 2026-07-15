using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data;
using Systems.SimpleBuilding.Data.SaveFiles;
using Systems.SimpleSaving.Abstract;
using Systems.SimpleSaving.Utility;
using Systems.SimpleCore.Storage.Lists;

namespace Systems.SimpleBuilding.Utility
{
    /// <summary>
    ///     Provides static building-entry registration and save-pipeline integration.
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
        ///     Gets the first currently placed building assignable to <typeparamref name="TBuilding"/>.
        /// </summary>
        /// <remarks>
        ///     Only buildings created or restored through <see cref="BuildingBase"/> are registered.
        /// </remarks>
        [CanBeNull]
        public static TBuilding GetFirstBuildingOfType<TBuilding>()
            where TBuilding : BuildingBase
            => BuildingRegistry.GetFirstBuildingOfType<TBuilding>();

        /// <summary>
        ///     Gets all currently placed buildings assignable to <typeparamref name="TBuilding"/>.
        /// </summary>
        /// <remarks>
        ///     Release the returned access after reading its list. Only buildings created or restored through
        ///     <see cref="BuildingBase"/> are registered.
        /// </remarks>
        public static ROListAccess<TBuilding> GetAllBuildingsOfType<TBuilding>()
            where TBuilding : BuildingBase
        {
            RWListAccess<TBuilding> buildings = RWListAccess<TBuilding>.Create();
            BuildingRegistry.CopyBuildingsOfType(buildings.List);
            return buildings.ToReadOnly();
        }

        /// <summary>
        ///     Saves all placed buildings through the SimpleSaving API.
        /// </summary>
        /// <returns>An in-memory building save file, or <c>null</c> if serialization fails.</returns>
        [CanBeNull]
        public static SaveFileBase SaveToMemory()
        {
            BuildingSaveData saveData = new BuildingSaveData();
            return SaveAPI.Save(saveData);
        }

        /// <summary>
        ///     Restores placed buildings from a SimpleSaving save file.
        /// </summary>
        /// <remarks>
        ///     Register all available entries with <see cref="RegisterEntry"/> or <see cref="RegisterEntries"/>
        ///     before loading. Active scene slots register themselves automatically.
        /// </remarks>
        /// <param name="saveFile">Save file previously produced by <see cref="SaveToMemory"/>.</param>
        public static void Load([NotNull] SaveFileBase saveFile)
        {
            BuildingSaveData saveData = new BuildingSaveData();
            SaveAPI.Load(saveData, saveFile);
        }
    }
}
