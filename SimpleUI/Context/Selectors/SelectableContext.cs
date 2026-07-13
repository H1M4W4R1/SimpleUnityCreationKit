using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Context.Lists;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Context.Selectors
{
    /// <summary>
    ///     Selector context for UI
    /// </summary>
    public abstract class SelectableContext<TListObject> : ListContext<TListObject>
    {
        /// <summary>
        ///     Default index to select
        /// </summary>
        public int DefaultIndex { get; private set; }

        /// <summary>
        ///     Index of selected item
        /// </summary>
        public int SelectedIndex { get; private set; }

        /// <summary>
        ///     Checks if an item is selected
        /// </summary>
        public bool IsSelected => IsValidIndex(SelectedIndex);

        /// <summary>
        ///     Checks if there is a next item
        /// </summary>
        public bool HasNext => IsValidIndex(SelectedIndex + 1);
        
        /// <summary>
        ///     Checks if there is a previous item
        /// </summary>
        public bool HasPrevious => IsValidIndex(SelectedIndex - 1);
        
        /// <summary>
        ///     Gets the selected item
        /// </summary>
        /// <returns>The selected item, or null/default if the index is out of range</returns>
        [CanBeNull] public TListObject SelectedItem =>
            IsValidIndex(SelectedIndex) ? DataArray[SelectedIndex] : default;

        /// <summary>
        ///     Selects an item
        /// </summary>
        /// <param name="index">Index of item to select</param>
        public void SelectIndex(int index)
        {
            Assert.IsTrue(TrySelectIndex(index), "Index out of range");
        }

        /// <summary>
        ///     Tries to select the next item
        /// </summary>
        /// <param name="loop">If true, will loop to the first item if there is no next item</param>
        /// <returns>True if the item was selected, false otherwise</returns>
        public bool TrySelectNext(bool loop = false)
        {
            if (HasNext) return TrySelectIndex(SelectedIndex + 1);
            if (loop) return TrySelectIndex(0);
            return false;
        }

        /// <summary>
        ///     Tries to select the previous item
        /// </summary>
        /// <param name="loop">If true, will loop to the last item if there is no previous item</param>
        /// <returns>True if the item was selected, false otherwise</returns>
        public bool TrySelectPrevious(bool loop = false)
        {
            if (HasPrevious) return TrySelectIndex(SelectedIndex - 1);
            if (loop) return TrySelectIndex(DataArray.Count - 1);
            return false;
        }
        
        /// <summary>
        ///     Tries to select an item
        /// </summary>
        /// <param name="index">Item to select</param>
        /// <returns>True if the item was selected, false otherwise</returns>
        public bool TrySelectIndex(int index)
        {
            int oldIndex = SelectedIndex;
            if (!IsValidIndex(index)) return false;
            SelectedIndex = index;
            OnSelectionChanged(oldIndex, index);
            return true;
        }

        public bool TrySelectObject([CanBeNull] TListObject item)
        {
            // Find first index of item
            int itemIndex = -1;
            for (int iDataIndex = 0; iDataIndex < DataArray.Count; iDataIndex++)
            {
                TListObject dataObj = DataArray[iDataIndex];
                if (!Equals(dataObj, item)) continue;
                itemIndex = iDataIndex;
                break;
            }
            
            // Change selection
            if (itemIndex == -1) return false;
            
            int oldIndex = SelectedIndex;
            SelectedIndex = itemIndex;
            
            OnSelectionChanged(oldIndex, itemIndex);
            return true;
        }

        /// <summary>
        ///     Event called when selection changes
        /// </summary>
        public virtual void OnSelectionChanged(int oldIndex, int newIndex)
        {
            
        }

        public SelectableContext([NotNull] IReadOnlyList<TListObject> data, int defaultIndex = -1) : base(data)
        {
            DefaultIndex = defaultIndex;
            SelectedIndex = defaultIndex;
        }
    }
}