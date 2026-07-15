using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> An oriented box trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Box3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly quaternion rotation;
        private readonly float3 halfExtents;

        public Box3DDetectionZone(float3 position, quaternion rotation, float3 size)
        {
            this.position = position;
            this.rotation = rotation;
            halfExtents = size * 0.5f;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float3 localPosition = math.mul(math.inverse(rotation), detectionPosition - position);
            return math.all(math.abs(localPosition) <= halfExtents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
            Gizmos.matrix = previousMatrix;
#endif
        }
    }
}
