using Systems.SimpleUI.Components.Abstract.Interactable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Sliders
{
    /// <summary>
    ///     Slider for UI
    /// </summary>
    [RequireComponent(typeof(Slider))] public abstract class UISliderBase : UIInteractableObjectBase
    {
        [field: SerializeField, HideInInspector] protected Slider SliderReference { get; private set; }
        public sealed override bool IsInteractable => SliderReference.interactable;

        /// <summary>
        ///     Default value of the slider
        /// </summary>
        protected virtual float DefaultValue { get; private set; } = float.NaN;

        /// <summary>
        ///     Minimum value of the slider
        /// </summary>
        public float MinValue
        {
            get => SliderReference.minValue;
            protected set => SliderReference.minValue = value;
        }

        /// <summary>
        ///     Maximum value of the slider
        /// </summary>
        public float MaxValue
        {
            get => SliderReference.maxValue;
            protected set => SliderReference.maxValue = value;
        }

        /// <summary>
        ///     Current value of the slider
        /// </summary>
        public float CurrentValue
        {
            get => SliderReference.value;
            protected set => SliderReference.value = value;
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            SliderReference = GetComponent<Slider>();

            // Update current value of slider on creation
            if (float.IsNaN(DefaultValue)) DefaultValue = SliderReference.value;

            CurrentValue = DefaultValue;
        }

        protected override void AttachEvents()
        {
            SliderReference.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void DetachEvents()
        {
            SliderReference.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        /// <summary>
        ///     Event that is called when the slider value changes
        /// </summary>
        protected abstract void OnSliderValueChanged(float newValue);

        /// <summary>
        ///     Sets the interactable state of the slider
        /// </summary>
        public override void SetInteractable(bool interactable) =>
            SliderReference.interactable = interactable;

        protected override void OnValidate()
        {
            base.OnValidate();
            SliderReference = GetComponent<Slider>();
            Assert.IsNotNull(SliderReference, "UISliderBase requires a Slider component");
        }
    }
}
