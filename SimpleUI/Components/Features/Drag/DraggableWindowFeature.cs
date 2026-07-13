using Systems.SimpleUI.Components.Features.Positioning;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleUI.Components.Features.Drag
{
    /// <summary>
    ///     Feature that allows dragging windows
    /// </summary>
    [RequireComponent(typeof(LimitObjectToViewport))]
    public sealed class DraggableWindowFeature : DragFeature<DraggableWindowFeature>
    {
        /// <summary>
        ///     We don't want to snap back on failed drop as window should be draggable across the screen
        /// </summary>
        protected override bool SnapBackOnFailedDrop => false;

        /// <summary>
        ///     Don't snap to mouse as it will look terribly
        /// </summary>
        protected override bool SnapToMouse =>  false;

        /// <summary>
        ///     Do not change parent as we want to keep window in its original parent
        /// </summary>
        protected override bool ChangeParent => false;

        /// <summary>
        ///     We can drop windows even if zone is null
        /// </summary>
        protected internal override bool CanDropInto(DropZoneFeature<DraggableWindowFeature> zone)
        {
            return true;
        }

        protected internal override void OnSuccessfulDropInto(DropZoneFeature<DraggableWindowFeature> newZone)
        {
            if (!ReferenceEquals(newZone, null))
            {
                base.OnSuccessfulDropInto(newZone);
            }
        }

#if UNITY_INCLUDE_TESTS
        internal void InitializeForTests()
        {
            AssignComponents();
        }
#endif
    }
}
