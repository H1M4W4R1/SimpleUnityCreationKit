using Systems.SimpleUI.Components.Abstract;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Images
{
    /// <summary>
    ///     Drives a <see cref="RawImage"/> texture from a <see cref="LocalizedTexture"/>,
    ///     refreshing automatically whenever the active locale changes.
    /// </summary>
    [AddComponentMenu("Simple UI/Localization/Localized Texture")]
    [RequireComponent(typeof(RawImage))]
    public class UILocalizedTextureObject : UIObjectBase
    {
        [SerializeField] private LocalizedTexture _localizedTexture = new();

        [field: SerializeField, HideInInspector] private RawImage RawImageReference { get; set; }

        protected override void AttachEvents()
        {
            base.AttachEvents();
            _localizedTexture.AssetChanged += OnTextureChanged;
        }

        protected override void DetachEvents()
        {
            base.DetachEvents();
            _localizedTexture.AssetChanged -= OnTextureChanged;
        }

        private void OnTextureChanged(Texture value) => RawImageReference.texture = value;

        protected override void OnValidate()
        {
            base.OnValidate();
            RawImageReference = GetComponent<RawImage>();
            Assert.IsNotNull(RawImageReference, "UILocalizedTextureObject requires a RawImage component.");
        }
    }
}
