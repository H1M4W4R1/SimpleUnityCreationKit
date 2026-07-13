namespace Systems.SimpleUI.Components.Selectors.Abstract
{
    /// <summary>
    ///     Animated selector for UI that supports tweening between selections
    ///     which also supports previous/next selection
    /// </summary>
    public abstract class UIPreviousNextAnimatedSelectorBase<TObjectType> : UIAnimatedSelectorBase<TObjectType>,
        IPreviousNextSelector
    {
        /// <summary>
        ///     Previous/Next selectors will be able to loop
        /// </summary>
        public virtual bool IsLooping { get; protected set; }
        
        /// <summary>
        ///     True if there is a next item
        /// </summary>
        public bool HasNext => Context?.HasNext ?? false;
        
        /// <summary>
        ///     True if there is a previous item
        /// </summary>
        public bool HasPrevious => Context?.HasPrevious ?? false;
   
        /// <summary>
        ///     Selects the next item
        /// </summary>
        /// <returns>True if the item was selected, false otherwise</returns>
        public virtual bool TrySelectNext()
        {
            if (Context is null) return false;

            int oldIndex = Context.SelectedIndex;
            Context.TrySelectNext(IsLooping);

            // Ensure index has changed
            if (oldIndex == Context.SelectedIndex) return false;

            // Raise event
            OnSelectedIndexChanged(oldIndex, Context.SelectedIndex);
            return true;
        }

        /// <summary>
        ///     Selects the previous item
        /// </summary>
        /// <returns>True if the item was selected, false otherwise</returns>
        public virtual bool TrySelectPrevious()
        {
            if (Context is null) return false;
            
            int oldIndex = Context.SelectedIndex;
            Context.TrySelectPrevious(IsLooping);
            
            // Ensure index has changed
            if (oldIndex == Context.SelectedIndex) return false;

            // Raise event
            OnSelectedIndexChanged(oldIndex, Context.SelectedIndex);
            return true;
        }
      
    }
}