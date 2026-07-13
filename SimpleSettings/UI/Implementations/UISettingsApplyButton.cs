using Systems.SimpleSettings.Utility;
using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Button that applies settings.
    ///     If <see cref="_groupId"/> is set, only that group is applied;
    ///     otherwise all groups are applied.
    /// </summary>
    public sealed class UISettingsApplyButton : UIButtonBase
    {
        [SerializeField, Tooltip("Group ID to apply. Leave empty to apply all groups.")]
        private string _groupId;

        /// <inheritdoc/>
        protected override void OnClick()
        {
            if (string.IsNullOrEmpty(_groupId))
                SettingsAPI.ApplyAll();
            else
                SettingsAPI.Apply(_groupId);
        }
    }
}
