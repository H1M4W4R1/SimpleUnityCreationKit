using Systems.SimpleUI.Components.Toggles;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Toggles
{
    public sealed class ExampleToggle : UIToggleBase
    {
        protected override void OnToggleValueChanged(bool newValue)
        {
            Debug.Log("Toggle value: " + newValue);
        }
    }
}