using Systems.SimpleUI.Components.Buttons;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Buttons
{
    /// <summary>
    ///     Button that logs "Hello World!" to the console
    /// </summary>
    public sealed class LogHelloWorldButton : UIButtonBase
    {
        protected override void OnClick()
        {
            Debug.Log("Hello World!");
        }
    }
}