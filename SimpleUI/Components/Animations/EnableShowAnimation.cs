using DG.Tweening;
using Systems.SimpleUI.Components.Animations.Abstract;

namespace Systems.SimpleUI.Components.Animations
{
    public sealed class EnableShowAnimation : UIAnimationBase, IUIShowAnimation
    {
        public Sequence OnShow()
        {
            // Same as regular implementation, kept as fail-safe if it would get removed.
            return DOTween.Sequence().SetUpdate(true).OnComplete(Activate);
        }
    }
}