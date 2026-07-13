using Systems.SimpleUI.Components.Abstract;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Progress
{
    /// <summary>
    ///     UI image to display progress nicely
    /// </summary>
    [RequireComponent(typeof(Image))] public sealed class UIProgressImage : UIObjectBase
    {
        /// <summary>
        ///     Reference to the image component
        /// </summary>
        [field: SerializeField, HideInInspector] private Image ImageReference { get; set; }

   /// <summary>
        ///     Sets the progress of the image
        /// </summary>
        internal void SetProgress(float progress)
        {
            ImageReference.fillAmount = progress;
        }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            ImageReference = GetComponent<Image>();
            if (ImageReference && ImageReference.type != Image.Type.Filled) ImageReference.type = Image.Type.Filled;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ImageReference = GetComponent<Image>();
            Assert.IsNotNull(ImageReference, "UIProgressImage requires an Image component");
            if (ImageReference.type != Image.Type.Filled) ImageReference.type = Image.Type.Filled;
        }
    }
}
