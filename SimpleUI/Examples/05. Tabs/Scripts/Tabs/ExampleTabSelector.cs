using Systems.SimpleUI.Components.Selectors.Tabs;
using UnityEngine;

namespace Systems.SimpleUI.Examples._05._Tabs.Scripts.Tabs
{
    public sealed class ExampleTabSelector : UITabSelectorBase
    {

        [ContextMenu("Select First")]
        public void SelectFirst()
        {
            TrySelectIndex(0);
        }

        [ContextMenu("Select Another")]
        public void SelectAnother()
        {
            TrySelectIndex(1);
        }
        
    }
}