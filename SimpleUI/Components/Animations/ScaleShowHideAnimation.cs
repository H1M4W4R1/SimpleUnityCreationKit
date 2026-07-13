using DG.Tweening;
using Systems.SimpleUI.Components.Animations.Abstract;
using UnityEngine;

namespace Systems.SimpleUI.Components.Animations
{
    /// <summary>
    ///     Basic animation that scales the object to show it and scales it back to hide it
    /// </summary>
    public sealed class ScaleShowHideAnimation : UIAnimationBase, IUIShowAnimation, IUIHideAnimation
    {
        /// <summary>
        ///     The duration of the transition in seconds
        /// </summary>
        [field: SerializeField] public float TransitionDuration { get; private set; } = 0.25f;

        protected override void OnObjectActivated()
        {
            base.OnObjectActivated();
            
            // Ensure proper start scale
            selfTransform.localScale = Vector3.zero;
        }

        public Sequence OnShow() => DOTween.Sequence()
            .SetUpdate(true)
            .AppendCallback(Activate)
            .Append(selfTransform.DOScale(Vector3.one, TransitionDuration));

        public Sequence OnHide() => DOTween.Sequence().SetUpdate(true)
            .Append(selfTransform.DOScale(Vector3.zero, TransitionDuration))
            .AppendCallback(Deactivate);
        
   
    }
}