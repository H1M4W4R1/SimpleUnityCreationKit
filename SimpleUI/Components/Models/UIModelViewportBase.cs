using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract;
using Systems.SimpleUI.Components.Abstract.Markers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Systems.SimpleUI.Components.Models
{
    /// <summary>
    ///     Represents a viewport for model - used for displaying 3D models in UI
    ///     This version is designed to render static models with no rotating camera option.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public abstract class UIModelViewportBase : UIObjectWithContextBase<GameObject>,
        IRenderable<GameObject>
    {
        /// <summary>
        ///     Maximum number of models to cache
        /// </summary>
        [field: SerializeField] protected int ModelCacheSize { get; private set; }

        /// <summary>
        ///     Camera used to render viewport
        /// </summary>
        [field: SerializeField] [NotNull] protected Camera ViewportCamera { get; private set; } = null!;

        /// <summary>
        ///     Viewport location
        /// </summary>
        [field: SerializeField] [Tooltip("Models will be created here")] [NotNull]
        protected Transform ViewportLocation { get; private set; } = null!;

        [field: SerializeField, HideInInspector] [NotNull] protected RawImage ImageRenderer { get; private set; } =
            null!;

        /// <summary>
        ///     Model currently rendered in viewport
        /// </summary>
        [CanBeNull] protected GameObject CurrentlyRenderedModelPrefab { get; private set; }

        /// <summary>
        ///     Cache for models
        /// </summary>
        protected List<GameObject> PrefabCache { get; } = new();

        /// <summary>
        ///     Cache for models
        /// </summary>
        protected List<GameObject> ModelCache { get; } = new();

        /// <summary>
        ///     Gets the camera texture
        /// </summary>
        protected RenderTexture CameraTexture { get; private set; }

        protected override void AssignComponents()
        {
            base.AssignComponents();
            Assert.IsNotNull(ViewportCamera, "Viewport camera cannot be null");
            CameraTexture = ViewportCamera.targetTexture;
            Assert.IsNotNull(CameraTexture,
                "Camera texture cannot be null. Assign RenderTexture to your ViewportCamera Outpu Section!");
        }

        /// <summary>
        ///     Changes model to cached one or creates new one if not cached
        /// </summary>
        /// <param name="model">Model to cache</param>
        /// <param name="previousModel">Previous model that was rendered</param>
        /// <returns>Model that was stored in cache</returns>
        [NotNull] protected GameObject ChangeModel(
            [NotNull] GameObject model,
            [CanBeNull] out GameObject previousModel)
        {
            // Get last model if any
            previousModel = ModelCache.Count > 0 ? ModelCache[^1] : null;

            int cachedModelIndex = PrefabCache.IndexOf(model);
            GameObject createdModel = null;

            // Remove model if cache contains it
            // second condition prevents from wtf fuckups
            if (cachedModelIndex >= 0 && cachedModelIndex < ModelCache.Count)
            {
                createdModel = ModelCache[cachedModelIndex];
                ModelCache.RemoveAt(cachedModelIndex);
                PrefabCache.RemoveAt(cachedModelIndex);
            }

            // Remove oldest model if cache is full
            if (ModelCache.Count + 1 > ModelCacheSize && ModelCache.Count > 0)
            {
#if UNITY_EDITOR
                DestroyImmediate(ModelCache[0]);
#else
                Destroy(ModelCache[0]);
#endif

                ModelCache.RemoveAt(0);
                PrefabCache.RemoveAt(0);
            }

            // If model was at cache re-add it at end and return
            if (createdModel)
            {
                ModelCache.Add(createdModel);
                PrefabCache.Add(model);
                return createdModel;
            }

            // Otherwise create new model at viewport
            createdModel = Instantiate(model, ViewportLocation.position, ViewportLocation.rotation,
                ViewportLocation);
            ModelCache.Add(createdModel);
            PrefabCache.Add(model);
            return createdModel;
        }

        public void OnRender(GameObject withContext)
        {
            // If model is already rendered, do nothing
            if (ReferenceEquals(withContext, CurrentlyRenderedModelPrefab)) return;

            // Create instance of new model
            GameObject newModel = ChangeModel(withContext, out GameObject previousModel);

            // Swap models
            if (previousModel) previousModel.SetActive(false);
            newModel.SetActive(true);

            // Set current model prefab to new one
            CurrentlyRenderedModelPrefab = withContext;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            ImageRenderer = GetComponent<RawImage>();
            Assert.IsNotNull(ImageRenderer, "UIModelViewportBase requires a RawImage component");
            
            if (string.IsNullOrEmpty(gameObject.scene.name)) return;
            Assert.IsNotNull(ViewportLocation, "Viewport location cannot be null");
            Assert.IsNotNull(ViewportCamera, "Viewport camera cannot be null");
        }

        public override void ValidateContext()
        {
            base.ValidateContext();

            // Check if model has changed
            if (ReferenceEquals(CurrentlyRenderedModelPrefab, Context)) return;

            // Notify dirty
            RequestRefresh();
        }
    }
}