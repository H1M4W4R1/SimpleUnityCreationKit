using NUnit.Framework;
using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleInteract.Operations;
using UnityEngine;

namespace Systems.SimpleInteract.Tests
{
    public sealed class InteractorDetectionWorkflowTests : SimpleInteractTestBase
    {
        [Test]
        public void Interact_ReturnsNoObjectsInRangeWhenNothingIsDetected()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);

            AssertSimilar(InteractOperations.NoObjectsInRange(), interactor.Interact());
        }

        [Test]
        public void DetectorWorkflow_AddsInteractorToCacheAndAllowsInteraction()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);

            detector.DetectNow();
            Assert.AreEqual(1, interactableObject.Interactors.Count);
            Assert.AreSame(interactor, interactableObject.Interactors[0]);
            Assert.AreEqual(1, interactableObject.InteractionZoneEnterCount);
            Assert.AreEqual(1, detector.DetectionStartCount);

            AssertSimilar(InteractOperations.Interacted(), interactor.Interact());
            Assert.AreEqual(1, interactableObject.InteractionCount);
        }

        [Test]
        public void Interact_UsesFirstDetectedInteractableDetector()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector firstDetector;
            TestInteractableObject firstInteractable =
                CreateInteractableObject("First", Vector3.zero, out firstDetector);
            TestInteractableDetector secondDetector;
            TestInteractableObject secondInteractable =
                CreateInteractableObject("Second", Vector3.zero, out secondDetector);

            firstDetector.DetectNow();
            secondDetector.DetectNow();

            AssertSimilar(InteractOperations.Interacted(), interactor.Interact());

            Assert.AreEqual(1, firstInteractable.InteractionCount);
            Assert.AreEqual(0, secondInteractable.InteractionCount);
        }

        [Test]
        public void InteractAll_InteractsWithEveryDetectedObjectAndCountsSuccessesOnly()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector firstDetector;
            TestInteractableObject firstInteractable =
                CreateInteractableObject("First", Vector3.zero, out firstDetector);
            TestInteractableDetector secondDetector;
            TestInteractableObject secondInteractable =
                CreateInteractableObject("Second", Vector3.zero, out secondDetector);
            secondInteractable.RejectInteraction = true;

            firstDetector.DetectNow();
            secondDetector.DetectNow();

            int interactionCount;
            AssertSimilar(InteractOperations.Interacted(), interactor.InteractAll(out interactionCount));

            Assert.AreEqual(1, interactionCount);
            Assert.AreEqual(1, firstInteractable.InteractionCount);
            Assert.AreEqual(0, secondInteractable.InteractionCount);
            Assert.AreEqual(1, secondInteractable.InteractionFailedCount);
        }

        [Test]
        public void InteractAll_ReturnsNoObjectsInRangeWhenOnlyPlainDetectorSeesInteractor()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            PlainObjectDetector detector = CreatePlainDetector("Plain Detector");

            detector.DetectNow();

            int interactionCount;
            AssertSimilar(InteractOperations.NoObjectsInRange(), interactor.InteractAll(out interactionCount));
            Assert.AreEqual(0, interactionCount);
        }

        [Test]
        public void DetectorWorkflow_RemovesInteractorFromCacheWhenItLeavesZone()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);
            detector.DetectNow();

            detector.TestZone.Result = SpotResult.Outside;
            detector.DetectNow();

            Assert.AreEqual(0, interactableObject.Interactors.Count);
            Assert.AreEqual(1, interactableObject.InteractionZoneExitCount);
            Assert.AreEqual(1, detector.DetectionEndCount);
            Assert.AreEqual(1, detector.DetectionFailedCount);
            Assert.AreSame(interactor, interactableObject.LastInteractor);
        }

        [Test]
        public void DetectorWorkflow_UsesInteractableDetectionFilter()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);
            interactableObject.RejectDetection = true;

            detector.DetectNow();

            Assert.AreEqual(0, detector.DetectedObjects.Count);
            Assert.AreEqual(0, interactor.DetectedBy.Count);
            Assert.AreEqual(0, interactableObject.Interactors.Count);
            Assert.AreEqual(1, interactableObject.CanBeDetectedCount);
            Assert.AreEqual(1, detector.DetectionFailedCount);
        }

        [Test]
        public void GhostSupportingDetector_CachesGhostInteractorThroughFallback()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            interactor.SetGhost(true);
            TestGhostInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateGhostInteractableObject("Interactable", Vector3.zero, out detector);

            detector.DetectNow();

            Assert.AreEqual(1, detector.GhostDetectedCount);
            Assert.AreEqual(1, interactableObject.Interactors.Count);
            Assert.AreSame(interactor, interactableObject.Interactors[0]);
        }
    }
}
