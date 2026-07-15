using System.Runtime.CompilerServices;
using Systems.SimpleCore.Utility;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    /// <summary> A perspective-frustum trigger-style detection zone. </summary>
    [BurstCompile] public readonly struct Frustum3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly quaternion rotation;
        private readonly float farPlaneDistance;
        private readonly float nearPlaneDistance;
        private readonly float horizontalFieldOfView;
        private readonly float aspectRatio;

        public Frustum3DDetectionZone(float3 position, quaternion rotation, float farPlaneDistance,
            float nearPlaneDistance, float horizontalFieldOfView, float aspectRatio)
        {
            this.position = position;
            this.rotation = rotation;
            this.farPlaneDistance = farPlaneDistance;
            this.nearPlaneDistance = nearPlaneDistance;
            this.horizontalFieldOfView = horizontalFieldOfView;
            this.aspectRatio = aspectRatio;
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
        {
            float3 localPosition = math.mul(math.inverse(rotation), detectionPosition - position);
            if (localPosition.z < nearPlaneDistance || localPosition.z > farPlaneDistance) return false;

            float halfWidth = localPosition.z * math.tan(math.radians(horizontalFieldOfView * 0.5f));
            float halfHeight = halfWidth / aspectRatio;
            return math.abs(localPosition.x) <= halfWidth && math.abs(localPosition.y) <= halfHeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask) =>
            IsPointInZone(detectionPosition) ? SpotResult.InsideSeen : SpotResult.Outside;

        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            NativeArray<float3x2> lines = default;
            FrustumUtil.ComputeFrustumGizmosLines(position, rotation, GetFarPlaneHeight(), aspectRatio,
                nearPlaneDistance, farPlaneDistance, out lines);
            for (int index = 0; index < lines.Length; index++) Gizmos.DrawLine(lines[index].c0, lines[index].c1);
            lines.Dispose();
#endif
        }

        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] private float GetFarPlaneHeight()
        {
            float farPlaneHalfWidth = farPlaneDistance * math.tan(math.radians(horizontalFieldOfView * 0.5f));
            return farPlaneHalfWidth * 2f / aspectRatio;
        }
    }
}
