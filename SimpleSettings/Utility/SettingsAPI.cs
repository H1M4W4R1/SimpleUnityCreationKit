using JetBrains.Annotations;
using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using UnityEngine;

namespace Systems.SimpleSettings.Utility
{
    /// <summary>
    ///     Static facade for the most common settings operations.
    ///     All methods delegate to <see cref="SettingsManager.Instance"/>.
    /// </summary>
    /// <remarks>
    ///     Methods with no <c>groupId</c> parameter (or <c>null</c>) operate on
    ///     all registered groups. Methods with a <c>groupId</c> target a single group.
    /// </remarks>
    public static class SettingsAPI
    {
        // ─────────────────────── Apply ────────────────────────────────────

        /// <summary>Applies all setting groups.</summary>
        public static void ApplyAll()
        {
            if (Manager) Manager.ApplyAll();
        }

        /// <summary>Applies the group identified by <paramref name="groupId"/>.</summary>
        public static void Apply([NotNull] string groupId)
        {
            if (Manager) Manager.ApplyGroup(groupId);
        }

        // ─────────────────────── Revert ───────────────────────────────────

        /// <summary>Reverts all setting groups to their last applied values.</summary>
        public static void RevertAll()
        {
            if (Manager) Manager.RevertAll();
        }

        /// <summary>Reverts the group identified by <paramref name="groupId"/>.</summary>
        public static void Revert([NotNull] string groupId)
        {
            if (Manager) Manager.RevertGroup(groupId);
        }

        // ─────────────────────── Reset ────────────────────────────────────

        /// <summary>Resets all setting groups to their factory defaults.</summary>
        public static void ResetAll()
        {
            if (Manager) Manager.ResetAll();
        }

        /// <summary>Resets the group identified by <paramref name="groupId"/> to factory defaults.</summary>
        public static void ResetToDefaults([NotNull] string groupId)
        {
            if (Manager) Manager.ResetGroup(groupId);
        }

        // ─────────────────────── Undo ─────────────────────────────────────

        /// <summary>
        ///     Undoes the most recent unapplied change across all groups.
        /// </summary>
        /// <returns><c>true</c> if a change was undone.</returns>
        public static bool TryUndoAll() => Manager && Manager.TryUndoAll();

        /// <summary>
        ///     Undoes the most recent unapplied change in the group identified by
        ///     <paramref name="groupId"/>.
        /// </summary>
        /// <returns><c>true</c> if a change was undone.</returns>
        public static bool TryUndo([NotNull] string groupId) =>
            Manager && Manager.TryUndoGroup(groupId);

        // ─────────────────────── Save / Load ──────────────────────────────

        /// <summary>Saves all registered groups to disk.</summary>
        public static void SaveAll()
        {
            if (Manager) Manager.SaveAll();
        }

        /// <summary>Saves the group identified by <paramref name="groupId"/> to disk.</summary>
        public static void Save([NotNull] string groupId)
        {
            if (Manager) Manager.SaveGroup(groupId);
        }

        /// <summary>Loads all registered groups from disk.</summary>
        public static void LoadAll()
        {
            if (Manager) Manager.LoadAll();
        }

        /// <summary>Loads the group identified by <paramref name="groupId"/> from disk.</summary>
        public static void Load([NotNull] string groupId)
        {
            if (Manager) Manager.LoadGroup(groupId);
        }

        // ─────────────────────── Lookup ───────────────────────────────────

        /// <summary>
        ///     Returns the setting whose concrete type is exactly
        ///     <typeparamref name="TSetting"/>. This is the preferred lookup method
        ///     because keys are automatically derived from the type name, guaranteeing
        ///     uniqueness.
        /// </summary>
        /// <returns>The setting, or <c>null</c> if not registered.</returns>
        [CanBeNull] public static TSetting GetSetting<TSetting>()
            where TSetting : class, ISetting
        {
            if (Manager == null) return null;
            foreach (SettingGroupBase group in Manager.Groups)
                foreach (ISetting setting in GetSettingsFromGroup(group))
                    if (setting is TSetting typed) return typed;
            return null;
        }

        /// <summary>
        ///     Finds a setting by <paramref name="groupId"/> and <paramref name="key"/>.
        ///     Prefer <see cref="GetSetting{TSetting}"/> when the concrete type is known.
        /// </summary>
        /// <returns>The setting, or <c>null</c> if not found.</returns>
        [CanBeNull] public static ISetting FindSetting([NotNull] string groupId,
                                                        [NotNull] string key)
        {
            if (Manager == null) return null;
            SettingGroupBase group = Manager.GetGroup(groupId);
            if (group == null) return null;

            foreach (ISetting setting in GetSettingsFromGroup(group))
                if (setting.Key == key) return setting;

            return null;
        }

        // ─────────────────────── Helpers ──────────────────────────────────

        [CanBeNull] private static SettingsManager Manager
        {
            get
            {
                if (SettingsManager.Instance == null)
                    Debug.LogWarning("[SimpleSettings] SettingsAPI called but " +
                                     "SettingsManager.Instance is null.");
                return SettingsManager.Instance;
            }
        }

        private static System.Collections.Generic.IEnumerable<ISetting>
            GetSettingsFromGroup([NotNull] SettingGroupBase group) => group.GetAllSettings();
    }
}
