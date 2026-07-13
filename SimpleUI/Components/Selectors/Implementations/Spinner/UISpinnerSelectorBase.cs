using DG.Tweening;
using Systems.SimpleUI.Components.Lists;
using Systems.SimpleUI.Components.Selectors.Abstract;
using UnityEngine;

namespace Systems.SimpleUI.Components.Selectors.Implementations.Spinner
{
    /// <summary>
    ///     Nicely-rendered spinner selector.
    /// </summary>
    /// <typeparam name="TObjectType">Object type</typeparam>
    public abstract class UISpinnerSelectorBase<TObjectType> : UIPreviousNextAnimatedSelectorBase<TObjectType>
    {
        [field: SerializeField] protected float TransitionTime { get; set; } = 0.28f;
        [field: SerializeField] protected Axis SpinnerAxis { get; set; } = Axis.Horizontal;
        [field: SerializeField] private Ease SpinnerEase { get; set; } = Ease.OutCubic;

        [field: SerializeField] protected bool SpinnerLoopingEnabled { get; set; } = true;

        /// <summary>
        ///     Wraps <see cref="SpinnerLoopingEnabled"/> for compliance and proper serialization
        ///     because Unity always is shit at making objects serializable in reasonable manner
        /// </summary>
        public sealed override bool IsLooping
        {
            get => SpinnerLoopingEnabled;
            protected set => SpinnerLoopingEnabled = value;
        }

        // Spacing between objects
        private float _spacing = 200f;

        protected virtual Vector2 AxisVector(float multiplier)
        {
            return SpinnerAxis == Axis.Horizontal ? new Vector2(multiplier, 0f) : new Vector2(0f, multiplier);
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            // Prevent unnecessary updates
            // Removing this will fuck-up animations, do not touch unless you fixed that already.
            if (!WasModifiedOnLastRender) return;
            SnapAllElementsToSelected(Context?.SelectedIndex ?? 0);
        }

        /// <summary>
        ///     Compute minimal relative index (circular) so element is placed to the nearest slot.
        /// </summary>
        private int GetMinimalRelativeIndex(int elementIndex, int selectedIndex, int count)
        {
            if (count <= 0) return elementIndex - selectedIndex;

            int rel = elementIndex - selectedIndex;

            if (!IsLooping) return rel;

            // Normalize rel to be within [-count/2, count/2]
            // Note: use '< -half' (not '<=') so that -half stays -half instead of mapping to +half.
            int half = count / 2;
            while (rel > half) rel -= count;
            while (rel < -half) rel += count;
            return rel;
        }

        /// <summary>
        ///     Compute target anchored position for a given element index relative to selected index.
        /// </summary>
        private Vector2 ComputeTargetPosition(int elementIndex, int selectedIndex, int count)
        {
            int rel = GetMinimalRelativeIndex(elementIndex, selectedIndex, count);
            float multiplier = rel * _spacing;
            return AxisVector(multiplier);
        }

        /// <summary>
        ///     Snap all elements to the current selected index.
        /// </summary>
        protected void SnapAllElementsToCurrent()
        {
            SnapAllElementsToSelected(Context?.SelectedIndex ?? 0);
        }

        /// <summary>
        ///     Immediately place elements to the correct positions (used at startup or after heavy interrupts)
        /// </summary>
        protected void SnapAllElementsToSelected(int selectedIndex)
        {
            if (Context is null) return;
            int count = Context.Count;

            for (int i = 0; i < DrawnElements.Count; i++)
            {
                UIListElementBase<TObjectType> element = DrawnElements[i];
                if (element == null) continue;

                RectTransform rect = element.RectTransformReference
                    ? element.RectTransformReference
                    : element.GetComponent<RectTransform>();
                if (rect == null) continue;

                Vector3 target = ComputeTargetPosition(element.Index, selectedIndex, count);
                rect.anchoredPosition = target;
                // Ensure no running tween on element
                rect.DOKill();
            }
        }

        /// <summary>
        ///     Adjust rect.anchoredPosition by whole-cycle shifts (± totalSpan) until the rect's start position
        ///     is the representation closest to the desired target (avoids long-wrap animation).
        /// </summary>
        private void MoveRectToNearestEquivalent(RectTransform rect, Vector2 target, int count)
        {
            if (count <= 0) return;
            // axis unit vector
            Vector2 axisUnit = SpinnerAxis == Axis.Horizontal ? Vector2.right : Vector2.up;
            float totalSpan = count * _spacing;

            Vector2 current = rect.anchoredPosition;
            // difference along spinner axis (positive means target is ahead of current on that axis)
            float diff = Vector2.Dot(target - current, axisUnit);

            // shift current by ±totalSpan until the difference along axis is within half a full span
            float halfSpan = totalSpan * 0.5f;
            while (Mathf.Abs(diff) > halfSpan)
            {
                if (diff < 0)
                {
                    // target is behind current by more than half span -> shift current backward
                    current -= axisUnit * totalSpan;
                }
                else
                {
                    // target is ahead by more than half span -> shift current forward
                    current += axisUnit * totalSpan;
                }

                diff = Vector2.Dot(target - current, axisUnit);
            }

            rect.anchoredPosition = current;
        }

        protected override Sequence AnimateSelectionChange(int from, int to)
        {
            if (Context is null) return null;
            int count = Context.Count;
            if (count == 0) return null;

            // Kill any previously created sequence (base already does but be defensive)
            CurrentSequence?.Kill();
            CurrentSequence = DOTween.Sequence();

            float totalSpan = count * _spacing;

            // Build tweens for every drawn element to move to its computed slot relative to new selected index
            for (int i = 0; i < DrawnElements.Count; i++)
            {
                UIListElementBase<TObjectType> element = DrawnElements[i];
                if (element == null) continue;

                RectTransform rect = element.RectTransformReference
                    ? element.RectTransformReference
                    : element.GetComponent<RectTransform>();
                if (rect == null) continue;

                // Stop any existing tweens affecting this rect so we always animate from true current visual state
                rect.DOKill();

                Vector2 target = ComputeTargetPosition(element.Index, to, count);

                // --- IMPORTANT: shift the rect (no animation) to the nearest equivalent representation
                // so the actual tween will go the short way instead of across the whole viewport.
                if (IsLooping && totalSpan > 0f)
                {
                    MoveRectToNearestEquivalent(rect, target, count);
                }

                // Create tween from current anchored pos to target
                Tween t = rect.DOAnchorPos(target, TransitionTime).SetEase(SpinnerEase);
                CurrentSequence.Join(t);
            }

            // When animation completes, ensure elements are exactly at intended positions and fire callback
            CurrentSequence.OnComplete(() =>
            {
                // Final snap defend (avoid floating point drift after many tweens / spams)
                SnapAllElementsToSelected(to);
                OnSelectionAnimationComplete(from, to);
            });

            return CurrentSequence;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Limit spacing size to size of object ;)
            if (!ElementPrefab) return; // Optional
            
            // Setup size to be correct
            float sizeX = ElementPrefab.RectTransformReference.sizeDelta.x;
            _spacing = sizeX;
        }

        public enum Axis
        {
            Horizontal,
            Vertical
        }
    }
}