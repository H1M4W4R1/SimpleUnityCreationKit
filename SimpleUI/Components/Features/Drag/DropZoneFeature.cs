using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Systems.SimpleUI.Components.Features.Drag
{
    /// <summary>
    ///     Base drop zone feature.
    /// </summary>
    /// <typeparam name="TFeature">Type of draggable this zone accepts.</typeparam>
    public abstract class DropZoneFeature<TFeature> : MonoBehaviour
        where TFeature : DragFeature<TFeature>
    {
        /// <summary>
        ///     RectTransform of this zone.
        /// </summary>
        [field: SerializeField, HideInInspector] protected RectTransform rectTransform;
        
        /// <summary>
        ///     Collection of all drop zones.
        /// </summary>
        protected static readonly List<DropZoneFeature<TFeature>> zones = new();
        
        /// <summary>
        ///     Access to all drop zones.
        /// </summary>
        public static IReadOnlyList<DropZoneFeature<TFeature>> Zones => zones;

        protected virtual void Awake()
        {
            AssignComponents();
        }

        protected virtual void OnEnable() => zones.Add(this);
        protected virtual void OnDisable() => zones.Remove(this);

        protected virtual void AssignComponents()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        ///     Checks if the draggable can be dropped into this zone.
        /// </summary>
        public virtual bool CanDrop([NotNull] TFeature feature) => feature.CanDropInto(this);
        
        /// <summary>
        ///     Checks if the draggable can be picked up from this zone.
        /// </summary>
        public virtual bool CanPick([NotNull] TFeature feature) => feature.CanPickFrom(this);

        /// <summary>
        ///     Called when objet is dropped on this zone.
        /// </summary>
        protected internal virtual void OnDrop([NotNull] TFeature feature) => feature.OnSuccessfulDropInto(this);
        
        /// <summary>
        ///     Called  when drop to this zone fails.
        /// </summary>
        protected internal virtual void OnFailedDrop([NotNull] TFeature feature) => feature.OnFailedDrop(feature.CurrentDropZone, this);
        
        /// <summary>
        ///     Called when object is picked up from this zone.
        /// </summary>
        public virtual void OnPick([NotNull] TFeature dragFeature)
        {
            // Draggable was picked up from this zone
            dragFeature.OnPickFrom(this);
        }

        /// <summary>
        ///     Checks if the pointer is over this zone.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        /// <returns>True if the pointer is over this zone, false otherwise.</returns>
        internal virtual bool IsPointerOverZone([NotNull] PointerEventData eventData)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position,
                eventData.pressEventCamera);
        }
        
        protected virtual void OnValidate()
        {
            AssignComponents();
            Assert.IsNotNull(rectTransform, "DropZoneFeature requires a RectTransform component");
        }
    }
}
