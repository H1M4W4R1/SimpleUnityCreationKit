using Systems.SimpleUI.Components.Canvases;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Features.Positioning
{
    [ExecuteAlways] [RequireComponent(typeof(RectTransform))]
    public sealed class LimitObjectToViewport : MonoBehaviour
    {
        /// <summary>
        ///     Viewport that the object is limited to
        /// </summary>
        private RectTransform _viewport;

        /// <summary>
        ///     RectTransform of the object
        /// </summary>
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            UIRootCanvasBase rootCanvas = GetComponentInParent<UIRootCanvasBase>();
            if (rootCanvas) _viewport = rootCanvas.GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            // Ensure components are assigned
            if (!_viewport || !_rectTransform) return;
            KeepInsideViewport();
        }

        /// <summary>
        ///     Keeps the object inside the viewport
        /// </summary>
        private void KeepInsideViewport()
        {
            // Transform corners into world space
            Vector3[] worldCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(worldCorners);

            Vector3[] viewportCorners = new Vector3[4];
            _viewport.GetWorldCorners(viewportCorners);

            Vector3 offset = Vector3.zero;

            // Left
            float delta = viewportCorners[0].x - worldCorners[0].x;
            if (delta > 0) offset.x += delta;

            // Right
            delta = viewportCorners[2].x - worldCorners[2].x;
            if (delta < 0) offset.x += delta;

            // Bottom
            delta = viewportCorners[0].y - worldCorners[0].y;
            if (delta > 0) offset.y += delta;

            // Top
            delta = viewportCorners[2].y - worldCorners[2].y;
            if (delta < 0) offset.y += delta;

            // Apply correction
            if (!Mathf.Approximately(offset.x, 0) || !Mathf.Approximately(offset.y, 0) ||
                !Mathf.Approximately(offset.z, 0))
            {
                _rectTransform.position += offset;
            }
        }

#if UNITY_INCLUDE_TESTS
        internal void ApplyLimitForTests()
        {
            _rectTransform = GetComponent<RectTransform>();
            UIRootCanvasBase rootCanvas = GetComponentInParent<UIRootCanvasBase>();
            if (rootCanvas) _viewport = rootCanvas.GetComponent<RectTransform>();
            KeepInsideViewport();
        }
#endif

        private void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(_rectTransform,
                "LimitObjectToViewport requires a RectTransform component on the object.");

            if (string.IsNullOrEmpty(gameObject.scene.name)) return;
            UIRootCanvasBase rootCanvas = GetComponentInParent<UIRootCanvasBase>();
            Assert.IsNotNull(rootCanvas,
                "LimitObjectToViewport requires a UIRootCanvasBase component in parent or on object itself.");

            _viewport = rootCanvas.GetComponent<RectTransform>();
            Assert.IsNotNull(_viewport,
                "LimitObjectToViewport requires a RectTransform component on the UIRootCanvasBase.");
        }
    }
}
