using NUnit.Framework;
using Systems.SimpleInteract.Components.Detectors;
using Systems.SimpleInteract.Components.Detectors.Abstract;
using Systems.SimpleInteract.Operations;
using UnityEngine;

namespace Systems.SimpleInteract.Tests
{
    public sealed class InteractableObjectInteractionTests : SimpleInteractTestBase
    {
        [Test]
        public void InteractWith_WhenPermitted_CallsSuccessCallbackAndReturnsInteracted()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);

            AssertSimilar(InteractOperations.Interacted(), interactableObject.InteractWith(interactor));

            Assert.AreEqual(1, interactableObject.CanBeInteractedWithCount);
            Assert.AreEqual(1, interactor.CanInteractCount);
            Assert.AreEqual(1, interactableObject.InteractionCount);
            Assert.AreEqual(0, interactableObject.InteractionFailedCount);
            Assert.AreSame(interactor, interactableObject.LastInteractor);
            Assert.AreSame(interactableObject, interactor.LastInteractable);
        }

        [Test]
        public void InteractWith_WhenInteractableRejects_CallsFailedCallbackAndSkipsInteractorCheck()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);
            interactableObject.RejectInteraction = true;

            AssertSimilar(InteractOperations.Denied(), interactableObject.InteractWith(interactor));

            Assert.AreEqual(1, interactableObject.CanBeInteractedWithCount);
            Assert.AreEqual(0, interactor.CanInteractCount);
            Assert.AreEqual(0, interactableObject.InteractionCount);
            Assert.AreEqual(1, interactableObject.InteractionFailedCount);
        }

        [Test]
        public void InteractWith_WhenInteractorRejects_CallsFailedCallbackWithInteractorResult()
        {
            TestInteractor interactor = CreateInteractor("Interactor", Vector3.zero);
            interactor.RejectInteraction = true;
            TestInteractableDetector detector;
            TestInteractableObject interactableObject =
                CreateInteractableObject("Interactable", Vector3.zero, out detector);

            AssertSimilar(InteractOperations.Denied(), interactableObject.InteractWith(interactor));

            Assert.AreEqual(1, interactableObject.CanBeInteractedWithCount);
            Assert.AreEqual(1, interactor.CanInteractCount);
            Assert.AreEqual(0, interactableObject.InteractionCount);
            Assert.AreEqual(1, interactableObject.InteractionFailedCount);
            Assert.AreEqual(InteractOperations.Denied().systemCode, interactableObject.LastSystemCode);
            Assert.AreEqual(InteractOperations.Denied().resultCode, interactableObject.LastResultCode);
        }

        [Test]
        public void BuiltInDetectors_AreResolvableThroughInteractableDetectorInterface()
        {
            GameObject twoDimensionalObject = Track(new GameObject("2D Interactable"));
            InteractableDetector2D twoDimensionalDetector = twoDimensionalObject.AddComponent<InteractableDetector2D>();

            GameObject threeDimensionalObject = Track(new GameObject("3D Interactable"));
            InteractableDetector3D threeDimensionalDetector =
                threeDimensionalObject.AddComponent<InteractableDetector3D>();

            Assert.AreSame(twoDimensionalDetector, twoDimensionalObject.GetComponent<IInteractableDetector>());
            Assert.AreSame(threeDimensionalDetector, threeDimensionalObject.GetComponent<IInteractableDetector>());
        }
    }
}
