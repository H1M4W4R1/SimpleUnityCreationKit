using JetBrains.Annotations;
using UnityEngine;

namespace Systems.SimpleBuilding.Components
{
    /// <summary>
    ///     Projects building operations from the center of a camera's viewport.
    /// </summary>
    public sealed class CameraBuildingRaycaster : BuildingRaycasterBase
    {
        [SerializeField] [CanBeNull] private Camera _camera;

        protected override bool TryGetRay(out Ray ray)
        {
            if (ReferenceEquals(_camera, null) || !_camera) _camera = Camera.main;
            if (ReferenceEquals(_camera, null) || !_camera)
            {
                ray = default;
                return false;
            }

            ray = new Ray(_camera.transform.position, _camera.transform.forward);
            return true;
        }
    }
}
