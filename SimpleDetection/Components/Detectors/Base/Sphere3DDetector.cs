using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Base
{
    /// <summary> A sphere detector that detects every eligible object inside its zone. </summary>
    public abstract class Sphere3DDetector : ObjectDetectorBase
    {
        [SerializeField] [Min(0.01f)] private float radius = 2f;

        protected override IDetectionZone GetDetectionZone()
        {
            float3 position = CachedTransform.position;
            return new Sphere3DDetectionZone(position, radius);
        }
    }
}
