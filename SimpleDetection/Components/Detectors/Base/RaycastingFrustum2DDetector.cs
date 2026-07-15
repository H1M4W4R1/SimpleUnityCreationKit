using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    // ReSharper disable once ClassCanBeSealed.Global
    public abstract class RaycastingFrustum2DDetector : ObjectDetectorBase
    {
        [SerializeField] [Min(0.01f)] private float angle = 45f;
        [SerializeField] [Min(0.01f)] private float radius = 2f;
        
        protected override IDetectionZone GetDetectionZone()
        {
            Transform objTransform = CachedTransform;
            float3 position = objTransform.position;
            float3 forward = objTransform.up;

            return new RaycastingFrustum2DDetectionZone(position.xy, forward.xy, angle, radius);
        }
    }
}
