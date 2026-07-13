using Systems.SimpleSettings.Utility;
using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Button that reverts settings to their last applied values.
    ///     If <see cref="_groupId"/> is set, only that group is reverted;
    ///     otherwise all groups are reverted.
    /// </summary>
    public sealed class UISettingsRevertButton : UIButtonBase
    {
        [SerializeField, Tooltip("Group ID to revert. Leave empty to revert all groups.")]
        private string _groupId;

        /// <inheritdoc/>
        protected override void OnClick()
        {
            if (string.IsNullOrEmpty(_groupId))
                SettingsAPI.RevertAll();
            else
                SettingsAPI.Revert(_groupId);
        }
    }
}
