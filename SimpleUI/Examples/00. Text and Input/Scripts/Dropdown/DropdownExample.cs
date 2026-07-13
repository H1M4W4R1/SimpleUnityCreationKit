using System.Globalization;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Selectors.Implementations.Dropdown;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Dropdown
{
    public sealed class DropdownExample : UIDropdownSelectorBase<float>
    {
        [NotNull] protected override string GetOptionLabel(float obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);
            
            // Skip if context is null
            if (ReferenceEquals(Context, null)) return;
            Debug.Log($"Selected index changed from {from} to {to}");
            Debug.Log($"Selected value: {Context.SelectedItem}");
        }
    }
}