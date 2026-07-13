using Systems.SimpleUI.Components.Abstract.Interactable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Toggles
{
    [RequireComponent(typeof(Toggle))] public abstract class UIToggleBase : UIInteractableObjectBase
    {
        [field: SerializeField, HideInInspector] protected internal Toggle ToggleReference { get; private set; }

        [field: SerializeField, HideInInspector]
        protected UIToggleGroupBase ToggleGroupReference { get; private set; }

        public sealed override bool IsInteractable => ToggleReference.interactable;

        /// <summary>
        ///     Returns the current state of the toggle
        /// </summary>
        public bool IsToggled
        {
            get => ToggleReference.isOn;
            protected internal set => ToggleReference.isOn = value;
        }

        protected override void AttachEvents()
        {
            ToggleReference.onValueChanged.AddListener(_OnToggleValueChanged);
        }

        protected override void DetachEvents()
        {
            ToggleReference.onValueChanged.RemoveListener(_OnToggleValueChanged);
        }

        protected override void OnTearDownComplete()
        {
            base.OnTearDownComplete();
            if (ToggleGroupReference) ToggleGroupReference.RefreshToggleArray();
        }

        /// <summary>
        ///     Internal event that is called when the toggle value changes
        ///     Proceeds to the <see cref="OnToggleValueChanged"/> event
        /// </summary>
        private void _OnToggleValueChanged(bool newValue)
        {
            if (ToggleGroupReference) ToggleGroupReference.OnToggleChanged(this, newValue);

            OnToggleValueChanged(newValue);
        }

        /// <summary>
        ///     Event that is called when the toggle value changes
        /// </summary>
        protected abstract void OnToggleValueChanged(bool newValue);

        /// <summary>
        ///     Changes the interactable state of the toggle
        /// </summary>
        public override void SetInteractable(bool interactable) =>
            ToggleReference.interactable = interactable;

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ToggleReference = GetComponent<Toggle>();
            ToggleGroupReference = GetComponentInParent<UIToggleGroupBase>(true);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ToggleReference = GetComponent<Toggle>();
            Assert.IsNotNull(ToggleReference, "UIToggleBase requires a Toggle component");

            // Optional
            ToggleGroupReference = GetComponentInParent<UIToggleGroupBase>(true);
        }
    }
}
