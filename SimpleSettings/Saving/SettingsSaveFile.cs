using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleSaving.Abstract;
using UnityEngine;

namespace Systems.SimpleSettings.Saving
{
    /// <summary>
    ///     Save file for a single <see cref="Abstract.SettingGroupBase"/>.
    ///     Contains the group ID and a flat list of key-value entries, where
    ///     each value is the string produced by <see cref="Abstract.ISetting.SerializeCurrentValue"/>.
    /// </summary>
    [Serializable]
    public sealed class SettingsSaveFile : SaveFileBase
    {
        /// <summary>ID of the group this file belongs to.</summary>
        [field: SerializeField] public string GroupId { get; set; }

        /// <summary>All serialized setting entries for this group.</summary>
        [field: SerializeField] public List<SettingEntry> Entries { get; set; } = new();

        /// <summary>
        ///     A single serialized key-value pair representing one setting.
        /// </summary>
        [Serializable]
        public sealed class SettingEntry
        {
            /// <summary>Setting key (typically the concrete class name).</summary>
            [field: SerializeField] public string Key { get; set; }

            /// <summary>Serialized string representation of the setting's value.</summary>
            [field: SerializeField] public string Value { get; set; }
        }
    }

    /// <summary>
    ///     Wrapper file that holds save data for multiple groups in a single file
    ///     (used when <c>SaveMode.SingleFile</c> is active).
    /// </summary>
    [Serializable]
    public sealed class CombinedSettingsSaveFile : SaveFileBase
    {
        /// <summary>One entry per registered group.</summary>
        [field: SerializeField] public List<SettingsSaveFile> Groups { get; set; } = new();

        /// <summary>
        ///     Tries to find the <see cref="SettingsSaveFile"/> for the specified
        ///     <paramref name="groupId"/>.
        /// </summary>
        public bool TryGetGroup([NotNull] string groupId,
                                [CanBeNull] out SettingsSaveFile file)
        {
            foreach (SettingsSaveFile g in Groups)
            {
                if (g.GroupId != groupId) continue;
                file = g;
                return true;
            }

            file = null;
            return false;
        }

        /// <summary>
        ///     Adds or replaces the entry for <paramref name="file"/>'s group.
        /// </summary>
        public void SetGroup([NotNull] SettingsSaveFile file)
        {
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].GroupId != file.GroupId) continue;
                Groups[i] = file;
                return;
            }

            Groups.Add(file);
        }
    }
}
