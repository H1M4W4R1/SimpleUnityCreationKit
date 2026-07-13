using Systems.SimpleUI.Components.Selectors.Tabs;

namespace Systems.SimpleUI.Examples._05._Tabs.Scripts.Tabs
{
    /// <summary>
    ///     Example tab implementation
    /// </summary>
    public sealed class ExampleTab : UITab
    {
        protected internal override void OnTabDeselected()
        {
            base.OnTabDeselected();
            gameObject.SetActive(false);
        }
        
        protected internal override void OnTabSelected()
        {
            base.OnTabSelected();
            gameObject.SetActive(true);
        }
    }
}