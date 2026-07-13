using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleBuilding.Abstract;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data.Context;
using Systems.SimpleBuilding.Operations;
using Systems.SimpleBuilding.Utility;
using Systems.SimpleCore.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Tests
{
    public sealed class BuildingAPITests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int objectIndex = _createdObjects.Count - 1; objectIndex >= 0; objectIndex--)
            {
                Object createdObject = _createdObjects[objectIndex];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void TryBuild_ConsumesResourcesAndInvokesPlacementCallbacksWithContext()
        {
            TestBuildingEntry entry = CreateEntry<TestBuilding>();
            Vector3 position = new Vector3(2f, 3f, 4f);
            Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);

            OperationResult result = BuildingAPI.TryBuild(entry, position, rotation, out BuildingBase building);

            Assert.IsTrue(result);
            Assert.AreEqual(BuildingOperations.SUCCESS_PLACED, result.resultCode);
            Assert.IsNotNull(building);
            Track(building.gameObject);
            Assert.AreEqual(entry, building.Entry);
            Assert.AreEqual(position, building.transform.position);
            Assert.Less(Quaternion.Angle(rotation, building.transform.rotation), 0.001f);
            Assert.AreEqual(1, entry.ConsumeCallCount);
            Assert.AreEqual(1, entry.PlacedCallCount);
            Assert.AreEqual(position, entry.LastPlacementPosition);
            TestBuilding testBuilding = building as TestBuilding;
            Assert.IsNotNull(testBuilding);
            Assert.AreEqual(1, testBuilding.PlacedCallCount);
        }

        [Test]
        public void TryBuild_WhenResourceConsumptionFails_ReportsPlacementFailureAndCreatesNothing()
        {
            TestBuildingEntry entry = CreateEntry<TestBuilding>();
            entry.AllowConsumption = false;

            OperationResult result = BuildingAPI.TryBuild(
                entry, Vector3.zero, Quaternion.identity, out BuildingBase building);

            Assert.IsFalse(result);
            Assert.AreEqual(OperationResult.ERROR_DENIED, result.resultCode);
            Assert.IsNull(building);
            Assert.AreEqual(1, entry.ConsumeCallCount);
            Assert.AreEqual(1, entry.PlacementFailedCallCount);
        }

        [Test]
        public void TryBuildAndDemolish_WithSlotBuilding_ReservesThenReleasesSlot()
        {
            TestBuildingEntry entry = CreateEntry<TestSlotBuilding>();
            GameObject slotObject = Track(new GameObject("Building Slot"));
            BuildingSlot slot = slotObject.AddComponent<BuildingSlot>();
            List<BuildingSlot> slots = new List<BuildingSlot> { slot };

            OperationResult buildResult = BuildingAPI.TryBuild(
                entry, Vector3.zero, Quaternion.identity, out BuildingBase building, slots: slots);

            Assert.IsTrue(buildResult);
            Track(building.gameObject);
            Assert.IsTrue(slot.IsOccupied);
            Assert.AreEqual(building, slot.OccupyingBuilding);

            OperationResult demolishResult = BuildingAPI.TryDemolish(building);

            Assert.IsTrue(demolishResult);
            Assert.AreEqual(BuildingOperations.SUCCESS_DEMOLISHED, demolishResult.resultCode);
            Assert.IsFalse(slot.IsOccupied);
            Assert.AreEqual(1, entry.RefundCallCount);
            Assert.AreEqual(1, entry.DemolishedCallCount);
        }

        [Test]
        public void TryBuild_WithDefaultSlotSnapping_UsesTheSlotTransformPosition()
        {
            TestBuildingEntry entry = CreateEntry<TestSlotBuilding>();
            GameObject slotObject = Track(new GameObject("Building Slot"));
            slotObject.transform.position = new Vector3(4f, 0.25f, -3f);
            BoxCollider boxCollider = slotObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1f, 0.2f, 1f);
            BuildingSlot slot = slotObject.AddComponent<BuildingSlot>();
            GameObject raycasterObject = Track(new GameObject("Building Raycaster"));
            TestBuildingRaycaster raycaster = raycasterObject.AddComponent<TestBuildingRaycaster>();
            raycaster.RaycastRay = new Ray(slotObject.transform.position + Vector3.up * 5f, Vector3.down);
            Physics.SyncTransforms();

            OperationResult selectResult = raycaster.Select(entry);
            OperationResult buildResult = raycaster.TryBuild(out BuildingBase building);

            Assert.IsTrue(selectResult);
            Assert.IsTrue(buildResult);
            Track(building.gameObject);
            Assert.AreEqual(slot.SnapTransform.position, building.transform.position);
            ISlotBuilding slotBuilding = building as ISlotBuilding;
            Assert.IsNotNull(slotBuilding);
            Assert.IsTrue(slotBuilding.SnapToSlot);
        }

        private TestBuildingEntry CreateEntry<TBuildingType>() where TBuildingType : BuildingBase
        {
            GameObject prefabObject = Track(new GameObject("Building Prefab"));
            TBuildingType prefab = prefabObject.AddComponent<TBuildingType>();
            TestBuildingEntry entry = Track(ScriptableObject.CreateInstance<TestBuildingEntry>());
            entry.TestPrefab = prefab;
            return entry;
        }

        private TUnityObject Track<TUnityObject>(TUnityObject unityObject) where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }
    }

    public sealed class TestBuildingEntry : BuildingEntryBase
    {
        public BuildingBase TestPrefab;
        public bool AllowConsumption = true;
        public int ConsumeCallCount;
        public int RefundCallCount;
        public int PlacedCallCount;
        public int PlacementFailedCallCount;
        public int DemolishedCallCount;
        public Vector3 LastPlacementPosition;

        protected internal override BuildingBase GetPrefab() => TestPrefab;

        protected internal override OperationResult TryConsumeResources(in BuildingPlacementContext context)
        {
            ConsumeCallCount++;
            return AllowConsumption ? BuildingOperations.Permitted() : BuildingOperations.Denied();
        }

        protected internal override OperationResult TryRefundResources(in BuildingDemolitionContext context)
        {
            RefundCallCount++;
            return BuildingOperations.Permitted();
        }

        protected internal override void OnBuildingPlaced(
            in BuildingPlacementContext context,
            BuildingBase building,
            in OperationResult result)
        {
            PlacedCallCount++;
            LastPlacementPosition = context.position;
        }

        protected internal override void OnBuildingPlacementFailed(
            in BuildingPlacementContext context,
            in OperationResult result)
        {
            PlacementFailedCallCount++;
        }

        protected internal override void OnBuildingDemolished(
            in BuildingDemolitionContext context,
            in OperationResult result)
        {
            DemolishedCallCount++;
        }
    }

    public class TestBuilding : BuildingBase
    {
        public int PlacedCallCount;

        protected internal override void OnBuildingPlaced(
            in BuildingPlacementContext context,
            in OperationResult result)
        {
            PlacedCallCount++;
        }
    }

    public sealed class TestSlotBuilding : TestBuilding, ISlotBuilding
    {
        public int SlotCount => 1;
    }

    public sealed class TestBuildingRaycaster : BuildingRaycasterBase
    {
        [System.NonSerialized]
        public Ray RaycastRay;

        protected override bool TryGetRay(out Ray ray)
        {
            ray = RaycastRay;
            return true;
        }
    }
}
