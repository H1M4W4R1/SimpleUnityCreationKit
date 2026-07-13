using Systems.SimpleUI.Utility;

namespace Systems.SimpleUI.Components.Windows
{
    /// <summary>
    ///     Popup window to handle notifications and other weird stuff
    /// </summary>
    public abstract class UIPopupBase : UIWindowBase
    {
        public sealed override bool AllowMultipleInstancesWithDifferentContext => false;

        public sealed override bool AllowMultipleInstancesWithSameContext => false;

        protected internal override void OnWindowClosed()
        {
            base.OnWindowClosed();
            
            // Open next popup in queue if any
            UserInterface.TryOpenNextPopup();
        }
        
    }
}