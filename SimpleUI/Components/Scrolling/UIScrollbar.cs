using Systems.SimpleUI.Components.Abstract.Interactable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Scrolling
{
    [RequireComponent(typeof(Scrollbar))] public abstract class UIScrollbar : UIInteractableObjectBase
    {
        [field: SerializeField, HideInInspector] protected Scrollbar ScrollbarReference { get; private set; }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            ScrollbarReference.onValueChanged.AddListener(OnScrollbarValueChanged);
        }
        
        protected override void DetachEvents()
        {
            ScrollbarReference.onValueChanged.RemoveListener(OnScrollbarValueChanged);
            base.DetachEvents();
        }

        /// <summary>
        ///     Raises when the scrollbar value changes
        /// </summary>
        /// <param name="value">New scrollbar value (0-1)</param>
        protected abstract void OnScrollbarValueChanged(float value);
        
        /// <summary>
        ///     Checks if the scrollbar is interactable
        /// </summary>
        public sealed override bool IsInteractable => ScrollbarReference.interactable;
        
        /// <summary>
        ///     Changes the interactable state of the scrollbar
        /// </summary>
        public override void SetInteractable(bool interactable) =>
            ScrollbarReference.interactable = interactable;

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ScrollbarReference = GetComponent<Scrollbar>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ScrollbarReference = GetComponent<Scrollbar>();
            Assert.IsNotNull(ScrollbarReference, "UIScrollbar requires a Scrollbar component");
        }
    }
}
