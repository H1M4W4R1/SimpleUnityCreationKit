using Systems.SimpleUI.Components.Toggles;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Toggles
{
    public sealed class ExampleToggleGroup : UIToggleGroupBase
    {
        // ReSharper disable once NotAccessedField.Local
        private int _selectedToggleIndex;
        
        protected override void OnToggleValueChanged(int toggleIndex, bool newValue)
        {
            if (!newValue) return;
            _selectedToggleIndex = toggleIndex;
            Debug.Log("Selected toggle index: " + FirstToggleIndex);
        }
    }
}