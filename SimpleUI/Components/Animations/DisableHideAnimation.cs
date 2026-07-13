using DG.Tweening;
using Systems.SimpleUI.Components.Animations.Abstract;

namespace Systems.SimpleUI.Components.Animations
{
    public sealed class DisableHideAnimation : UIAnimationBase, IUIHideAnimation
    {
        public Sequence OnHide()
        {
            return DOTween.Sequence().SetUpdate(true).OnComplete(Deactivate);
        }
    }
}