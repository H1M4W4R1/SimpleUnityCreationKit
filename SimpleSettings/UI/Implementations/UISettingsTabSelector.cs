using Systems.SimpleUI.Components.Selectors.Tabs;

namespace Systems.SimpleSettings.UI.Implementations
{
    /// <summary>
    ///     Concrete tab selector for settings panels.
    /// </summary>
    /// <remarks>
    ///     Attach alongside a <see cref="Systems.SimpleUI.Components.Selectors.Implementations.Tabbing.UISelectorToggleGroup"/>
    ///     (required by the base) and a <see cref="UISettingsTabContextProvider"/> on the same GameObject.
    ///     Tab toggles in the hierarchy must use
    ///     <see cref="Systems.SimpleUI.Components.Selectors.Tabs.UITabSelectorToggle"/>.
    /// </remarks>
    public sealed class UISettingsTabSelector : UITabSelectorBase
    {
    }
}
