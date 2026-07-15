using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Data.SaveFiles;
using Systems.SimpleRelations.Abstract;
using Systems.SimpleRelations.Data;
using Systems.SimpleRelations.Utility;
using Systems.SimpleSaving.Abstract;
using UnityEngine;

namespace Systems.SimpleFactions.Data
{
    /// <summary>SimpleSaving adapter for persistent faction-to-faction and faction-to-runtime-object relations.</summary>
    internal sealed class FactionRelationSaveData : ISaveData<FactionRelationSaveFile>
    {
        [CanBeNull] private FactionRelationSaveFile _loadedSaveFile;
        [NotNull] private readonly List<FactionRelationSaveEntry> _collectedEntries =
            new List<FactionRelationSaveEntry>();
        [NotNull] private readonly HashSet<ulong> _collectedFactionTypeHashes = new HashSet<ulong>();

        /// <inheritdoc />
        public void CollectData()
        {
            _collectedEntries.Clear();
            _collectedFactionTypeHashes.Clear();
            ROListAccess<FactionBase> factionAccess = FactionDatabase.GetAll<FactionBase>();
            IReadOnlyList<FactionBase> factions = factionAccess.List;
            for (int factionIndex = 0; factionIndex < factions.Count; factionIndex++)
            {
                FactionBase sourceFaction = factions[factionIndex];
                if (ReferenceEquals(sourceFaction, null) || !sourceFaction) continue;
                ulong sourceFactionTypeHash = HashIdentifier.New(sourceFaction.GetType()).Value;
                if (!_collectedFactionTypeHashes.Add(sourceFactionTypeHash)) continue;

                IRelatable source = sourceFaction;
                IReadOnlyList<RelationEntry> relations = source.Relations;
                for (int relationIndex = 0; relationIndex < relations.Count; relationIndex++)
                {
                    RelationEntry relation = relations[relationIndex];
                    if (ReferenceEquals(relation, null)) continue;

                    RelationTypeBase relationType = relation.RelationType;
                    if (ReferenceEquals(relationType, null) || !relationType) continue;
                    FactionRelationSaveEntry entry = new FactionRelationSaveEntry
                    {
                        SourceFactionTypeHash = sourceFactionTypeHash,
                        RelationTypeHash = HashIdentifier.New(relationType.GetType()).Value,
                        Value = relation.Value
                    };

                    FactionBase targetFaction = relation.Target as FactionBase;
                    if (!ReferenceEquals(targetFaction, null) && targetFaction)
                    {
                        entry.TargetKind = FactionRelationTargetKind.Faction;
                        entry.TargetFactionTypeHash = HashIdentifier.New(targetFaction.GetType()).Value;
                    }
                    else if (TryGetRuntimeIdentifier(relation.Target, out Snowflake128 targetIdentifier))
                    {
                        entry.TargetKind = FactionRelationTargetKind.RuntimeObject;
                        entry.TargetRuntimeIdentifier = targetIdentifier;
                    }
                    else
                    {
                        Debug.LogWarning(
                            "Skipped faction relation because the runtime target does not implement IIdentifiable<Snowflake128> with a created identifier.",
                            sourceFaction);
                        continue;
                    }

                    _collectedEntries.Add(entry);
                }
            }

            factionAccess.Release();
        }

        /// <inheritdoc />
        [NotNull]
        public FactionRelationSaveFile BuildSaveFile()
        {
            FactionRelationSaveFile saveFile = new FactionRelationSaveFile
            {
                Entries = _collectedEntries.ToArray()
            };
            return saveFile;
        }

        /// <inheritdoc />
        public void ParseSaveFile([NotNull] FactionRelationSaveFile saveFile)
        {
            _loadedSaveFile = saveFile;
        }

        /// <inheritdoc />
        public void DistributeData()
        {
            FactionRelationSaveFile saveFile = _loadedSaveFile;
            if (ReferenceEquals(saveFile, null)) return;

            ClearFactionRelations();
            FactionRelationSaveEntry[] entries = saveFile.Entries;
            if (!ReferenceEquals(entries, null))
            {
                for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
                    RestoreEntry(entries[entryIndex]);
            }

            _loadedSaveFile = null;
        }

        [CanBeNull]
        private static FactionBase FindFaction(ulong typeHash)
        {
            ROListAccess<FactionBase> factionAccess = FactionDatabase.GetAll<FactionBase>();
            IReadOnlyList<FactionBase> factions = factionAccess.List;
            FactionBase found = null;
            for (int factionIndex = 0; factionIndex < factions.Count; factionIndex++)
            {
                FactionBase faction = factions[factionIndex];
                if (ReferenceEquals(faction, null) || !faction) continue;
                if (HashIdentifier.New(faction.GetType()).Value != typeHash) continue;

                found = faction;
                break;
            }

            factionAccess.Release();
            return found;
        }

        [CanBeNull]
        private static RelationTypeBase FindRelationType(ulong typeHash)
        {
            ROListAccess<RelationTypeBase> access = RelationTypeDatabase.GetAll<RelationTypeBase>();
            IReadOnlyList<RelationTypeBase> relationTypes = access.List;
            RelationTypeBase found = null;
            for (int index = 0; index < relationTypes.Count; index++)
            {
                RelationTypeBase relationType = relationTypes[index];
                if (ReferenceEquals(relationType, null) || !relationType) continue;
                if (HashIdentifier.New(relationType.GetType()).Value != typeHash) continue;

                found = relationType;
                break;
            }

            access.Release();
            return found;
        }

        private static void ClearFactionRelations()
        {
            ROListAccess<FactionBase> factionAccess = FactionDatabase.GetAll<FactionBase>();
            IReadOnlyList<FactionBase> factions = factionAccess.List;
            for (int factionIndex = 0; factionIndex < factions.Count; factionIndex++)
            {
                FactionBase faction = factions[factionIndex];
                if (ReferenceEquals(faction, null) || !faction) continue;

                faction.ClearRelations();
            }

            factionAccess.Release();
        }

        private static void RestoreEntry([CanBeNull] FactionRelationSaveEntry entry)
        {
            if (ReferenceEquals(entry, null)) return;

            FactionBase sourceFaction = FindFaction(entry.SourceFactionTypeHash);
            RelationTypeBase relationType = FindRelationType(entry.RelationTypeHash);
            if (ReferenceEquals(sourceFaction, null)) return;
            if (ReferenceEquals(relationType, null)) return;

            IRelatable target;
            if (entry.TargetKind == FactionRelationTargetKind.Faction)
            {
                target = FindFaction(entry.TargetFactionTypeHash);
            }
            else if (!RelatableObjectDatabase.TryGet(entry.TargetRuntimeIdentifier, out target))
            {
                Debug.LogWarning("Skipped saved faction relation because its runtime target is not registered.");
                return;
            }

            if (ReferenceEquals(target, null)) return;
            RelationAPI.Set(sourceFaction, target, relationType, entry.Value);
        }

        private static bool TryGetRuntimeIdentifier([CanBeNull] IRelatable target, out Snowflake128 identifier)
        {
            identifier = Snowflake128.Empty;
            if (ReferenceEquals(target, null)) return false;
            if (target is not IIdentifiable<Snowflake128> identifiable) return false;

            identifier = identifiable.Identifier;
            return identifier.IsCreated;
        }
    }
}
