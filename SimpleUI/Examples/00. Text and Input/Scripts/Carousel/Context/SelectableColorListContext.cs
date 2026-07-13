using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Context.Selectors;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Carousel.Context
{
    public sealed class SelectableColorListContext : SelectableContext<Color>
    {
        public SelectableColorListContext([NotNull] IReadOnlyList<Color> data, int defaultIndex = -1) : base(data,
            defaultIndex)
        {
        }
    }
}