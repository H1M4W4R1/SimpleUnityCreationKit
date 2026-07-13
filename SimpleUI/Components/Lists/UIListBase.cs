using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using Systems.SimpleUI.Context.Lists;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Lists
{
    /// <summary>
    ///     List object used to render list elements
    /// </summary>
    /// <typeparam name="TListObject">Object type in the list</typeparam>
    public abstract class UIListBase<TListObject> : UIListBase<ListContext<TListObject>, TListObject>
    {
    }

    /// <summary>
    ///     List object used to render list elements, used to inherit list properties for different
    ///     context types.
    /// </summary>
    /// <typeparam name="TListContext">Context type of the list</typeparam>
    /// <typeparam name="TListObject">Object type in the list</typeparam>
    public abstract class UIListBase<TListContext, TListObject> : UIObjectWithContextBase<TListContext>,
        IRenderable<TListContext>
        where TListContext : ListContext<TListObject>
    {
        private IReadOnlyList<TListObject> _renderedList;
        private int _renderedCount;

        /// <summary>
        ///     Returns true if the list was modified on the last render
        /// </summary>
        protected bool WasModifiedOnLastRender { get; private set; }

        /// <summary>
        ///     List container
        /// </summary>
        [field: SerializeField] protected Transform Container { get; private set; }

        /// <summary>
        ///     List of all drawn elements
        /// </summary>
        protected readonly List<UIListElementBase<TListObject>> DrawnElements = new();

        /// <summary>
        ///     List of all hidden elements
        /// </summary>
        protected readonly Queue<UIListElementBase<TListObject>> HiddenElements = new();

        /// <summary>
        ///     Reference to the prefab of the list element
        /// </summary>
        [field: SerializeReference]
        [Tooltip("Only if you want to make list create it's elements. Otherwise leave null.")]
        [CanBeNull]
        protected UIListElementBase<TListObject> ElementPrefab { get; private set; }

        protected override void OnSetupComplete()
        {
            base.OnSetupComplete();

            // Add all drawn elements to the list to prevent issues when designer puts something
            // onto the scene...
            DrawnElements.AddRange(
                GetComponentsInChildren<UIListElementBase<TListObject>>(includeInactive: false));
        }

        /// <summary>
        ///     Executed when element is hidden
        /// </summary>
        protected virtual void OnElementHidden([NotNull] UIListElementBase<TListObject> element)
        {
            
        }

        /// <summary>
        ///     Executed when element is shown
        /// </summary>
        protected virtual void OnElementShown([NotNull] UIListElementBase<TListObject> element)
        {
            
        }

        public virtual void OnRender(TListContext withContext)
        {
            Assert.IsNotNull(withContext, "List context cannot be null.");

            // If null then list does not support rendering at all
            // we mark it as rendered for compatibility with non-renderable lists
            if (!ElementPrefab)
            {
                _renderedCount = withContext.Count;
                _renderedList = withContext.DataArray;
                return;
            }

            // Traversing index
            int traversingIndex = 0;

            UnsafeList<int> nToMove = new(64, Allocator.TempJob);

            // Handle elements that already exist
            for (int drawnObjectIndex = 0; drawnObjectIndex < DrawnElements.Count; drawnObjectIndex++)
            {
                // Show element if it is not hidden
                UIListElementBase<TListObject> element = DrawnElements[drawnObjectIndex];

                // Check if current index is valid
                if (!withContext.IsValidIndex(traversingIndex))
                {
                    // Hide element as it is not valid
                    nToMove.Add(drawnObjectIndex);
                    continue;
                }

                // Check if element is within bounds array and same as traversing element
                if (Equals(withContext[traversingIndex], element.CachedContext))
                {
                    // Refresh element with correct index to ensure proper rendering
                    element.Index = traversingIndex;
                    element.Owner = withContext;
                    element.RequestRefresh(); // we don't need to show element as it's already visible

                    // Move to next element
                    traversingIndex++;
                    continue;
                }

                // Hide element as it is not valid, we search for next element
                // with same value as current traversing object
                nToMove.Add(drawnObjectIndex);
            }

            // Handle elements cleanup
            for (int nIndex = nToMove.Length - 1; nIndex >= 0; nIndex--)
            {
                // Hide element from UI
                UIListElementBase<TListObject> element = DrawnElements[nToMove[nIndex]];
                DrawnElements.RemoveAt(nToMove[nIndex]);
                HiddenElements.Enqueue(element);
                element.Hide();
                
                // Element was hidden
                OnElementHidden(element);
            }

            // Dispose of temporary list
            WasModifiedOnLastRender = nToMove.Length > 0;
            nToMove.Dispose();
            
            // Assert element prefab is not null
            Assert.IsNotNull(ElementPrefab, "Element prefab cannot be null.");

            // Handle elements that need to be created
            while (traversingIndex < withContext.Count)
            {
                UIListElementBase<TListObject> element = GetOrCreateElement();

                // Null check - ElementPrefab may not be assigned
                if (ReferenceEquals(element, null))
                {
                    Debug.LogError($"[UIListBase] GetOrCreateElement returned null. Ensure ElementPrefab is assigned on {name}.");
                    break;
                }

                // Set element context
                element.Owner = withContext;
                element.Index = traversingIndex;
                traversingIndex++;

                // Add element to the list
                DrawnElements.Add(element);

                // Show element
                if (!element.IsVisible) element.Show();
                  
                // Ensure element is re-rendered based on regular implementations
                // as first render will be called before element data is set to be valid
                element.RequestRefresh();
                WasModifiedOnLastRender = true;
                
                // Element was shown
                OnElementShown(element);
            }

            // Update rendered list
            _renderedList = withContext.DataArray;
            _renderedCount = withContext.Count;
        }

        [CanBeNull] private UIListElementBase<TListObject> GetOrCreateElement()
        {
            if (HiddenElements.Count > 0)
            {
                // Get hidden element and move to end of list
                UIListElementBase<TListObject> element = HiddenElements.Dequeue();
                element.transform.SetAsLastSibling();
                return element;
            }

            // Check if element prefab is valid
            if (!ElementPrefab) return null;

            // Create element
            return Instantiate(ElementPrefab, Container);
        }

        public override void ValidateContext()
        {
            base.ValidateContext();

            // Check if context has changed (list data changed)
            if (!ReferenceEquals(_renderedList, Context?.DataArray))
            {
                SetDirty();
                return;
            }

            // Check if count has changed (element added/removed)
            if (_renderedCount == Context?.Count) return;
            SetDirty();

            // Single-element updates are handled within UIListElementBase
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Container) Container = transform;
        }
    }
}