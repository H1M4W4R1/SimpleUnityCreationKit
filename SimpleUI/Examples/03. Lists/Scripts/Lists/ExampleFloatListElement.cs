using System.Globalization;
using Systems.SimpleUI.Components.Abstract.Markers;
using Systems.SimpleUI.Components.Lists;
using TMPro;
using UnityEngine;

namespace Systems.SimpleUI.Examples._03._Lists.Scripts.Lists
{
    public sealed class ExampleFloatListElement : UIListElementBase<float>, IRenderable<float>
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void OnRender(float withContext)
        {
            _text.text = withContext.ToString(CultureInfo.InvariantCulture);
        }
    }
}