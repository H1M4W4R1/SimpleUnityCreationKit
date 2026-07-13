#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleFactions.Abstract;
using Systems.SimpleFactions.Interfaces;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[assembly: InternalsVisibleTo("SimpleFactions.Tests")]
namespace Systems.SimpleFactions.Editor.Automation
{
    /// <summary>
    ///     Editor utility that automatically assigns <see cref="ReputationLevelBase"/> assets to
    ///     their target <see cref="FactionBase"/> when they implement
    ///     <see cref="IForFaction{TFaction}"/>.
    ///     <para>
    ///         Runs on script reload (<see cref="DidReloadScripts"/>) and after assets are
    ///         imported or deleted (<see cref="AssetPostprocessor"/>).
    ///     </para>
    ///     <para>
    ///         <see cref="AssetDatabase.SaveAssets"/> must never be called from within
    ///         <see cref="OnPostprocessAllAssets"/> because Unity re-imports any modified
    ///         <c>.asset</c> files after a save, which would re-trigger
    ///         <see cref="OnPostprocessAllAssets"/> and cause an infinite import loop.
    ///         That path only marks assets dirty; the save is deferred to the script-reload
    ///         callbacks that run outside the import pipeline.
    ///     </para>
    /// </summary>
    [InitializeOnLoad]
    public sealed class FactionLevelAssigner : AssetPostprocessor
    {
        // Guards against re-entrant calls from the save path:
        // SaveAssets() re-imports modified .asset files, which would trigger
        // OnPostprocessAllAssets again. The guard prevents that second call from
        // entering AssignAll while a save is already in flight.
        private static bool _isAssigning;

        static FactionLevelAssigner()
        {
            // Run once on domain reload so levels are wired when entering Play Mode.
            AssignAll(save: true);
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded() => AssignAll(save: true);

        // Called by Unity after any asset import / deletion / move.
        // IMPORTANT: must not call SaveAssets() here — see class-level remarks.
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool relevant = false;

            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (!importedAssets[i].EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;
                relevant = true;
                break;
            }

            if (!relevant && deletedAssets.Length > 0) relevant = true;

            if (relevant) AssignAll(save: false);
        }

        /// <summary>
        ///     Scans all <see cref="ReputationLevelBase"/> assets in the project, checks whether
        ///     each implements <see cref="IForFaction{TFaction}"/>, and adds it to the target
        ///     faction's level list if not already present. Levels not implementing the interface
        ///     are left untouched (they may still be assigned manually in the Inspector).
        /// </summary>
        /// <param name="save">
        ///     When <c>true</c>, calls <see cref="AssetDatabase.SaveAssets"/> after marking dirty.
        ///     Pass <c>false</c> from inside <see cref="OnPostprocessAllAssets"/> to avoid the
        ///     re-import loop.
        /// </param>
        private static void AssignAll(bool save)
        {
            if (_isAssigning) return;
            _isAssigning = true;

            try
            {
                bool anyDirty = AssignAllInternal();
                if (anyDirty && save)
                    AssetDatabase.SaveAssets();
            }
            finally
            {
                _isAssigning = false;
            }
        }

        private static bool AssignAllInternal()
        {
            string[] guids = AssetDatabase.FindAssets("t:ReputationLevelBase");
            if (guids.Length == 0) return false;

            bool anyDirty = false;

            for (int g = 0; g < guids.Length; g++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
                ReputationLevelBase level = AssetDatabase.LoadAssetAtPath<ReputationLevelBase>(assetPath);

                if (ReferenceEquals(level, null)) continue;

                Type levelType = level.GetType();

                // Find all IForFaction<TFaction> interfaces implemented by this concrete type.
                Type[] interfaces = levelType.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    Type iface = interfaces[i];
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() != typeof(IForFaction<>)) continue;

                    Type factionType = iface.GetGenericArguments()[0];
                    FactionBase faction = FindFactionAsset(factionType);

                    if (ReferenceEquals(faction, null))
                    {
                        Debug.LogWarning(
                            $"[SimpleFactions] {levelType.Name} implements IForFaction<{factionType.Name}> " +
                            $"but no asset of type {factionType.Name} was found in the project.");
                        continue;
                    }

                    if (!faction.AssignLevel(level)) continue;

                    EditorUtility.SetDirty(faction);
                    anyDirty = true;
                }
            }

            return anyDirty;
        }

        internal static bool AssignAllForTests()
        {
            return AssignAllInternal();
        }

        [CanBeNull]
        private static FactionBase FindFactionAsset([NotNull] Type factionType)
        {
            string[] factionGuids = AssetDatabase.FindAssets($"t:{factionType.Name}");
            for (int i = 0; i < factionGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(factionGuids[i]);
                FactionBase candidate = AssetDatabase.LoadAssetAtPath<FactionBase>(path);
                if (!ReferenceEquals(candidate, null) && candidate.GetType() == factionType)
                    return candidate;
            }

            return null;
        }
    }
}
#endif
