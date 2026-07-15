using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A rotated rectangular 2D trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Box2DDetectionZone : IDetectionZone
    {
        private readonly float2 position;
        private readonly float2 halfExtents;
        private readonly float rotationRadians;

        public Box2DDetectionZone(float2 position, float2 size, float rotationRadians)
        {
            this.position = position;
            halfExtents = size * 0.5f;
            this.rotationRadians = rotationRadians;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float2 relativePosition = detectionPosition.xy - position;
            float sine = math.sin(rotationRadians);
            float cosine = math.cos(rotationRadians);
            float2 localPosition = new(cosine * relativePosition.x + sine * relativePosition.y,
                -sine * relativePosition.x + cosine * relativePosition.y);
            return math.all(math.abs(localPosition) <= halfExtents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(new Vector3(position.x, position.y, 0f),
                Quaternion.Euler(0f, 0f, math.degrees(rotationRadians)), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfExtents.x * 2f, halfExtents.y * 2f, 0f));
            Gizmos.matrix = previousMatrix;
#endif
        }
    }
}
