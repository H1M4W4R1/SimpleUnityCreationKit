using JetBrains.Annotations;
using Systems.SimpleUI.Components.Lists;
using Systems.SimpleUI.Context.Selectors;

namespace Systems.SimpleUI.Components.Selectors.Abstract
{
    /// <summary>
    ///     Selector for UI, used to select single item from a list
    /// </summary>
    /// <typeparam name="TObjectType">Object type in the list</typeparam>
    public abstract class UISelectorBase<TObjectType> : UIListBase<SelectableContext<TObjectType>, TObjectType>
    {
        /// <summary>
        ///     Cached selected item
        /// </summary>
        protected TObjectType _cachedSelectedItem;
        
        /// <summary>
        ///     Gets the selected item
        /// </summary>
        /// <returns>The selected item or null if no item is selected</returns>
        [CanBeNull] public TObjectType SelectedItem => Context != null ? Context.SelectedItem : default;

        /// <summary>
        ///     Checks if an item is selected
        /// </summary>
        public bool IsSelected => Context is {IsSelected: true};

        /// <summary>
        ///     Select given object if it is in the list
        /// </summary>
        /// <param name="item">Object to select</param>
        /// <returns>True if the object was selected, false otherwise</returns>
        public bool TrySelectObject([CanBeNull] TObjectType item)
        {
            if (Context is null) return false;

            // Get old index and try to select new object
            int oldIndex = Context.SelectedIndex;
            if (!Context.TrySelectObject(item)) return false;

            // Ensure that index has changed
            if (oldIndex == Context.SelectedIndex) return false;

            // Raise event
            OnSelectedIndexChanged(oldIndex, Context.SelectedIndex);
            return true;
        }

        /// <summary>
        ///     Changes the selected index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TrySelectIndex(int index)
        {
            if (Context is null) return false;

            // Get old index and try to select new index
            int oldIndex = Context.SelectedIndex;
            if (!Context.TrySelectIndex(index)) return false;

            // Ensure index has changed
            if (oldIndex == Context.SelectedIndex) return false;

            OnSelectedIndexChanged(oldIndex, Context.SelectedIndex);
            return true;
        }

        protected override void OnLateSetupComplete()
        {
            base.OnLateSetupComplete();
            
            // Select first element if context is not null
            // Used to ensure that the first element is selected nicely
            // Do not touch this, or it will go haywire as hell... We have to pass DefaultIndex here
            // because Unity is stupid and always triggers ToggleGroup change event on startup...
            if(Context is not null && Context.DataArray.Count > 0)
                TrySelectIndex(Context.DefaultIndex >= 0 ? Context.DefaultIndex : 0);
        }

        /// <summary>
        ///     Event called when selection changes
        /// </summary>
        protected virtual void OnSelectedIndexChanged(int from, int to)
        {
            // Request to refresh element if index has changed
            // to redraw the renderable
            RequestRefresh();
            
            // Cache selected item
            _cachedSelectedItem = Context is not null ? Context.SelectedItem : default;
        }

        protected override void OnRefresh()
        {
            // Ensure base implementation is called
            base.OnRefresh();

            // Update selected element
            TrySelectIndex(Context?.SelectedIndex ?? -1);
        }
        
        public override void ValidateContext()
        {
            base.ValidateContext();

            // If context is null, do nothing
            if (Context is null) return;

            // If selected item is the same, do nothing
            if (Equals(_cachedSelectedItem, Context.SelectedItem)) return;
            
            // Perform reasonable selection
            if (Context.IsValidIndex(Context.SelectedIndex))
            {
                OnSelectedIndexChanged(Context.SelectedIndex, Context.SelectedIndex);
            }
            else if(Context.DataArray.Count > 0)
            {
                // Select last item: invalid index most likely means items were removed,
                // so the index exceeds the new maximum — clamp to last available item
                TrySelectIndex(Context.DataArray.Count - 1);
            }
        }
    }
}