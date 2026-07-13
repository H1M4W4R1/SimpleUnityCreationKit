using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleCore.Saving.Abstract;
using Systems.SimpleSettings.Saving;
using Systems.SimpleSettings.Utility;

namespace Systems.SimpleSettings.Abstract
{
    /// <summary>
    ///     Abstract base for a logical group of related settings
    ///     (e.g. Graphics, Audio, Controls).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Subclasses must call <see cref="RegisterSettings"/> from their constructor
    ///         after all <see cref="ISetting"/> instances are created.
    ///     </para>
    ///     <para>
    ///         Implements <see cref="ISaveData{SettingsSaveFile}"/> so it participates
    ///         in the SimpleCore save pipeline. Each setting's value is serialized as a
    ///         string via <see cref="ISetting.SerializeCurrentValue"/>.
    ///     </para>
    ///     <para>
    ///         Group-level undo is an ordered <see cref="Stack{ISetting}"/>: every
    ///         <see cref="Setting{TValue}.Set"/> call pushes the owning setting onto
    ///         this stack. <see cref="TryUndoLastChange"/> pops and undoes the most
    ///         recently changed setting.
    ///     </para>
    /// </remarks>
    public abstract class SettingGroupBase : ISaveData<SettingsSaveFile>
    {
        private readonly Stack<ISetting> _orderedUndoStack = new();

        // ──────────────────────── Identity ────────────────────────────────

        /// <summary>
        ///     Unique identifier for this group (e.g. <c>"graphics"</c>).
        ///     Must be stable across sessions as it is used for save-file lookup.
        /// </summary>
        [NotNull] public abstract string GroupId { get; }

        /// <summary>
        ///     File name (without extension) used when saving this group.
        ///     Defaults to <c>"settings"</c> (all groups share one file).
        ///     Override for dedicated per-group files (e.g. <c>"audio_settings"</c>).
        /// </summary>
        [NotNull] public virtual string SaveFileName => "settings";

        // ──────────────────────── Registration ───────────────────────────

        /// <summary>Returns all settings that belong to this group.</summary>
        [NotNull] protected abstract IEnumerable<ISetting> GetSettings();

        /// <summary>
        ///     Public accessor for all settings in this group.
        ///     Delegates to <see cref="GetSettings"/>; used by <see cref="SettingsAPI"/>.
        /// </summary>
        [NotNull] public IEnumerable<ISetting> GetAllSettings() => GetSettings();

        /// <summary>
        ///     Registers <paramref name="settings"/> with this group:
        ///     assigns <see cref="ISetting.GroupId"/> and wires the ordered-undo callback.
        ///     Call this from the subclass constructor after all settings are created.
        /// </summary>
        protected void RegisterSettings([NotNull] IEnumerable<ISetting> settings)
        {
            foreach (ISetting setting in settings)
                setting.InitializeForGroup(GroupId, s => _orderedUndoStack.Push(s));
        }

        // ─────────────────────── Bulk operations ──────────────────────────

        /// <summary>Applies all settings in this group and clears the undo stack.</summary>
        public void Apply()
        {
            foreach (ISetting setting in GetSettings())
                setting.Apply();
            _orderedUndoStack.Clear();
            OnGroupApplied();
        }

        /// <summary>Reverts all settings in this group to their last applied values.</summary>
        public void Revert()
        {
            foreach (ISetting setting in GetSettings())
                setting.Revert();
            _orderedUndoStack.Clear();
        }

        /// <summary>Resets all settings in this group to their factory defaults.</summary>
        public void ResetToDefaults()
        {
            foreach (ISetting setting in GetSettings())
                setting.ResetToDefault();
        }

        /// <summary>
        ///     Undoes the most recent unapplied change across all settings in this group.
        /// </summary>
        /// <returns><c>true</c> if a change was undone; <c>false</c> if nothing to undo.</returns>
        public bool TryUndoLastChange()
        {
            while (_orderedUndoStack.TryPop(out ISetting setting))
                if (setting.TryUndo()) return true;
            return false;
        }

        /// <summary>Whether any setting in this group has unapplied changes.</summary>
        public bool IsDirty
        {
            get
            {
                foreach (ISetting setting in GetSettings())
                    if (setting.IsDirty) return true;
                return false;
            }
        }

        /// <summary>
        ///     Called after <see cref="Apply"/> completes.
        ///     Override for group-level side effects.
        /// </summary>
        protected virtual void OnGroupApplied() { }

        // ─────────────────── ISaveData<SettingsSaveFile> ──────────────────

        /// <summary>No-op — settings already hold their current values.</summary>
        public void CollectData() { }

        /// <summary>
        ///     Serializes all settings' keys and values into a new
        ///     <see cref="SettingsSaveFile"/>.
        /// </summary>
        public SettingsSaveFile BuildSaveFile()
        {
            SettingsSaveFile file = new() { GroupId = GroupId };

            foreach (ISetting setting in GetSettings())
            {
                string serialized = setting.SerializeCurrentValue();
                if (serialized == null) continue;

                file.Entries.Add(new SettingsSaveFile.SettingEntry
                {
                    Key   = setting.Key,
                    Value = serialized,
                });
            }

            return file;
        }

        /// <summary>
        ///     Loads each entry from <paramref name="saveFile"/> into the matching setting.
        ///     Uses <see cref="ISetting.DeserializeAndLoad"/> which bypasses the undo stack
        ///     and immediately applies the engine effect.
        /// </summary>
        public void ParseSaveFile(SettingsSaveFile saveFile)
        {
            Dictionary<string, ISetting> byKey = new();
            foreach (ISetting setting in GetSettings())
                byKey[setting.Key] = setting;

            foreach (SettingsSaveFile.SettingEntry entry in saveFile.Entries)
            {
                if (!byKey.TryGetValue(entry.Key, out ISetting setting)) continue;
                setting.DeserializeAndLoad(entry.Value);
            }
        }

        /// <summary>No-op — <see cref="ISetting.DeserializeAndLoad"/> applies effects during parse.</summary>
        public void DistributeData() { }
    }
}
