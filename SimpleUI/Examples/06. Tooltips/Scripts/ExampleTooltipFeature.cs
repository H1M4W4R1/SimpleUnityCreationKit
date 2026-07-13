using Systems.SimpleUI.Components.Tooltips;
using UnityEngine;

namespace Systems.SimpleUI.Examples._06._Tooltips.Scripts
{
    public sealed class ExampleTooltipFeature : UITooltipFeature<TooltipExample, string>
    {
        [field: SerializeField] private string TooltipText { get; set; }
        
        protected override string GetNewTooltipContext() => TooltipText;
    }
}