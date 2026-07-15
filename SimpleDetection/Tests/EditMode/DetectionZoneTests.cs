using NUnit.Framework;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Systems.SimpleDetection.Data.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Tests
{
    public sealed class DetectionZoneTests : SimpleDetectionTestBase
    {
        [Test]
        public void RaycastingCircle2D_IncludesCenterAndBoundaryButRejectsOutside()
        {
            RaycastingCircle2DDetectionZone zone = new RaycastingCircle2DDetectionZone(new float2(0f, 0f), 2f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(2f, 0f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(2.01f, 0f, 0f)));
        }

        [Test]
        public void RaycastingCircle2D_ReportsSeenOutsideAndObstructed()
        {
            RaycastingCircle2DDetectionZone zone = new RaycastingCircle2DDetectionZone(new float2(0f, 0f), 2f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(0f, 0f, 0f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(3f, 0f, 0f), 1));

            GameObject obstacle = Track(new GameObject("2D Obstacle"));
            obstacle.transform.position = new Vector3(0.5f, 0f, 0f);
            BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.25f, 0.25f);
            Physics2D.SyncTransforms();

            Assert.AreEqual(SpotResult.InsideObstructed, zone.IsPointSeen(new float3(1f, 0f, 0f), 1));
        }

        [Test]
        public void Frustum2D_UsesForwardHalfAngleAndRadius()
        {
            RaycastingFrustum2DDetectionZone zone = new RaycastingFrustum2DDetectionZone(
                new float2(0f, 0f),
                new float2(0f, 1f),
                90f,
                3f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 2f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(1f, 1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(2f, 1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, -1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 3.1f, 0f)));
        }

        [Test]
        public void Sphere3D_IncludesCenterAndBoundaryButRejectsOutside()
        {
            RaycastingSphere3DDetectionZone zone = new RaycastingSphere3DDetectionZone(new float3(0f, 0f, 0f), 2f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 2f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 2.01f)));
        }

        [Test]
        public void RaycastingSphere3D_ReportsSeenOutsideAndObstructed()
        {
            RaycastingSphere3DDetectionZone zone = new RaycastingSphere3DDetectionZone(new float3(0f, 0f, 0f), 2f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(0f, 0f, 0f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(3f, 0f, 0f), 1));

            GameObject obstacle = Track(new GameObject("3D Obstacle"));
            obstacle.transform.position = new Vector3(0.5f, 0f, 0f);
            BoxCollider collider = obstacle.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.25f, 0.25f, 0.25f);
            Physics.SyncTransforms();

            Assert.AreEqual(SpotResult.InsideObstructed, zone.IsPointSeen(new float3(1f, 0f, 0f), 1));
        }

        [Test]
        public void Frustum3D_UsesNearFarFieldOfViewAndAspect()
        {
            RaycastingFrustum3DDetectionZone zone = new RaycastingFrustum3DDetectionZone(
                new float3(0f, 0f, 0f),
                quaternion.identity,
                10f,
                1f,
                90f,
                1f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 1f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(2f, 0f, 5f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 0.5f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 11f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(6f, 0f, 5f)));
        }

        [Test]
        public void Circle2D_ReportsSeenWhenAnObstacleIsBetweenTheDetectorAndPoint()
        {
            Circle2DDetectionZone zone = new Circle2DDetectionZone(new float2(0f, 0f), 2f);

            GameObject obstacle = Track(new GameObject("2D Obstacle"));
            obstacle.transform.position = new Vector3(0.5f, 0f, 0f);
            BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.25f, 0.25f);
            Physics2D.SyncTransforms();

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(1f, 0f, 0f), 1));
        }

        [Test]
        public void Box2D_UsesSizeAndRotation()
        {
            Box2DDetectionZone zone = new Box2DDetectionZone(new float2(0f, 0f), new float2(2f, 4f), math.radians(90f));

            Assert.IsTrue(zone.IsPointInZone(new float3(1.9f, 0f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 1.1f, 0f)));
        }

        [Test]
        public void Box3D_UsesRotationAndAllThreeDimensions()
        {
            Box3DDetectionZone zone = new Box3DDetectionZone(new float3(0f, 0f, 0f), quaternion.RotateY(math.radians(90f)),
                new float3(2f, 4f, 6f));

            Assert.IsTrue(zone.IsPointInZone(new float3(2.9f, 0f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 2.1f, 0f)));
        }

        [Test]
        public void Cylinder3D_UsesRadiusAndHeight()
        {
            Cylinder3DDetectionZone zone = new Cylinder3DDetectionZone(new float3(0f, 0f, 0f), quaternion.identity, 2f, 4f);

            Assert.IsTrue(zone.IsPointInZone(new float3(1.9f, 1.9f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(2.1f, 0f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 2.1f, 0f)));
        }

        [Test]
        public void Frustum2D_IgnoresObstacles()
        {
            Frustum2DDetectionZone zone = new Frustum2DDetectionZone(new float2(0f, 0f), new float2(0f, 1f), 90f, 3f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(0f, 2f, 0f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(0f, -1f, 0f), 1));
        }

        [Test]
        public void Frustum3D_UsesNearFarFieldOfViewAndAspectWithoutRaycasting()
        {
            Frustum3DDetectionZone zone = new Frustum3DDetectionZone(new float3(0f, 0f, 0f), quaternion.identity,
                10f, 1f, 90f, 1f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(2f, 0f, 5f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(0f, 0f, 0.5f), 1));
        }
    }
}
