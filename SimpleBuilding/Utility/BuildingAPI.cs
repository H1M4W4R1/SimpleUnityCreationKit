using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Data;
using Systems.SimpleBuilding.Data.SaveFiles;
using Systems.SimpleCore.Saving.Abstract;
using Systems.SimpleCore.Saving.Utility;

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
        ///     Saves all placed buildings through the SimpleCore save API.
        /// </summary>
        /// <returns>An in-memory building save file, or <c>null</c> if serialization fails.</returns>
        [CanBeNull]
        public static SaveFileBase SaveToMemory()
        {
            BuildingSaveData saveData = new BuildingSaveData();
            return SaveAPI.Save(saveData);
        }

        /// <summary>
        ///     Restores placed buildings from a SimpleCore save file.
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
