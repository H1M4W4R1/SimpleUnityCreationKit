using Systems.SimpleUI.Components.Abstract;

namespace Systems.SimpleUI.Components.Selectors.Tabs
{
    /// <summary>
    ///     Example UI tab
    /// </summary>
    public abstract class UITab : UIObjectBase
    {
        protected internal virtual void OnTabSelected()
        {
            
        }
        
        protected internal virtual void OnTabDeselected()
        {
        }
    }
}