using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    /// <summary> A cylinder, aligned with the detector's local Y axis, that ignores line of sight. </summary>
    public abstract class Cylinder3DDetector : ObjectDetectorBase
    {
        [SerializeField] [Min(0.01f)] private float height = 2f;
        [SerializeField] [Min(0.01f)] private float radius = 2f;

        protected override IDetectionZone GetDetectionZone()
        {
            Transform detectorTransform = CachedTransform;
            return new Cylinder3DDetectionZone(detectorTransform.position, detectorTransform.rotation, radius, height);
        }
    }
}
