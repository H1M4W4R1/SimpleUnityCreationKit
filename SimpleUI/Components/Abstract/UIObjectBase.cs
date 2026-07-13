using DG.Tweening;
using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract.Markers;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Components.Animations.Abstract;
using Systems.SimpleUI.Components.Windows;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Systems.SimpleUI.Components.Abstract
{
    /// <summary>
    ///     Represents a user interface object
    /// </summary>
    public abstract class UIObjectBase : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        [field: SerializeField, HideInInspector] private UIAnimationBase ShowAnimationReference { get; set; }
        [field: SerializeField, HideInInspector] private UIAnimationBase HideAnimationReference { get; set; }

        private Sequence _currentShowHideAnimationSequence;

        private bool _isDragging;

        [field: SerializeField, HideInInspector] [CanBeNull]
        protected internal UIWindowBase WindowContainerReference { get; private set; }

        [field: SerializeField, HideInInspector] [NotNull]
        protected internal RectTransform RectTransformReference { get; private set; } = null!;

        [field: SerializeField, HideInInspector] [CanBeNull] protected internal CanvasGroup CanvasGroupReference
        {
            get;
            private set;
        }

        [field: SerializeField, HideInInspector] [NotNull] protected internal Canvas ClosestCanvasReference
        {
            get;
            private set;
        } = null!;

        [field: SerializeField, HideInInspector] [CanBeNull] protected internal Canvas RootCanvasReference
        {
            get;
            private set;
        }

        [field: SerializeField, HideInInspector] [NotNull] protected internal GameObject GameObjectReference
        {
            get;
            private set;
        } = null!;


        /// <summary>
        ///     Check if element is not hidden
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        ///     Checks if the object is destroyed
        /// </summary>
        protected bool IsDestroyed { get; private set; }

        /// <summary>
        ///     Method used to assign components from the game object
        /// </summary>
        protected virtual void AssignComponents()
        {
            GameObjectReference = gameObject;
            RectTransformReference = GetComponent<RectTransform>();
            CanvasGroupReference = GetComponent<CanvasGroup>();

            ClosestCanvasReference = GetComponent<Canvas>();
            if (!ClosestCanvasReference) ClosestCanvasReference = GetComponentInParent<Canvas>();
            if (ClosestCanvasReference) RootCanvasReference = ClosestCanvasReference.rootCanvas;

            if (this is not UIWindowBase) WindowContainerReference = GetComponentInParent<UIWindowBase>();

            ShowAnimationReference = GetComponent<IUIShowAnimation>() as UIAnimationBase;
            HideAnimationReference = GetComponent<IUIHideAnimation>() as UIAnimationBase;
        }

        /// <summary>
        ///     Method used to attach events of components to this object
        /// </summary>
        protected virtual void AttachEvents()
        {
        }

        /// <summary>
        ///     Method used to detach events of components from this object
        /// </summary>
        protected virtual void DetachEvents()
        {
        }

        /// <summary>
        ///     Called when the object is setup
        /// </summary>
        protected virtual void OnSetupComplete()
        {
        }

        /// <summary>
        ///     Called when the object is torn down
        ///     Warning: events are detached at this point
        /// </summary>
        protected virtual void OnTearDownComplete()
        {
        }

        /// <summary>
        ///     Tries to perform first render, executed after
        ///     setup was complete
        /// </summary>
        private void TryPerformFirstRender()
        {
            if (this is IRenderable renderable) renderable.Render();
        }

        /// <summary>
        ///     Executed when the object is refreshed
        ///     (on tick and when dirty)
        /// </summary>
        protected virtual void OnRefresh()
        {
        }

        /// <summary>
        ///     Executed every frame
        /// </summary>
        protected virtual void OnTick()
        {
        }

        protected virtual void OnLateSetupComplete()
        {
            // Do nothing
        }

        protected internal void Show()
        {
            IsVisible = true;
            _currentShowHideAnimationSequence?.Kill();

            IUIShowAnimation showAnimation = ShowAnimationReference as IUIShowAnimation;

            // If no animation, activate and skip
            if (showAnimation is null)
            {
                gameObject.SetActive(true);
                return;
            }

            // Play nice animation
            _currentShowHideAnimationSequence = showAnimation.OnShow().Play();
        }

        protected internal void Hide()
        {
            IsVisible = false;
            _currentShowHideAnimationSequence?.Kill();

            IUIHideAnimation hideAnimation = HideAnimationReference as IUIHideAnimation;

            // If no animation, deactivate and skip
            if (hideAnimation is null)
            {
                gameObject.SetActive(false);
                return;
            }

            // Play nice animation
            _currentShowHideAnimationSequence = hideAnimation.OnHide()
                .Play();
        }


        protected void Awake()
        {
            // Access window container if this is not a window
            if (this is not UIWindowBase) WindowContainerReference = GetComponentInParent<UIWindowBase>();

            AssignComponents();

            // Set visibility
            IsVisible = gameObject.activeSelf;
        }

        private void Start()
        {
            // Call first build complete
            OnLateSetupComplete();
        }


        protected void OnEnable()
        {
            AttachEvents();
            OnSetupComplete();
            TryPerformFirstRender();

            if (CanvasGroupReference) CanvasGroupReference.interactable = true;
        }

        private void OnDisable()
        {
            DetachEvents();

            if (CanvasGroupReference) CanvasGroupReference.interactable = false;
        }

        protected void OnDestroy()
        {
            IsDestroyed = true;
            OnTearDownComplete();
        }

        protected void Update()
        {
            IWithContext withContext = this as IWithContext;

            // Check if context is dirty
            if (!ReferenceEquals(withContext, null)) withContext.ValidateContext();

            // Perform tick
            OnTick();

            // Skip if context is not dirty (only if context is available)
            if (this is IWithContext {IsDirty: false}) return;

            // Render object if necessary
            if (this is IRenderable renderable) renderable.Render();

            // Call refresh event
            OnRefresh();

            // Reset dirty status
            if (!ReferenceEquals(withContext, null)) withContext.SetDirty(false);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // Handle drag to be ignored to prevent focusing windows with dragging components...
            if (_isDragging) return;

            // Check if this is window and focus on it
            if (this is UIWindowBase window)
            {
                window.Focus();
                return;
            }

            // Focus on window container
            if (!WindowContainerReference) return;
            WindowContainerReference.Focus();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Focus on window container
            if (this is UIWindowBase window) window.Focus();
            _isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        protected virtual void OnValidate()
        {
            // Always good
            GameObjectReference = gameObject;

            // RectTransform
            RectTransformReference = GetComponent<RectTransform>();
            Assert.IsNotNull(RectTransformReference, "UIObjectBase requires a RectTransform component");

            // Canvas
            ClosestCanvasReference = GetComponent<Canvas>();
            if (!ClosestCanvasReference) ClosestCanvasReference = GetComponentInParent<Canvas>();

            if (!string.IsNullOrEmpty(GameObjectReference.scene.name))
                Assert.IsNotNull(ClosestCanvasReference,
                    "UIObjectBase requires a Canvas component to be in parent or on object itself.");

            if(ClosestCanvasReference) RootCanvasReference = ClosestCanvasReference.rootCanvas;            
            
            // Optional
            WindowContainerReference = GetComponentInParent<UIWindowBase>();
            ShowAnimationReference = GetComponent<IUIShowAnimation>() as UIAnimationBase;
            HideAnimationReference = GetComponent<IUIHideAnimation>() as UIAnimationBase;
            CanvasGroupReference = GetComponent<CanvasGroup>();
        }

#if UNITY_INCLUDE_TESTS
        internal void InitializeForTests()
        {
            AssignComponents();
            DetachEvents();
            AttachEvents();
        }
#endif
    }
}
