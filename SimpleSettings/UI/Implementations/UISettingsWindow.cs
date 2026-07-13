using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using Systems.SimpleSettings.Utility;
using Systems.SimpleUI.Components.Windows;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Top-level settings window.
    ///     Hosts Apply All, Revert All, Reset All, and Save action buttons as child
    ///     objects (e.g. <see cref="UISettingsApplyButton"/>, <see cref="UISettingsRevertButton"/>, etc.).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When the window is closed while settings are dirty, behaviour depends on
    ///         <see cref="_dirtyClosePolicy"/>:
    ///         <list type="bullet">
    ///             <item><see cref="DirtyClosePolicy.Revert"/> — all unapplied changes are reverted.</item>
    ///             <item><see cref="DirtyClosePolicy.Apply"/> — all unapplied changes are applied and saved.</item>
    ///             <item><see cref="DirtyClosePolicy.Ignore"/> — the window closes without touching settings.</item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         To restrict the policy to a single group, set <see cref="_groupId"/>.
    ///         Leave it empty to operate across all groups.
    ///     </para>
    /// </remarks>
    public sealed class UISettingsWindow : UIWindowBase
    {
        /// <summary>
        ///     Determines what happens to dirty (unapplied) settings when the window is closed.
        /// </summary>
        public enum DirtyClosePolicy
        {
            /// <summary>Revert all unapplied changes on close (default, safe).</summary>
            Revert,

            /// <summary>Apply and save all changes on close.</summary>
            Apply,

            /// <summary>Do nothing — leave settings in their current state.</summary>
            Ignore
        }

        [SerializeField, Tooltip("Group ID scope for dirty-close handling. Leave empty for all groups.")]
        private string _groupId;

        [SerializeField, Tooltip("What to do with unapplied changes when this window is closed.")]
        private DirtyClosePolicy _dirtyClosePolicy = DirtyClosePolicy.Revert;

        // ─────────────────────── UIWindowBase ─────────────────────────────

        /// <inheritdoc/>
        protected override void OnWindowClosed()
        {
            base.OnWindowClosed();

            if (!HasDirtySettings()) return;

            switch (_dirtyClosePolicy)
            {
                case DirtyClosePolicy.Revert:
                    HandleRevert();
                    break;

                case DirtyClosePolicy.Apply:
                    HandleApply();
                    break;

                case DirtyClosePolicy.Ignore:
                default:
                    break;
            }
        }

        // ─────────────────────── Helpers ──────────────────────────────────

        private bool HasDirtySettings()
        {
            if (SettingsManager.Instance == null) return false;

            if (string.IsNullOrEmpty(_groupId))
            {
                foreach (SettingGroupBase g in SettingsManager.Instance.Groups)
                    if (g.IsDirty) return true;
                return false;
            }

            SettingGroupBase group = SettingsManager.Instance.GetGroup(_groupId);
            return group?.IsDirty ?? false;
        }

        private void HandleRevert()
        {
            if (string.IsNullOrEmpty(_groupId))
                SettingsAPI.RevertAll();
            else
                SettingsAPI.Revert(_groupId);
        }

        private void HandleApply()
        {
            if (string.IsNullOrEmpty(_groupId))
            {
                SettingsAPI.ApplyAll();
                SettingsAPI.SaveAll();
            }
            else
            {
                SettingsAPI.Apply(_groupId);
                SettingsAPI.Save(_groupId);
            }
        }
    }
}
