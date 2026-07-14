using System;
using NUnit.Framework;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Components;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleBrain.Tests
{
    public sealed class BrainBaseTests
    {
        private GameObject _brainObject;
        private TestBrain _brain;

        [SetUp]
        public void SetUp()
        {
            TestKnowledge.Reset();
            RestrictedKnowledge.Reset();
            AvailabilityKnowledge.Reset();
            TrackingDecision.Reset();
            DeniedDecision.Reset();
            TrackingSubprocess.Reset();
            SelfFinishingSubprocess.Reset();
            ComaSubprocess.Reset();

            _brainObject = new GameObject("Brain Test");
            _brainObject.SetActive(false);
            _brain = _brainObject.AddComponent<TestBrain>();
            _brainObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_brainObject) Object.DestroyImmediate(_brainObject);
        }

        [Test]
        public void Knowledge_WhenNotLearned_IsUnavailable()
        {
            bool hasLearned = _brain.HasLearned<TestKnowledge>();
            bool knows = _brain.Knows<TestKnowledge>();
            bool wasRetrieved = _brain.TryGetKnowledge(out TestKnowledge knowledge);

            Assert.IsFalse(hasLearned);
            Assert.IsFalse(knows);
            Assert.IsFalse(wasRetrieved);
            Assert.IsNull(knowledge);
        }

        [Test]
        public void LearnedKnowledge_IsRetainedAndAvailableToDecisions()
        {
            OperationResult learnResult = _brain.TryLearn<TestKnowledge>();

            AssertResult(learnResult, BrainOperations.KnowledgeLearned());
            Assert.IsTrue(_brain.HasLearned<TestKnowledge>());
            Assert.IsTrue(_brain.Knows<TestKnowledge>());
            Assert.IsTrue(_brain.TryGetKnowledge(out TestKnowledge knowledge));
            Assert.AreEqual(1, TestKnowledge.learnedCount);
            Assert.AreSame(_brain, TestKnowledge.learnedBrain);

            knowledge.alertness = 4;
            OperationResult decideResult = _brain.TryDecide<TestDecision, int>(out int alertness);

            AssertResult(decideResult, BrainOperations.DecisionMade());
            Assert.AreEqual(4, alertness);
        }

        [Test]
        public void LearningExistingKnowledge_ReturnsAlreadyLearnedAndPreservesTheInstance()
        {
            _brain.TryLearn<TestKnowledge>();
            Assert.IsTrue(_brain.TryGetKnowledge(out TestKnowledge firstKnowledge));
            firstKnowledge.alertness = 7;

            OperationResult secondLearnResult = _brain.TryLearn<TestKnowledge>();

            AssertResult(secondLearnResult, BrainOperations.KnowledgeAlreadyLearned());
            Assert.IsTrue(_brain.TryGetKnowledge(out TestKnowledge secondKnowledge));
            Assert.AreSame(firstKnowledge, secondKnowledge);
            Assert.AreEqual(7, secondKnowledge.alertness);
            Assert.AreEqual(1, TestKnowledge.learnedCount);
        }

        [Test]
        public void LearningRejectedKnowledge_NotifiesFailureAndDoesNotStoreIt()
        {
            OperationResult learnResult = _brain.TryLearn<RestrictedKnowledge>();

            Assert.IsTrue(OperationResult.IsError(learnResult));
            Assert.IsFalse(_brain.HasLearned<RestrictedKnowledge>());
            Assert.IsFalse(_brain.Knows<RestrictedKnowledge>());
            Assert.IsFalse(_brain.TryGetKnowledge(out RestrictedKnowledge knowledge));
            Assert.IsNull(knowledge);
            Assert.AreEqual(1, RestrictedKnowledge.failedCount);
            Assert.AreSame(_brain, RestrictedKnowledge.failedBrain);
        }

        [Test]
        public void LearnedKnowledge_CanTemporarilyBeUnavailable()
        {
            OperationResult learnResult = _brain.TryLearn<AvailabilityKnowledge>();
            AvailabilityKnowledge.isAvailable = false;

            AssertResult(learnResult, BrainOperations.KnowledgeLearned());
            Assert.IsTrue(_brain.HasLearned<AvailabilityKnowledge>());
            Assert.IsFalse(_brain.Knows<AvailabilityKnowledge>());
            Assert.IsTrue(_brain.TryGetKnowledge(out AvailabilityKnowledge knowledge));
            Assert.IsNotNull(knowledge);
        }

        [Test]
        public void Decisions_ReturnTheirValueAndNotifySuccess()
        {
            OperationResult decideResult = _brain.TryDecide<TrackingDecision, int>(out int decisionValue);
            OperationResult assessResult = _brain.TryAssess<TrackingDecision, int>(out int assessmentValue);

            AssertResult(decideResult, BrainOperations.DecisionMade());
            AssertResult(assessResult, BrainOperations.DecisionMade());
            Assert.AreEqual(11, decisionValue);
            Assert.AreEqual(11, assessmentValue);
            Assert.AreEqual(2, TrackingDecision.decidedCount);
            Assert.AreSame(_brain, TrackingDecision.lastBrain);
        }

        [Test]
        public void Decisions_ReuseOneInstanceForEachConcreteType()
        {
            OperationResult firstResult = _brain.TryDecide<InstanceCountingDecision, int>(out int firstValue);
            OperationResult secondResult = _brain.TryDecide<InstanceCountingDecision, int>(out int secondValue);

            AssertResult(firstResult, BrainOperations.DecisionMade());
            AssertResult(secondResult, BrainOperations.DecisionMade());
            Assert.AreEqual(1, firstValue);
            Assert.AreEqual(1, secondValue);
            Assert.AreEqual(1, InstanceCountingDecision.instanceCount);
        }

        [Test]
        public void UntypedAssessments_ReturnTheirObjectValue()
        {
            OperationResult assessResult = _brain.TryAssess<UntypedDecision>(out object assessmentValue);

            AssertResult(assessResult, BrainOperations.DecisionMade());
            Assert.AreEqual("observed", assessmentValue);
        }

        [Test]
        public void RejectedDecisions_ReturnDefaultValueAndNotifyFailure()
        {
            OperationResult decideResult = _brain.TryDecide<DeniedDecision, int>(out int decisionValue);

            Assert.IsTrue(OperationResult.IsError(decideResult));
            Assert.AreEqual(default(int), decisionValue);
            Assert.AreEqual(1, DeniedDecision.failedCount);
            Assert.AreSame(_brain, DeniedDecision.failedBrain);
        }

        [Test]
        public void Subprocesses_ExposeCreationAndMissingState()
        {
            Assert.IsFalse(_brain.IsSubprocessCreated<TrackingSubprocess>());
            Assert.IsFalse(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.IsFalse(_brain.IsSubprocessPaused<TrackingSubprocess>());

            OperationResult stopResult = _brain.TryStopSubprocess<TrackingSubprocess>();
            OperationResult pauseResult = _brain.TryPauseSubprocess<TrackingSubprocess>();
            OperationResult resumeResult = _brain.TryResumeSubprocess<TrackingSubprocess>();

            AssertResult(stopResult, BrainOperations.SubprocessNotCreated());
            AssertResult(pauseResult, BrainOperations.SubprocessNotCreated());
            AssertResult(resumeResult, BrainOperations.SubprocessNotCreated());
        }

        [Test]
        public void Subprocesses_StartPauseResumeStopAndReuseTheirInstance()
        {
            OperationResult startResult = _brain.TryStartSubprocess<TrackingSubprocess>();
            OperationResult pauseResult = _brain.TryPauseSubprocess<TrackingSubprocess>();
            OperationResult resumeResult = _brain.TryResumeSubprocess<TrackingSubprocess>();
            OperationResult stopResult = _brain.TryStopSubprocess<TrackingSubprocess>();
            OperationResult restartResult = _brain.TryStartSubprocess<TrackingSubprocess>();

            AssertResult(startResult, BrainOperations.SubprocessStarted());
            AssertResult(pauseResult, BrainOperations.SubprocessPaused());
            AssertResult(resumeResult, BrainOperations.SubprocessResumed());
            AssertResult(stopResult, BrainOperations.SubprocessStopped());
            AssertResult(restartResult, BrainOperations.SubprocessStarted());
            Assert.IsTrue(_brain.IsSubprocessCreated<TrackingSubprocess>());
            Assert.IsTrue(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.instanceCount);
            Assert.AreEqual(2, TrackingSubprocess.startedCount);
            Assert.AreEqual(1, TrackingSubprocess.pausedCount);
            Assert.AreEqual(1, TrackingSubprocess.resumedCount);
            Assert.AreEqual(1, TrackingSubprocess.stoppedCount);
        }

        [Test]
        public void Subprocesses_RejectInvalidStateTransitions()
        {
            _brain.TryStartSubprocess<TrackingSubprocess>();

            OperationResult repeatedStartResult = _brain.TryStartSubprocess<TrackingSubprocess>();
            OperationResult resumeRunningResult = _brain.TryResumeSubprocess<TrackingSubprocess>();
            _brain.TryPauseSubprocess<TrackingSubprocess>();
            OperationResult repeatedPauseResult = _brain.TryPauseSubprocess<TrackingSubprocess>();
            OperationResult startPausedResult = _brain.TryStartSubprocess<TrackingSubprocess>();
            _brain.TryStopSubprocess<TrackingSubprocess>();
            OperationResult repeatedStopResult = _brain.TryStopSubprocess<TrackingSubprocess>();

            AssertResult(repeatedStartResult, BrainOperations.SubprocessAlreadyRunning());
            AssertResult(resumeRunningResult, BrainOperations.SubprocessNotPaused());
            AssertResult(repeatedPauseResult, BrainOperations.SubprocessNotRunning());
            AssertResult(startPausedResult, BrainOperations.SubprocessIsPaused());
            AssertResult(repeatedStopResult, BrainOperations.SubprocessAlreadyStopped());
        }

        [Test]
        public void Subprocesses_KeepTheirStateWhenConditionsRejectTransitions()
        {
            TrackingSubprocess.allowStart = false;
            OperationResult startResult = _brain.TryStartSubprocess<TrackingSubprocess>();

            Assert.IsTrue(OperationResult.IsError(startResult));
            Assert.IsTrue(_brain.IsSubprocessCreated<TrackingSubprocess>());
            Assert.IsFalse(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.startFailedCount);

            TrackingSubprocess.allowStart = true;
            _brain.TryStartSubprocess<TrackingSubprocess>();
            TrackingSubprocess.allowPause = false;
            OperationResult pauseResult = _brain.TryPauseSubprocess<TrackingSubprocess>();

            Assert.IsTrue(OperationResult.IsError(pauseResult));
            Assert.IsTrue(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.pauseFailedCount);

            TrackingSubprocess.allowPause = true;
            _brain.TryPauseSubprocess<TrackingSubprocess>();
            TrackingSubprocess.allowResume = false;
            OperationResult resumeResult = _brain.TryResumeSubprocess<TrackingSubprocess>();

            Assert.IsTrue(OperationResult.IsError(resumeResult));
            Assert.IsTrue(_brain.IsSubprocessPaused<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.resumeFailedCount);

            TrackingSubprocess.allowResume = true;
            _brain.TryResumeSubprocess<TrackingSubprocess>();
            TrackingSubprocess.allowStop = false;
            OperationResult stopResult = _brain.TryStopSubprocess<TrackingSubprocess>();

            Assert.IsTrue(OperationResult.IsError(stopResult));
            Assert.IsTrue(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.stopFailedCount);
        }

        [Test]
        public void Subprocesses_CanFinishThemselvesFromLifecycleCallbacks()
        {
            OperationResult startResult = _brain.TryStartSubprocess<SelfFinishingSubprocess>();

            AssertResult(startResult, BrainOperations.SubprocessStarted());
            Assert.IsTrue(SelfFinishingSubprocess.finishWasSuccessful);
            Assert.AreEqual(BrainOperations.SUCCESS_SUBPROCESS_FINISHED, SelfFinishingSubprocess.finishResultCode);
            Assert.IsFalse(_brain.IsSubprocessRunning<SelfFinishingSubprocess>());
            Assert.AreEqual(1, SelfFinishingSubprocess.finishedCount);
            Assert.AreSame(_brain, SelfFinishingSubprocess.finishedBrain);
        }

        [Test]
        public void Coma_PausesNormalSubprocessesAndKeepsAllowedOnesRunning()
        {
            _brain.TryStartSubprocess<TrackingSubprocess>();
            _brain.TryStartSubprocess<ComaSubprocess>();

            OperationResult enterComaResult = _brain.TryEnterComa();

            AssertResult(enterComaResult, BrainOperations.ComaEntered());
            Assert.IsTrue(_brain.IsInComa);
            Assert.IsTrue(_brain.IsSubprocessPaused<TrackingSubprocess>());
            Assert.IsTrue(_brain.IsSubprocessRunning<ComaSubprocess>());
            Assert.IsTrue(TrackingSubprocess.lastPauseWasComaInduced);

            OperationResult exitComaResult = _brain.TryExitComa();

            AssertResult(exitComaResult, BrainOperations.ComaExited());
            Assert.IsFalse(_brain.IsInComa);
            Assert.IsTrue(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.IsTrue(_brain.IsSubprocessRunning<ComaSubprocess>());
            Assert.IsTrue(TrackingSubprocess.lastResumeWasComaInduced);
        }

        [Test]
        public void Coma_LeavesManuallyPausedSubprocessesPaused()
        {
            _brain.TryStartSubprocess<TrackingSubprocess>();
            _brain.TryPauseSubprocess<TrackingSubprocess>();

            _brain.TryEnterComa();
            _brain.TryExitComa();

            Assert.IsTrue(_brain.IsSubprocessPaused<TrackingSubprocess>());
            Assert.IsFalse(TrackingSubprocess.lastPauseWasComaInduced);
        }

        [Test]
        public void Coma_StartingANormalSubprocessPausesItImmediately()
        {
            _brain.TryEnterComa();

            OperationResult startResult = _brain.TryStartSubprocess<TrackingSubprocess>();

            AssertResult(startResult, BrainOperations.SubprocessStarted());
            Assert.IsTrue(_brain.IsSubprocessPaused<TrackingSubprocess>());
            Assert.IsTrue(TrackingSubprocess.lastPauseWasComaInduced);
        }

        [Test]
        public void Coma_RespectsSubprocessPauseAndResumeConditions()
        {
            _brain.TryStartSubprocess<TrackingSubprocess>();
            TrackingSubprocess.allowPause = false;

            _brain.TryEnterComa();

            Assert.IsTrue(_brain.IsSubprocessRunning<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.pauseFailedCount);
            Assert.IsTrue(TrackingSubprocess.lastPauseFailureWasComaInduced);

            TrackingSubprocess.allowPause = true;
            _brain.TryExitComa();
            _brain.TryEnterComa();
            TrackingSubprocess.allowResume = false;

            _brain.TryExitComa();

            Assert.IsTrue(_brain.IsSubprocessPaused<TrackingSubprocess>());
            Assert.AreEqual(1, TrackingSubprocess.resumeFailedCount);
            Assert.IsTrue(TrackingSubprocess.lastResumeFailureWasComaInduced);
        }

        [Test]
        public void Coma_UsesConditionsAndIsIdempotent()
        {
            _brain.allowComaEntry = false;
            OperationResult deniedEntryResult = _brain.TryEnterComa();

            Assert.IsTrue(OperationResult.IsError(deniedEntryResult));
            Assert.IsFalse(_brain.IsInComa);
            Assert.AreEqual(1, _brain.comaEntryFailedCount);

            _brain.allowComaEntry = true;
            _brain.TryEnterComa();
            OperationResult repeatedEntryResult = _brain.TryEnterComa();

            AssertResult(repeatedEntryResult, BrainOperations.AlreadyInComa());
            Assert.AreEqual(1, _brain.comaEnteredCount);

            _brain.allowComaExit = false;
            OperationResult deniedExitResult = _brain.TryExitComa();

            Assert.IsTrue(OperationResult.IsError(deniedExitResult));
            Assert.IsTrue(_brain.IsInComa);
            Assert.AreEqual(1, _brain.comaExitFailedCount);

            _brain.allowComaExit = true;
            _brain.TryExitComa();
            OperationResult repeatedExitResult = _brain.TryExitComa();

            AssertResult(repeatedExitResult, BrainOperations.ComaExited());
            Assert.AreEqual(1, _brain.comaExitedCount);
            Assert.IsFalse(_brain.IsInComa);
        }

        private static void AssertResult(in OperationResult actual, in OperationResult expected)
        {
            Assert.IsTrue(OperationResult.AreSimilar(actual, expected));
        }

        private sealed class TestBrain : BrainBase
        {
            public bool allowComaEntry = true;
            public bool allowComaExit = true;
            public int comaEnteredCount;
            public int comaEntryFailedCount;
            public int comaExitedCount;
            public int comaExitFailedCount;

            protected override OperationResult CanEnterComa()
            {
                return allowComaEntry ? BrainOperations.Permitted() : Denied();
            }

            protected override OperationResult CanExitComa()
            {
                return allowComaExit ? BrainOperations.Permitted() : Denied();
            }

            protected override void OnComaEntered(in OperationResult result)
            {
                comaEnteredCount++;
                base.OnComaEntered(result);
            }

            protected override void OnComaEntryFailed(in OperationResult result)
            {
                comaEntryFailedCount++;
            }

            protected override void OnComaExited(in OperationResult result)
            {
                comaExitedCount++;
                base.OnComaExited(result);
            }

            protected override void OnComaExitFailed(in OperationResult result)
            {
                comaExitFailedCount++;
            }
        }

        [Serializable]
        private sealed class TestKnowledge : KnowledgeBase
        {
            public static int learnedCount;
            public static BrainBase learnedBrain;

            public int alertness;

            public static void Reset()
            {
                learnedCount = 0;
                learnedBrain = null;
            }

            protected override void OnLearned(in BrainContext context, in OperationResult result)
            {
                learnedCount++;
                learnedBrain = context.brain;
            }
        }

        [Serializable]
        private sealed class RestrictedKnowledge : KnowledgeBase
        {
            public static int failedCount;
            public static BrainBase failedBrain;

            public static void Reset()
            {
                failedCount = 0;
                failedBrain = null;
            }

            protected override OperationResult CanLearn(in BrainContext context) => Denied();

            protected override void OnLearningFailed(in BrainContext context, in OperationResult result)
            {
                failedCount++;
                failedBrain = context.brain;
            }
        }

        [Serializable]
        private sealed class AvailabilityKnowledge : KnowledgeBase
        {
            public static bool isAvailable;

            public static void Reset()
            {
                isAvailable = true;
            }

            protected override bool IsKnown(in BrainContext context) => isAvailable;
        }

        private sealed class TestDecision : DecisionBase<TestDecision, int>
        {
            protected override int Decide(in BrainContext context)
            {
                bool hasKnowledge = context.brain.TryGetKnowledge(out TestKnowledge knowledge);
                return hasKnowledge ? knowledge.alertness : 0;
            }
        }

        private sealed class TrackingDecision : DecisionBase<TrackingDecision, int>
        {
            public static int decidedCount;
            public static BrainBase lastBrain;

            public static void Reset()
            {
                decidedCount = 0;
                lastBrain = null;
            }

            protected override int Decide(in BrainContext context) => 11;

            protected override void OnDecided(in BrainContext context, object decisionResult, in OperationResult result)
            {
                decidedCount++;
                lastBrain = context.brain;
            }
        }

        private sealed class UntypedDecision : DecisionBase<UntypedDecision>
        {
            protected override object DecideUnsafe(in BrainContext context) => "observed";
        }

        private sealed class DeniedDecision : DecisionBase<DeniedDecision, int>
        {
            public static int failedCount;
            public static BrainBase failedBrain;

            public static void Reset()
            {
                failedCount = 0;
                failedBrain = null;
            }

            protected override OperationResult CanDecide(in BrainContext context) => Denied();

            protected override int Decide(in BrainContext context) => 1;

            protected override void OnDecisionFailed(in BrainContext context, in OperationResult result)
            {
                failedCount++;
                failedBrain = context.brain;
            }
        }

        private sealed class InstanceCountingDecision : DecisionBase<InstanceCountingDecision, int>
        {
            public static int instanceCount;

            public InstanceCountingDecision()
            {
                instanceCount++;
            }

            protected override int Decide(in BrainContext context) => instanceCount;
        }

        private sealed class TrackingSubprocess : BrainSubprocessBase
        {
            public static bool allowStart;
            public static bool allowStop;
            public static bool allowPause;
            public static bool allowResume;
            public static int instanceCount;
            public static int startedCount;
            public static int startFailedCount;
            public static int stoppedCount;
            public static int stopFailedCount;
            public static int pausedCount;
            public static int pauseFailedCount;
            public static int resumedCount;
            public static int resumeFailedCount;
            public static bool lastPauseWasComaInduced;
            public static bool lastPauseFailureWasComaInduced;
            public static bool lastResumeWasComaInduced;
            public static bool lastResumeFailureWasComaInduced;

            public TrackingSubprocess()
            {
                instanceCount++;
            }

            public static void Reset()
            {
                allowStart = true;
                allowStop = true;
                allowPause = true;
                allowResume = true;
                instanceCount = 0;
                startedCount = 0;
                startFailedCount = 0;
                stoppedCount = 0;
                stopFailedCount = 0;
                pausedCount = 0;
                pauseFailedCount = 0;
                resumedCount = 0;
                resumeFailedCount = 0;
                lastPauseWasComaInduced = false;
                lastPauseFailureWasComaInduced = false;
                lastResumeWasComaInduced = false;
                lastResumeFailureWasComaInduced = false;
            }

            protected override OperationResult CanStart(in BrainSubprocessContext context)
            {
                return allowStart ? BrainOperations.Permitted() : Denied();
            }

            protected override OperationResult CanStop(in BrainSubprocessContext context)
            {
                return allowStop ? BrainOperations.Permitted() : Denied();
            }

            protected override OperationResult CanPause(in BrainSubprocessContext context)
            {
                return allowPause ? BrainOperations.Permitted() : Denied();
            }

            protected override OperationResult CanResume(in BrainSubprocessContext context)
            {
                return allowResume ? BrainOperations.Permitted() : Denied();
            }

            protected override void OnStarted(in BrainSubprocessContext context, in OperationResult result)
            {
                startedCount++;
            }

            protected override void OnStartFailed(in BrainSubprocessContext context, in OperationResult result)
            {
                startFailedCount++;
            }

            protected override void OnStopped(in BrainSubprocessContext context, in OperationResult result)
            {
                stoppedCount++;
            }

            protected override void OnStopFailed(in BrainSubprocessContext context, in OperationResult result)
            {
                stopFailedCount++;
            }

            protected override void OnPaused(in BrainSubprocessContext context, in OperationResult result)
            {
                pausedCount++;
                lastPauseWasComaInduced = context.isComaInduced;
            }

            protected override void OnPauseFailed(in BrainSubprocessContext context, in OperationResult result)
            {
                pauseFailedCount++;
                lastPauseFailureWasComaInduced = context.isComaInduced;
            }

            protected override void OnResumed(in BrainSubprocessContext context, in OperationResult result)
            {
                resumedCount++;
                lastResumeWasComaInduced = context.isComaInduced;
            }

            protected override void OnResumeFailed(in BrainSubprocessContext context, in OperationResult result)
            {
                resumeFailedCount++;
                lastResumeFailureWasComaInduced = context.isComaInduced;
            }

        }

        private sealed class SelfFinishingSubprocess : BrainSubprocessBase
        {
            public static int finishedCount;
            public static BrainBase finishedBrain;
            public static bool finishWasSuccessful;
            public static ushort finishResultCode;

            public static void Reset()
            {
                finishedCount = 0;
                finishedBrain = null;
                finishWasSuccessful = false;
                finishResultCode = default;
            }

            protected override void OnStarted(in BrainSubprocessContext context, in OperationResult result)
            {
                OperationResult finishOperationResult = Finish(context);
                finishWasSuccessful = OperationResult.IsSuccess(finishOperationResult);
                finishResultCode = finishOperationResult.resultCode;
            }

            protected override void OnFinished(in BrainSubprocessContext context, in OperationResult result)
            {
                finishedCount++;
                finishedBrain = context.brain;
            }
        }

        private sealed class ComaSubprocess : BrainSubprocessBase, ISubprocessAllowedInComa
        {
            public static void Reset()
            {
            }
        }

        private static OperationResult Denied()
        {
            return OperationResult.Error(BrainOperations.SYSTEM_BRAIN, OperationResult.ERROR_DENIED);
        }
    }
}
