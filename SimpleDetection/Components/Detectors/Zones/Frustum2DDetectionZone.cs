using System.Runtime.CompilerServices;
using Systems.SimpleCore.Utility;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A 2D vision-cone trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Frustum2DDetectionZone : IDetectionZone
    {
        private readonly float2 position;
        private readonly float2 forward;
        private readonly float angle;
        private readonly float radius;
        private readonly float radiusSq;

        public Frustum2DDetectionZone(float2 position, float2 forward, float angle, float radius)
        {
            this.position = position;
            this.forward = math.normalizesafe(forward, new float2(0f, 1f));
            this.angle = angle;
            this.radius = radius;
            radiusSq = radius * radius;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float2 relativePosition = detectionPosition.xy - position;
            if (math.lengthsq(relativePosition) > radiusSq) return false;
            if (math.lengthsq(relativePosition) < math.EPSILON) return true;

            float dotProduct = math.dot(math.normalize(relativePosition), forward);
            return dotProduct >= math.cos(math.radians(angle * 0.5f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            const int Resolution = 16;
            float halfAngle = math.radians(angle * 0.5f);
            float3 origin = new(position, 0f);
            float3 previousPoint = origin + new float3(MathExtensions.Rotate(forward, -halfAngle) * radius, 0f);
            Gizmos.DrawLine(origin, previousPoint);

            for (int index = 1; index <= Resolution; index++)
            {
                float interpolation = index / (float) Resolution;
                float currentAngle = math.lerp(-halfAngle, halfAngle, interpolation);
                float3 currentPoint = origin + new float3(MathExtensions.Rotate(forward, currentAngle) * radius, 0f);
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            Gizmos.DrawLine(previousPoint, origin);
#endif
        }
    }
}
