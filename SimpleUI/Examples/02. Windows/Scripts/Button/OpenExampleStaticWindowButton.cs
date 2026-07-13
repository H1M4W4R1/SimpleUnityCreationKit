using Systems.SimpleUI.Components.Buttons;
using Systems.SimpleUI.Examples._02._Windows.Scripts.Windows;
using Systems.SimpleUI.Utility;

namespace Systems.SimpleUI.Examples._02._Windows.Scripts.Button
{
    public sealed class OpenExampleStaticWindowButton : UIButtonBase
    {
        protected override void OnClick()
        {
            // Show Example Window
            UserInterface.OpenWindow<ExampleStaticWindow>();
        }
    }
}