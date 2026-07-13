using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Features.Positioning
{
    [ExecuteAlways] [RequireComponent(typeof(RectTransform))]
    public sealed class LimitObjectToParent : MonoBehaviour
    {
        /// <summary>
        ///     Object's RectTransform
        /// </summary>
        [field: SerializeField, HideInInspector] private RectTransform rectTransform;

        /// <summary>
        ///     Parent RectTransform to limit the object to
        /// </summary>
        [field: SerializeField, HideInInspector] private RectTransform parentRectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = rectTransform.parent as RectTransform;
        }

#if UNITY_EDITOR
        private void FixedUpdate()
        {
            parentRectTransform = rectTransform.parent as RectTransform;
        }
#endif

        private void LateUpdate()
        {
            if (!rectTransform || !parentRectTransform) return;
            KeepInsideParent();
        }

        /// <summary>
        ///     Keeps the object inside the parent
        /// </summary>
        private void KeepInsideParent()
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            Vector3[] parentCorners = new Vector3[4];
            parentRectTransform.GetWorldCorners(parentCorners);

            Vector3 offset = Vector3.zero;

            // Left
            float delta = parentCorners[0].x - worldCorners[0].x;
            if (delta > 0) offset.x += delta;

            // Right
            delta = parentCorners[2].x - worldCorners[2].x;
            if (delta < 0) offset.x += delta;

            // Bottom
            delta = parentCorners[0].y - worldCorners[0].y;
            if (delta > 0) offset.y += delta;

            // Top
            delta = parentCorners[2].y - worldCorners[2].y;
            if (delta < 0) offset.y += delta;

            if (!Mathf.Approximately(offset.x, 0) || !Mathf.Approximately(offset.y, 0) ||
                !Mathf.Approximately(offset.z, 0))
            {
                rectTransform.position += offset;
            }
        }

#if UNITY_INCLUDE_TESTS
        internal void ApplyLimitForTests()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = rectTransform.parent as RectTransform;
            KeepInsideParent();
        }
#endif

        private void OnValidate()
        {
            rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(rectTransform, "LimitObjectToParent requires a RectTransform component");

            if (string.IsNullOrEmpty(gameObject.scene.name)) return;
            parentRectTransform = rectTransform.parent as RectTransform;
            Assert.IsNotNull(parentRectTransform, "LimitObjectToParent requires a parent RectTransform component");
        }
    }
}
