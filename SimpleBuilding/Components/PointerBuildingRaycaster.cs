using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Projects building operations from a screen-space pointer position supplied by UI or input code.
    /// </summary>
    public sealed class PointerBuildingRaycaster : BuildingRaycasterBase
    {
        [SerializeField] [CanBeNull] private Camera _camera;
        [SerializeField] private bool _useMousePositionWhenUnset = true;
        private Vector2 _pointerPosition;
        private bool _hasPointerPosition;

        /// <summary>
        ///     Updates the pointer position, for example from an EventSystem pointer-move callback.
        /// </summary>
        public void SetPointerPosition(Vector2 pointerPosition)
        {
            _pointerPosition = pointerPosition;
            _hasPointerPosition = true;
        }

        public void ClearPointerPosition()
        {
            _hasPointerPosition = false;
        }

        protected override bool TryGetRay(out Ray ray)
        {
            if (ReferenceEquals(_camera, null) || !_camera) _camera = Camera.main;
            if (ReferenceEquals(_camera, null) || !_camera)
            {
                ray = default;
                return false;
            }

            Vector2 pointerPosition = _pointerPosition;
            if (!_hasPointerPosition)
            {
                if (!_useMousePositionWhenUnset)
                {
                    ray = default;
                    return false;
                }

                pointerPosition = Input.mousePosition;
            }

            ray = _camera.ScreenPointToRay(pointerPosition);
            return true;
        }
    }
}
