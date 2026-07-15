using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A cylinder, aligned to the detector's local Y axis, used as a trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Cylinder3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly quaternion rotation;
        private readonly float radiusSq;
        private readonly float halfHeight;

        public Cylinder3DDetectionZone(float3 position, quaternion rotation, float radius, float height)
        {
            this.position = position;
            this.rotation = rotation;
            radiusSq = radius * radius;
            halfHeight = height * 0.5f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float3 localPosition = math.mul(math.inverse(rotation), detectionPosition - position);
            return math.abs(localPosition.y) <= halfHeight &&
                   math.lengthsq(localPosition.xz) <= radiusSq;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.up * halfHeight, math.sqrt(radiusSq));
            Gizmos.DrawWireSphere(Vector3.down * halfHeight, math.sqrt(radiusSq));
            Gizmos.DrawLine(new Vector3(math.sqrt(radiusSq), -halfHeight, 0f), new Vector3(math.sqrt(radiusSq), halfHeight, 0f));
            Gizmos.DrawLine(new Vector3(-math.sqrt(radiusSq), -halfHeight, 0f), new Vector3(-math.sqrt(radiusSq), halfHeight, 0f));
            Gizmos.DrawLine(new Vector3(0f, -halfHeight, math.sqrt(radiusSq)), new Vector3(0f, halfHeight, math.sqrt(radiusSq)));
            Gizmos.DrawLine(new Vector3(0f, -halfHeight, -math.sqrt(radiusSq)), new Vector3(0f, halfHeight, -math.sqrt(radiusSq)));
            Gizmos.matrix = previousMatrix;
#endif
        }
    }
}
