using NUnit.Framework;
using Systems.SimpleUI.Components.Features.Drag;
using Systems.SimpleUI.Components.Features.Positioning;
using UnityEngine;

namespace Systems.SimpleUI.Tests
{
    public sealed class UIDragAndPositioningTests
    {
        [SetUp]
        public void SetUp()
        {
            SimpleUITestFixtures.ResetScene();
        }

        [TearDown]
        public void TearDown()
        {
            SimpleUITestFixtures.ResetScene();
        }

        [Test]
        public void SlotAcceptsDraggableAndUpdatesOccupantAndParent()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSlotFeature slot = CreateSlot("Slot", root.transform, new Vector2(0f, 0f));
            TestDragFeature drag = CreateDrag("Drag", root.transform);

            slot.OnDrop(drag);

            Assert.AreSame(drag, slot.Occupant);
            Assert.AreSame(slot, drag.CurrentDropZone);
            Assert.AreSame(slot.transform, drag.transform.parent);
            Assert.AreEqual(Vector3.zero, drag.transform.localPosition);
        }

        [Test]
        public void SlotCanRejectSecondOccupantWhenSwapIsDisabled()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSlotFeature slot = CreateSlot("Slot", root.transform, new Vector2(0f, 0f));
            slot.SwapAllowed = false;
            TestDragFeature firstDrag = CreateDrag("First Drag", root.transform);
            TestDragFeature secondDrag = CreateDrag("Second Drag", root.transform);

            slot.OnDrop(firstDrag);

            Assert.IsFalse(slot.CanDrop(secondDrag));
        }

        [Test]
        public void SlotSwapsOccupantsBetweenSourceAndTargetSlots()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSlotFeature firstSlot = CreateSlot("First Slot", root.transform, new Vector2(-100f, 0f));
            TestSlotFeature secondSlot = CreateSlot("Second Slot", root.transform, new Vector2(100f, 0f));
            TestDragFeature firstDrag = CreateDrag("First Drag", root.transform);
            TestDragFeature secondDrag = CreateDrag("Second Drag", root.transform);

            firstSlot.OnDrop(firstDrag);
            secondSlot.OnDrop(secondDrag);

            Assert.IsTrue(firstSlot.CanDrop(secondDrag));
            firstSlot.OnDrop(secondDrag);

            Assert.AreSame(secondDrag, firstSlot.Occupant);
            Assert.AreSame(firstDrag, secondSlot.Occupant);
            Assert.AreSame(firstSlot, secondDrag.CurrentDropZone);
            Assert.AreSame(secondSlot, firstDrag.CurrentDropZone);
        }

        [Test]
        public void SlotFailedDropRestoresOccupant()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestSlotFeature slot = CreateSlot("Slot", root.transform, new Vector2(0f, 0f));
            TestDragFeature drag = CreateDrag("Drag", root.transform);

            slot.OnDrop(drag);
            slot.OnPick(drag);
            slot.OnFailedDrop(drag);

            Assert.AreSame(drag, slot.Occupant);
            Assert.AreEqual(Vector3.zero, drag.transform.localPosition);
        }

        [Test]
        public void RejectedBeginDragDoesNotMoveOnDragOrProcessEnd()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            TestDragFeature drag = CreateDrag("Rejected Drag", root.transform);
            drag.AllowPick = false;
            Vector3 originalPosition = drag.transform.position;
            UnityEngine.EventSystems.PointerEventData eventData =
                SimpleUITestFixtures.CreatePointerEvent(new Vector2(100f, 100f));

            drag.OnBeginDrag(eventData);
            drag.OnDrag(eventData);
            drag.OnEndDrag(eventData);

            Assert.AreEqual(originalPosition, drag.transform.position);
        }

        [Test]
        public void LimitObjectToParentMovesChildBackInsideParentBounds()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            RectTransform parent = TestRectUtility.CreateRect("Parent", root.transform, new Vector2(100f, 100f),
                Vector2.zero);
            RectTransform child = TestRectUtility.CreateRect("Child", parent, new Vector2(40f, 40f),
                new Vector2(80f, 0f));
            LimitObjectToParent limiter = TestRectUtility.AddParentLimiter(child);

            limiter.ApplyLimitForTests();

            Assert.LessOrEqual(child.anchoredPosition.x, 30.1f);
        }

        [Test]
        public void LimitObjectToViewportMovesChildBackInsideRootCanvasBounds()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(100f, 100f);

            RectTransform child = TestRectUtility.CreateRect("Child", root.transform, new Vector2(40f, 40f),
                new Vector2(80f, 0f));
            LimitObjectToViewport limiter = TestRectUtility.AddViewportLimiter(child);

            limiter.ApplyLimitForTests();

            Assert.LessOrEqual(child.anchoredPosition.x, 30.1f);
        }

        [Test]
        public void DraggableWindowNullDropKeepsWindowUnderCanvas()
        {
            GameObject root = SimpleUITestFixtures.CreateWindowCanvas();
            RectTransform window = TestRectUtility.CreateRect("Window", root.transform, new Vector2(100f, 100f),
                Vector2.zero);
            DraggableWindowFeature draggableWindow = window.gameObject.AddComponent<DraggableWindowFeature>();
            draggableWindow.InitializeForTests();

            draggableWindow.OnEndDrag(SimpleUITestFixtures.CreatePointerEvent(Vector2.zero));

            Assert.AreSame(root.transform, window.parent);
        }

        private static TestSlotFeature CreateSlot(string name, Transform parent, Vector2 anchoredPosition)
        {
            GameObject slotObject = new GameObject(name, typeof(RectTransform));
            slotObject.transform.SetParent(parent, false);
            RectTransform rectTransform = slotObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100f, 100f);
            rectTransform.anchoredPosition = anchoredPosition;
            TestSlotFeature slot = slotObject.AddComponent<TestSlotFeature>();
            SimpleUITestFixtures.InitializeRecursively(slotObject);
            return slot;
        }

        private static TestDragFeature CreateDrag(string name, Transform parent)
        {
            GameObject dragObject = new GameObject(name, typeof(RectTransform));
            dragObject.transform.SetParent(parent, false);
            RectTransform rectTransform = dragObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(30f, 30f);
            TestDragFeature drag = dragObject.AddComponent<TestDragFeature>();
            SimpleUITestFixtures.InitializeRecursively(dragObject);
            return drag;
        }
    }
}
