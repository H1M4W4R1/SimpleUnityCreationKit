using Systems.SimpleUI.Components.Selectors.Tabs;
using UnityEngine;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     A settings tab that shows its associated panel when selected and hides it when deselected.
    /// </summary>
    /// <remarks>
    ///     Place one <see cref="UISettingsTab"/> per category in the hierarchy and assign
    ///     the corresponding settings panel to <c>_panel</c>.  The tab is discovered
    ///     automatically by <see cref="UISettingsTabContextProvider"/>.
    /// </remarks>
    public sealed class UISettingsTab : UITab
    {
        [SerializeField] private GameObject _panel;

        protected override void OnTabSelected()
        {
            base.OnTabSelected();
            if (_panel) _panel.SetActive(true);
        }

        protected override void OnTabDeselected()
        {
            base.OnTabDeselected();
            if (_panel) _panel.SetActive(false);
        }
    }
}
