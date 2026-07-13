using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Progress
{
    /// <summary>
    ///     Object that represents a progress "bar"
    /// </summary>
    public abstract class UIProgressBase : UIObjectWithContextBase<float>, IRenderable<float>
    {
        private float _drawnValue = -1f;
        
        /// <summary>
        ///     Collection of progress images
        /// </summary>
        [field: SerializeField, HideInInspector] private UIProgressImage[] progressImages;
        
        protected override void AssignComponents()
        {
            base.AssignComponents();
            progressImages = GetComponentsInChildren<UIProgressImage>(true);
            Assert.IsTrue(progressImages.Length > 0, "No progress images found in the hierarchy. " +
                                                     "Maybe validation failed? Try to modify progress value.");
        }

        public override void ValidateContext()
        {
            base.ValidateContext();
            
            // Check if progress has changed
            if (Mathf.Approximately(Context, _drawnValue)) return;
            SetDirty();
        }

        public void OnRender(float newProgress)
        {
            // Clamp progress to [0, 1]
            newProgress = Mathf.Clamp01(newProgress);
            
            // Update all progress images
            for (int index = 0; index < progressImages.Length; index++)
            {
                UIProgressImage progressImage = progressImages[index];
                progressImage.SetProgress(newProgress);
            }
            
            _drawnValue = newProgress;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            progressImages = GetComponentsInChildren<UIProgressImage>(true);
            Assert.IsTrue(progressImages.Length > 0, "No progress images found in the hierarchy. " +
                                                     "Try to add some as children of UIProgressBase component.");
        }
    }
}
