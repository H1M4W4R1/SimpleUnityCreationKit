using DG.Tweening;

namespace Systems.SimpleUI.Components.Selectors.Abstract
{
    /// <summary>
    ///     Animated selector for UI that supports tweening between selections
    /// </summary>
    public abstract class UIAnimatedSelectorBase<TObjectType> : UISelectorBase<TObjectType>
    {
        protected Sequence CurrentSequence { get; set; }

        /// <summary>
        ///     Play animation for selection change
        /// </summary>
        /// <param name="from">Old index</param>
        /// <param name="to">New index</param>
        protected abstract Sequence AnimateSelectionChange(int from, int to);

        /// <summary>
        ///     Called when animation completes
        /// </summary>
        protected virtual void OnSelectionAnimationComplete(int from, int to)
        {
        }

        /// <summary>
        ///     Handles the selection change event.
        ///     Is called before animation starts. If you want to do something after animation
        ///     was completed see <see cref="OnSelectionAnimationComplete"/>
        /// </summary>
        protected override void OnSelectedIndexChanged(int from, int to)
        {
            base.OnSelectedIndexChanged(from, to);

            // Kill any running animation
            CurrentSequence?.Kill();

            // Create new animation
            CurrentSequence = AnimateSelectionChange(from, to);

            if (CurrentSequence == null) return;
            CurrentSequence.OnComplete(() => OnSelectionAnimationComplete(from, to));
            CurrentSequence.Play();
        }
    }
}