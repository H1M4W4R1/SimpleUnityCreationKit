using Systems.SimpleSettings.Abstract;
using Systems.SimpleSettings.Core;
using Systems.SimpleSettings.Utility;
using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Button that undoes the most recent unapplied change.
    ///     If <see cref="_groupId"/> is set, only that group's undo stack is popped;
    ///     otherwise all groups are tried in order until one succeeds.
    /// </summary>
    /// <remarks>
    ///     The button is automatically disabled when there is nothing to undo.
    ///     This is checked on each frame via <see cref="OnRefresh"/>.
    /// </remarks>
    public sealed class UISettingsUndoButton : UIButtonBase
    {
        [SerializeField, Tooltip("Group ID for undo scope. Leave empty to undo across all groups.")]
        private string _groupId;

        /// <inheritdoc/>
        protected override void OnClick()
        {
            if (string.IsNullOrEmpty(_groupId))
                SettingsAPI.TryUndoAll();
            else
                SettingsAPI.TryUndo(_groupId);
        }

        /// <summary>
        ///     Disables the button when the relevant group(s) have no changes to undo.
        /// </summary>
        protected override void OnRefresh()
        {
            base.OnRefresh();

            if (SettingsManager.Instance == null) return;

            bool hasUndoable = false;
            if (string.IsNullOrEmpty(_groupId))
            {
                foreach (SettingGroupBase g in SettingsManager.Instance.Groups)
                    if (g.IsDirty) { hasUndoable = true; break; }
            }
            else
            {
                SettingGroupBase group = SettingsManager.Instance.GetGroup(_groupId);
                hasUndoable = group?.IsDirty ?? false;
            }

            SetInteractable(hasUndoable);
        }
    }
}
