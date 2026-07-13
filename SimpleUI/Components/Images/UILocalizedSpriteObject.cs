using Systems.SimpleUI.Components.Abstract;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Images
{
    /// <summary>
    ///     Drives an <see cref="Image"/> sprite from a <see cref="LocalizedSprite"/>,
    ///     refreshing automatically whenever the active locale changes.
    /// </summary>
    [AddComponentMenu("Simple UI/Localization/Localized Sprite")]
    [RequireComponent(typeof(Image))]
    public class UILocalizedSpriteObject : UIObjectBase
    {
        [SerializeField] private LocalizedSprite _localizedSprite = new();

        [field: SerializeField, HideInInspector] private Image ImageReference { get; set; }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            _localizedSprite.AssetChanged += OnSpriteChanged;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            _localizedSprite.AssetChanged -= OnSpriteChanged;
        }

        private void OnSpriteChanged(Sprite value) => ImageReference.sprite = value;

        protected override void OnValidate()
        {
            base.OnValidate();
            ImageReference = GetComponent<Image>();
            Assert.IsNotNull(ImageReference, "UILocalizedSpriteObject requires an Image component.");
        }
    }
}
