using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    /// <summary> An oriented box trigger-style detector. </summary>
    public abstract class Box3DDetector : ObjectDetectorBase
    {
        [SerializeField] private Vector3 size = new(2f, 2f, 2f);

        protected override IDetectionZone GetDetectionZone()
        {
            Transform detectorTransform = CachedTransform;
            float3 zoneSize = math.max((float3) size, new float3(0.01f));
            return new Box3DDetectionZone(detectorTransform.position, detectorTransform.rotation, zoneSize);
        }
    }
}
