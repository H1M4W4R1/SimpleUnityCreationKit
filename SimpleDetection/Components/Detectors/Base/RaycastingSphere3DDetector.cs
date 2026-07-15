using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    // ReSharper disable once ClassCanBeSealed.Global
    public abstract class RaycastingSphere3DDetector : ObjectDetectorBase
    {
        [SerializeField] [Min(0.01f)] private float radius = 2f;

        protected override IDetectionZone GetDetectionZone()
        {
            float3 position = CachedTransform.position;
            return new RaycastingSphere3DDetectionZone(position, radius);
        }
    }
}
