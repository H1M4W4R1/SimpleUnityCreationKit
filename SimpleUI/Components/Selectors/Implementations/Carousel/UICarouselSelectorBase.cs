using DG.Tweening;
using Systems.SimpleUI.Components.Selectors.Abstract;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Selectors.Implementations.Carousel
{
    /// <summary>
    ///     Carousel selector using Unity ScrollRect for horizontal/vertical snapping
    /// </summary>
    /// <typeparam name="TObjectType">Object type in the list</typeparam>
    [RequireComponent(typeof(UICarouselScrollRect))]
    public abstract class UICarouselSelectorBase<TObjectType> : UIPreviousNextAnimatedSelectorBase<TObjectType>
    {
        [field: SerializeField, HideInInspector] protected UICarouselScrollRect ScrollRectReference { get; private set; }
        [field: SerializeField] protected Ease CarouselEase { get; set; } = Ease.OutCubic;
        
        /// <summary>
        ///     Carousels don't support looping at all.
        /// </summary>
        public sealed override bool IsLooping => false;

        /// <summary>
        ///     Time of transition between items
        /// </summary>
        [field: SerializeField] protected float TransitionDuration = 0.35f;
        
        private bool IsHorizontal => ScrollRectReference.horizontal;
        
        protected override void OnLateSetupComplete()
        {
            base.OnLateSetupComplete();
            if (Context is null) return;

            SnapToIndex(Context.SelectedIndex);
        }

        /// <summary>
        ///     Snap instantly to an index (no animation)
        /// </summary>
        protected void SnapToIndex(int index)
        {
            if (Context is null || !Context.IsValidIndex(index)) return;

            float normalized = GetNormalizedPositionForIndex(index);
            SetNormalizedPosition(normalized);
        }

        /// <summary>
        ///     Convert index to normalized position [0..1]
        /// </summary>
        private float GetNormalizedPositionForIndex(int index)
        {
            if (Context is null) return 0f;
            if (Context.Count <= 1) return 0f;
            float step = 1f / (Context.Count - 1);
            return Mathf.Clamp01(step * index);
        }

        /// <summary>
        ///     Direct setter for ScrollRect position
        /// </summary>
        private void SetNormalizedPosition(float value)
        {
            if (IsHorizontal)
                ScrollRectReference.horizontalNormalizedPosition = value;
            else
                ScrollRectReference.verticalNormalizedPosition = 1f - value; // vertical is inverted
        }

        /// <summary>
        ///     Animate selection change via DoTween
        /// </summary>
        protected override Sequence AnimateSelectionChange(int from, int to)
        {
            if (Context is null || !Context.IsValidIndex(to)) return null;

            float targetNormalized = GetNormalizedPositionForIndex(to);

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            if (IsHorizontal)
            {
                seq.Append(ScrollRectReference
                    .DOHorizontalNormalizedPos(targetNormalized, TransitionDuration)
                    .SetEase(CarouselEase));
            }
            else
            {
                seq.Append(ScrollRectReference
                    .DOVerticalNormalizedPos(1f - targetNormalized, TransitionDuration)
                    .SetEase(CarouselEase));
            }

            return seq;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ScrollRectReference = GetComponent<UICarouselScrollRect>();
            Assert.IsNotNull(ScrollRectReference, "UICarouselSelectorBase requires a UICarouselScrollRect component");
        }
    }
}