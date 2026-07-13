using Systems.SimpleUI.Components.Selectors.Implementations.Carousel;
using UnityEngine;

namespace Systems.SimpleUI.Examples._00._Text_and_Input.Scripts.Carousel
{
    public sealed class CarouselExample : UICarouselSelectorBase<Color>
    {
        protected override void OnSelectionAnimationComplete(int from, int to)
        {
            base.OnSelectionAnimationComplete(from, to);
            if (ReferenceEquals(Context, null)) return;
            Debug.Log($"Selected color: {Context[to]}");
        }
    }
}