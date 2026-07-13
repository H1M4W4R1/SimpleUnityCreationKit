using Systems.SimpleUI.Components.Selectors.Implementations.Tabbing;

namespace Systems.SimpleUI.Components.Selectors.Tabs
{
    /// <summary>
    ///     Tab selector for UI
    /// </summary>
    public abstract class UITabSelectorBase : UIToggleGroupSelectorBase<UITab>
    {
        /// <summary>
        ///     Currently selected tab
        /// </summary>
        protected int SelectedTab => Context?.SelectedIndex ?? -1;
        
        /// <summary>
        ///     Use this method to handle tab selection - play animations etc.
        /// </summary>
        protected virtual void OnTabSelected(int from, int to)
        {
            if (Context is null) return;
            
            if(Context.IsValidIndex(from))
            {
                UITab info = Context[from];
                info.OnTabDeselected();
            }

            if (Context.IsValidIndex(to))
            {
                UITab info = Context[to];
                info.OnTabSelected();
            }
        }

        /// <summary>
        ///     Handles the selection change event
        /// </summary>
        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);
            OnTabSelected(from, to);
        }
    }
}