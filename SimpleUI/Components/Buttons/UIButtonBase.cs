using Systems.SimpleUI.Components.Abstract.Interactable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Buttons
{
    [RequireComponent(typeof(Button))] public abstract class UIButtonBase : UIInteractableObjectBase
    {
        /// <summary>
        ///     Reference to the button component
        /// </summary>
        [field: SerializeField, HideInInspector] protected Button ButtonReference { get; private set; }
        
        public sealed override bool IsInteractable => ButtonReference.interactable;
        
        protected override void AttachEvents()
        {
            base.AttachEvents();
            ButtonReference.onClick.AddListener(OnClick);
        }

        protected override void DetachEvents()
        {
            ButtonReference.onClick.RemoveListener(OnClick);
            base.DetachEvents();
        }

        /// <summary>
        ///     Event that is called when the button is clicked
        /// </summary>
        protected abstract void OnClick();

        /// <summary>
        ///     Changes the interactable state of the button
        /// </summary>
        public override void SetInteractable(bool interactable)
        {
            ButtonReference.interactable = interactable;
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ButtonReference = GetComponent<Button>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ButtonReference = GetComponent<Button>();
            Assert.IsNotNull(ButtonReference, "UIButtonBase requires a Button component");
        }
    }
}
