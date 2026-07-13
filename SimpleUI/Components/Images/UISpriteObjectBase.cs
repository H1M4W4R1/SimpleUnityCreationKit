using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Images
{
    [RequireComponent(typeof(Image))]
    public abstract class UISpriteObjectBase : UIObjectWithContextBase<Sprite>, IRenderable<Sprite>
    {
        [field: SerializeField, HideInInspector] protected Image ImageReference { get; private set; }

        public virtual void OnRender(Sprite withContext)
        {
            ImageReference.sprite = withContext;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ImageReference = GetComponent<Image>();
            Assert.IsNotNull(ImageReference, "UISpriteObjectBase requires an Image component");
        }
    }
}