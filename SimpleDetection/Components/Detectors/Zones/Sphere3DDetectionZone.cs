using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A spherical trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Sphere3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly float radiusSq;

        public Sphere3DDetectionZone(float3 position, float radius)
        {
            this.position = position;
            radiusSq = radius * radius;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition) => math.lengthsq(detectionPosition - position) <= radiusSq;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            Gizmos.DrawWireSphere(position, math.sqrt(radiusSq));
#endif
        }
    }
}
