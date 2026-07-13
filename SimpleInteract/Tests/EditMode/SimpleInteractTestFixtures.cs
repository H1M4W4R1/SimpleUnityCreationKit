using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Markers;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Systems.SimpleDetection.Components.Objects.Abstract;
using Systems.SimpleDetection.Data;
using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleDetection.Operations;
using Systems.SimpleInteract.Components;
using Systems.SimpleInteract.Components.Detectors;
using Systems.SimpleInteract.Components.Detectors.Abstract;
using Systems.SimpleInteract.Data;
using Systems.SimpleInteract.Operations;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleInteract.Tests
{
    public abstract class SimpleInteractTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _createdObjects.Count - 1; index >= 0; index--)
            {
                Object createdObject = _createdObjects[index];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestInteractor CreateInteractor(string objectName, Vector3 position)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            gameObject.transform.position = position;
            TestInteractor interactor = gameObject.AddComponent<TestInteractor>();
            return interactor;
        }

        protected TestInteractableObject CreateInteractableObject(
            string objectName,
            Vector3 position,
            out TestInteractableDetector detector)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            gameObject.transform.position = position;
            detector = gameObject.AddComponent<TestInteractableDetector>();
            TestInteractableObject interactableObject = gameObject.AddComponent<TestInteractableObject>();
            interactableObject.InitializeForTests();
            return interactableObject;
        }

        protected TestInteractableObject CreateGhostInteractableObject(
            string objectName,
            Vector3 position,
            out TestGhostInteractableDetector detector)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            gameObject.transform.position = position;
            detector = gameObject.AddComponent<TestGhostInteractableDetector>();
            TestInteractableObject interactableObject = gameObject.AddComponent<TestInteractableObject>();
            interactableObject.InitializeForTests();
            return interactableObject;
        }

        protected PlainObjectDetector CreatePlainDetector(string objectName)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            PlainObjectDetector detector = gameObject.AddComponent<PlainObjectDetector>();
            return detector;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected.resultCode + " but received " + actual.resultCode);
        }
    }

    public sealed class TestInteractDetectionZone : IDetectionZone
    {
        public SpotResult Result { get; set; } = SpotResult.InsideSeen;

        public bool IsPointInZone(in float3 detectionPosition)
        {
            return Result != SpotResult.Outside;
        }

        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask)
        {
            return Result;
        }

        public void DrawGizmos(LayerMask raycastLayerMask)
        {
        }
    }

    public class TestInteractableDetector : ObjectDetectorBase, IInteractableDetector
    {
        private event Delegates.ObjectDetectedHandle ObjectDetected;
        private event Delegates.ObjectDetectionFailedHandle ObjectDetectionFailed;
        private event Delegates.ObjectDetectionEndHandle ObjectDetectionEnd;
        private event Delegates.ObjectDetectionStartHandle ObjectDetectionStart;
        private event Delegates.ObjectGhostDetectedHandle ObjectGhostDetected;
        private event Delegates.CanBeDetectedHandle ObjectCanBeDetected;

        public readonly TestInteractDetectionZone TestZone = new TestInteractDetectionZone();

        public int DetectedCount { get; private set; }
        public int DetectionFailedCount { get; private set; }
        public int DetectionEndCount { get; private set; }
        public int DetectionStartCount { get; private set; }
        public int GhostDetectedCount { get; private set; }

        event Delegates.ObjectDetectedHandle IInteractableDetector.ObjectDetected
        {
            add => ObjectDetected += value;
            remove => ObjectDetected -= value;
        }

        event Delegates.ObjectDetectionFailedHandle IInteractableDetector.ObjectDetectionFailed
        {
            add => ObjectDetectionFailed += value;
            remove => ObjectDetectionFailed -= value;
        }

        event Delegates.ObjectDetectionEndHandle IInteractableDetector.ObjectDetectionEnd
        {
            add => ObjectDetectionEnd += value;
            remove => ObjectDetectionEnd -= value;
        }

        event Delegates.ObjectDetectionStartHandle IInteractableDetector.ObjectDetectionStart
        {
            add => ObjectDetectionStart += value;
            remove => ObjectDetectionStart -= value;
        }

        event Delegates.ObjectGhostDetectedHandle IInteractableDetector.ObjectGhostDetected
        {
            add => ObjectGhostDetected += value;
            remove => ObjectGhostDetected -= value;
        }

        event Delegates.CanBeDetectedHandle IInteractableDetector.ObjectCanBeDetected
        {
            add => ObjectCanBeDetected += value;
            remove => ObjectCanBeDetected -= value;
        }

        public void DetectNow()
        {
            FixedUpdate();
        }

        protected override IDetectionZone GetDetectionZone()
        {
            return TestZone;
        }

        protected override OperationResult CanDetect(in ObjectDetectionContext context)
        {
            OperationResult baseDetection = base.CanDetect(context);
            if (!baseDetection) return baseDetection;
            if (ReferenceEquals(ObjectCanBeDetected, null)) return DetectionOperations.Permitted();
            return ObjectCanBeDetected.Invoke(context);
        }

        protected override void OnObjectDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectedCount++;
            base.OnObjectDetected(context, detectionResult);
            ObjectDetected?.Invoke(context, detectionResult);
        }

        protected override void OnObjectDetectionFailed(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectionFailedCount++;
            base.OnObjectDetectionFailed(context, detectionResult);
            ObjectDetectionFailed?.Invoke(context, detectionResult);
        }

        protected override void OnObjectDetectionEnd(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectionEndCount++;
            base.OnObjectDetectionEnd(context, detectionResult);
            ObjectDetectionEnd?.Invoke(context, detectionResult);
        }

        protected override void OnObjectDetectionStart(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectionStartCount++;
            base.OnObjectDetectionStart(context, detectionResult);
            ObjectDetectionStart?.Invoke(context, detectionResult);
        }

        protected override void OnObjectGhostDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            GhostDetectedCount++;
            base.OnObjectGhostDetected(context, detectionResult);
            ObjectGhostDetected?.Invoke(context, detectionResult);
        }
    }

    public sealed class TestGhostInteractableDetector : TestInteractableDetector, ISupportGhostDetection
    {
    }

    public sealed class PlainObjectDetector : ObjectDetectorBase
    {
        public readonly TestInteractDetectionZone TestZone = new TestInteractDetectionZone();

        public void DetectNow()
        {
            FixedUpdate();
        }

        protected override IDetectionZone GetDetectionZone()
        {
            return TestZone;
        }
    }

    public sealed class TestInteractor : InteractorBase
    {
        public bool RejectInteraction { get; set; }
        public int CanInteractCount { get; private set; }
        public InteractableObjectBase LastInteractable { get; private set; }

        public void SetGhost(bool isGhost)
        {
            IsGhost = isGhost;
        }

        protected internal override OperationResult CanInteract(InteractionContext context)
        {
            CanInteractCount++;
            LastInteractable = context.interactable;
            if (RejectInteraction) return InteractOperations.Denied();
            return base.CanInteract(context);
        }
    }

    public sealed class TestInteractableObject : InteractableObjectBase
    {
        private bool _initializedForTests;

        public bool RejectInteraction { get; set; }
        public bool RejectDetection { get; set; }
        public int CanBeInteractedWithCount { get; private set; }
        public int InteractionCount { get; private set; }
        public int InteractionFailedCount { get; private set; }
        public int InteractionZoneEnterCount { get; private set; }
        public int InteractionZoneExitCount { get; private set; }
        public int CanBeDetectedCount { get; private set; }
        public InteractorBase LastInteractor { get; private set; }
        public ushort LastSystemCode { get; private set; }
        public ushort LastResultCode { get; private set; }

        public OperationResult InteractWith(InteractorBase interactor)
        {
            return Interact(interactor);
        }

        public void InitializeForTests()
        {
            if (_initializedForTests) return;
            Awake();
            _initializedForTests = true;
        }

        protected internal override OperationResult CanBeInteractedWith(InteractionContext context)
        {
            CanBeInteractedWithCount++;
            LastInteractor = context.interactor;
            if (RejectInteraction) return InteractOperations.Denied();
            return base.CanBeInteractedWith(context);
        }

        protected internal override OperationResult CanBeDetected(ObjectDetectionContext context)
        {
            CanBeDetectedCount++;
            if (RejectDetection) return DetectionOperations.InvalidDetectableObject();
            return base.CanBeDetected(context);
        }

        protected override void OnInteract(
            in InteractionContext context,
            in OperationResult interactCapabilityResult)
        {
            InteractionCount++;
            Capture(context.interactor, interactCapabilityResult);
        }

        protected override void OnInteractFailed(
            in InteractionContext context,
            in OperationResult interactCapabilityResult)
        {
            InteractionFailedCount++;
            Capture(context.interactor, interactCapabilityResult);
        }

        protected override void OnInteractionZoneEnter(InteractorBase obj)
        {
            InteractionZoneEnterCount++;
            LastInteractor = obj;
        }

        protected override void OnInteractionZoneExit(InteractorBase obj)
        {
            InteractionZoneExitCount++;
            LastInteractor = obj;
        }

        private void Capture(InteractorBase interactor, OperationResult result)
        {
            LastInteractor = interactor;
            LastSystemCode = result.systemCode;
            LastResultCode = result.resultCode;
        }
    }
}
