using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    /// <summary> A 2D vision-cone detector that ignores line of sight. </summary>
    public abstract class Frustum2DDetector : ObjectDetectorBase
    {
        [SerializeField] [Min(0.01f)] private float angle = 45f;
        [SerializeField] [Min(0.01f)] private float radius = 2f;

        protected override IDetectionZone GetDetectionZone()
        {
            Transform detectorTransform = CachedTransform;
            float3 position = detectorTransform.position;
            float3 forward = detectorTransform.up;
            return new Frustum2DDetectionZone(position.xy, forward.xy, angle, radius);
        }
    }
}
