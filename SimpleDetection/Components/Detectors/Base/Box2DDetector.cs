using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    /// <summary> A rotated rectangular 2D trigger-style detector. </summary>
    public abstract class Box2DDetector : ObjectDetectorBase
    {
        [SerializeField] private Vector2 size = new(2f, 2f);

        protected override IDetectionZone GetDetectionZone()
        {
            Transform detectorTransform = CachedTransform;
            float3 position = detectorTransform.position;
            float2 zoneSize = math.max((float2) size, new float2(0.01f));
            float rotationRadians = math.radians(detectorTransform.eulerAngles.z);
            return new Box2DDetectionZone(position.xy, zoneSize, rotationRadians);
        }
    }
}
