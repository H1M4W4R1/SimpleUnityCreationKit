using Systems.SimpleUI.Components.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Examples._06._Tooltips.Scripts
{
    public sealed class TooltipExample : UITooltipBase<string>
    {
        [field: SerializeField, HideInInspector] private TextMeshProUGUI TextReference { get; set; }

        protected override void OnValidate()
        {
            base.OnValidate();
            TextReference = GetComponentInChildren<TextMeshProUGUI>();
            Assert.IsNotNull(TextReference, "TooltipExample requires a TextMeshProUGUI component");
        }

        public override void OnRender(string withContext)
        {
            TextReference.text = withContext;
        }
    }
}