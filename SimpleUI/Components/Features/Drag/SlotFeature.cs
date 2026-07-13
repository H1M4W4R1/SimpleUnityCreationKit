using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Features.Drag
{
    /// <summary>
    ///     Slot that accepts exactly one draggable.
    /// </summary>
    /// <typeparam name="TDragFeature">Type of draggable it can hold.</typeparam>
    public abstract class SlotFeature<TDragFeature> : DropZoneFeature<TDragFeature>
        where TDragFeature : DragFeature<TDragFeature>
    {
        /// <summary>
        ///     Allow swapping items in the slot.
        /// </summary>
        [UsedImplicitly] protected virtual bool AllowSwap { get; set; } = true;

        /// <summary>
        ///     Gets the draggable that is currently in the slot.
        /// </summary>
        public TDragFeature Occupant { get; protected set; }

        /// <summary>
        ///     Checks if the draggable can be picked up.
        /// </summary>
        /// <param name="feature">Feature to pick up.</param>
        /// <returns>True if the draggable can be picked up, false otherwise.</returns>
        public override bool CanPick(TDragFeature feature)
        {
            Assert.AreEqual(Occupant, feature, "Occupant must be the same as feature. This should not happen.");
            return true;
        }

        public override void OnPick(TDragFeature dragFeature)
        {
            Occupant = null;
            dragFeature.transform.SetParent(transform);
        }

        /// <summary>
        ///     Checks if the draggable can be dropped into the slot.
        /// </summary>
        /// <param name="feature">Draggable to be dropped.</param>
        /// <returns>True if the draggable can be dropped into the slot, false otherwise.</returns>
        public override bool CanDrop(TDragFeature feature)
        {
            // Handle base case
            if (!base.CanDrop(feature)) return false;

            // Default: if empty then we can easily drop anything into slot
            if (ReferenceEquals(Occupant, null)) return true;

            // If swap is not allowed then we can only drop if occupant is null
            if (!AllowSwap) return false;

            // Allow swapping only if target slot exists
            if (ReferenceEquals(feature.CurrentDropZone, null)) return false;
            
            // Otherwise: check if we can pick-up this slot
            return CanPick(Occupant);
        }

        /// <summary>
        ///     Performs actions when the draggable is dropped into the slot.
        /// </summary>
        /// <param name="feature">Draggable to be dropped.</param>
        protected internal override void OnDrop(TDragFeature feature)
        {
            // If we have an occupant and a drop zone
            if (!ReferenceEquals(Occupant, null))
            {
                TDragFeature cachedOccupant = Occupant;
                
                if (!ReferenceEquals(feature.CurrentDropZone, null))
                {
                    // Pick-up current occupant, other one was already picked-up
                    OnPick(Occupant);

                    // Drop onto another feature's drop zone
                    feature.CurrentDropZone.OnDrop(cachedOccupant);
                }
                else
                {
                    Debug.LogError("Feature has no drop zone. Something went terribly wrong...");
                }
            }

            // Perform base dropping operations
            base.OnDrop(feature);

            // Swap occupant
            Occupant = feature;
            feature.transform.localPosition = Vector3.zero; // snap to center
        }

        /// <summary>
        ///     Performs actions when the draggable fails to be dropped out of this slot
        /// </summary>
        /// <param name="feature">Feature that failed to drop.</param>
        protected internal override void OnFailedDrop(TDragFeature feature)
        {
            base.OnFailedDrop(feature);
            Occupant = feature;
            feature.transform.localPosition = Vector3.zero; // snap to center
        }

        /// <summary>
        ///     Clears the slot.
        /// </summary>
        public virtual void ClearSlot()
        {
            Occupant = null;
        }
    }
}
