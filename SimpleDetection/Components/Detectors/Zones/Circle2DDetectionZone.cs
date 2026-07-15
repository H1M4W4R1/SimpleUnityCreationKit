using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A circular 2D trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Circle2DDetectionZone : IDetectionZone
    {
        private readonly float2 position;
        private readonly float radiusSq;

        public Circle2DDetectionZone(float2 position, float radius)
        {
            this.position = position;
            radiusSq = radius * radius;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition) => math.lengthsq(detectionPosition.xy - position) <= radiusSq;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            Gizmos.DrawWireSphere(new Vector3(position.x, position.y, 0f), math.sqrt(radiusSq));
#endif
        }
    }
}
