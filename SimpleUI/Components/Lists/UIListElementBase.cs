using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Context.Lists;
using UnityEngine;

namespace Systems.SimpleUI.Components.Lists
{
    /// <summary>
    ///     Element of a list, automatically provides context of the object it is linked to
    /// </summary>
    /// <typeparam name="TListObject">List object type</typeparam>
    public abstract class UIListElementBase<TListObject> : UIObjectWithContextBase<TListObject>,
        IWithLocalContext<TListObject>
    {
        /// <summary>
        ///     Name of the element
        /// </summary>
        [field: SerializeField, HideInInspector] protected internal string Name { get; set; }
        
        /// <summary>
        ///     Cached context of the element
        /// </summary>
        [CanBeNull] protected TListObject _contextCache;

        /// <summary>
        ///     Gets the cached context of the element
        /// </summary>
        [CanBeNull] protected internal TListObject CachedContext => _contextCache;
        
        /// <summary>
        ///     List context that this element is linked to
        /// </summary>
        protected internal ListContext<TListObject> Owner { get; internal set; }

        /// <summary>
        ///     Index of this element in the list
        /// </summary>
        protected internal int Index { get; internal set; }

        public bool TryGetContext(out TListObject context)
        {
            // Check if index is valid
            if (!Owner.IsValidIndex(Index))
            {
                context = default;
                return false;
            }

            // Provide context
            context = Owner[Index];
            return true;
        }

        public override void ValidateContext()
        {
            base.ValidateContext();
            
            // Skip if not visible
            if (!IsVisible) return;
            
            // Check if owner is null
            if (Owner == null) return;

            // Element is different from one in list, refresh this element
            if (Equals(Context, _contextCache)) return;
            SetDirty();
        }

        protected override void OnRefresh()
        {
            // Ensure base implementation is called
            base.OnRefresh();
            
            // Handled after re-render
            if(Owner.IsValidIndex(Index))
                _contextCache = Owner[Index];

            GameObjectReference!.name = $"{Name} [{Index}]";
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Name = gameObject.name;
        }
    }
}