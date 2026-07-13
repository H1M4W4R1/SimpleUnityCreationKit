using Systems.SimpleSettings.Utility;
using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Button that resets settings to factory defaults.
    ///     If <see cref="_groupId"/> is set, only that group is reset;
    ///     otherwise all groups are reset.
    /// </summary>
    public sealed class UISettingsResetButton : UIButtonBase
    {
        [SerializeField, Tooltip("Group ID to reset. Leave empty to reset all groups.")]
        private string _groupId;

        /// <inheritdoc/>
        protected override void OnClick()
        {
            if (string.IsNullOrEmpty(_groupId))
                SettingsAPI.ResetAll();
            else
                SettingsAPI.ResetToDefaults(_groupId);
        }
    }
}
