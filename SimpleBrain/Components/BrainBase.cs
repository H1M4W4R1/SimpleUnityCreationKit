using JetBrains.Annotations;
using Systems.SimpleBrain.Abstract;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;
using UnityEngine;

namespace Systems.SimpleBrain.Components
{
    /// <summary>
    ///     Actor component that owns knowledge, decisions, and long-running AI subprocesses.
    /// </summary>
    /// <remarks>
    ///     This is the entry point for all SimpleBrain operations. It intentionally does not cache another actor
    ///     component: a brain belongs on the actor that it controls.
    /// </remarks>
    public abstract class BrainBase : SimpleBehaviour, IAwakeBehaviour, ITickableBehaviour
    {
        [SerializeField, HideInInspector] private BrainKnowledgeStorage _knowledgeStorage = new BrainKnowledgeStorage();
        [SerializeField, HideInInspector] private BrainSubprocessStorage _subprocessStorage = new BrainSubprocessStorage();

        /// <summary>
        ///     Whether ordinary brain and subprocess ticks are suspended.
        /// </summary>
        public bool IsInComa { get; private set; }

        /// <summary>
        ///     Determines whether this brain has ever learned the requested knowledge.
        /// </summary>
        public bool HasLearned<TKnowledge>()
            where TKnowledge : KnowledgeBase
        {
            return _knowledgeStorage.HasLearned<TKnowledge>();
        }

        /// <summary>
        ///     Determines whether learned knowledge is currently available to this brain.
        /// </summary>
        public bool Knows<TKnowledge>()
            where TKnowledge : KnowledgeBase
        {
            return _knowledgeStorage.Knows<TKnowledge>(this);
        }

        /// <summary>
        ///     Retrieves learned knowledge so decisions can inspect its serialized state.
        /// </summary>
        public bool TryGetKnowledge<TKnowledge>(out TKnowledge knowledge)
            where TKnowledge : KnowledgeBase
        {
            return _knowledgeStorage.TryGetKnowledge(out knowledge);
        }

        /// <summary>
        ///     Creates and stores the requested knowledge if it has not already been learned.
        /// </summary>
        public OperationResult TryLearn<TKnowledge>()
            where TKnowledge : KnowledgeBase, new()
        {
            return _knowledgeStorage.TryLearn<TKnowledge>(this);
        }

        /// <summary>
        ///     Creates the requested subprocess if necessary and starts it from its stopped state.
        /// </summary>
        public OperationResult TryStartSubprocess<TSubprocess>()
            where TSubprocess : BrainSubprocessBase, new()
        {
            OperationResult startResult = _subprocessStorage.TryStart<TSubprocess>(this);
            if (startResult && IsInComa) _subprocessStorage.PauseForComa(this);
            return startResult;
        }

        /// <summary>
        ///     Stops a created subprocess while retaining it for a later restart.
        /// </summary>
        public OperationResult TryStopSubprocess<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.TryStop<TSubprocess>(this);
        }

        /// <summary>
        ///     Pauses a running subprocess.
        /// </summary>
        public OperationResult TryPauseSubprocess<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.TryPause<TSubprocess>(this);
        }

        /// <summary>
        ///     Resumes a paused subprocess.
        /// </summary>
        public OperationResult TryResumeSubprocess<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.TryResume<TSubprocess>(this);
        }

        public bool IsSubprocessCreated<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.IsCreated<TSubprocess>();
        }

        public bool IsSubprocessRunning<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.IsRunning<TSubprocess>();
        }

        public bool IsSubprocessPaused<TSubprocess>()
            where TSubprocess : BrainSubprocessBase
        {
            return _subprocessStorage.IsPaused<TSubprocess>();
        }

        /// <summary>
        ///     Executes an assessment, which is an alias for <see cref="TryDecide{TDecision}(out object)"/>.
        /// </summary>
        public OperationResult TryAssess<TAssessment>([CanBeNull] out object assessmentResult)
            where TAssessment : DecisionBase<TAssessment>, new()
        {
            return TryDecide<TAssessment>(out assessmentResult);
        }

        /// <summary>
        ///     Executes the cached instance of a decision.
        /// </summary>
        public OperationResult TryDecide<TDecision>([CanBeNull] out object decisionResult)
            where TDecision : DecisionBase<TDecision>, new()
        {
            TDecision decision = DecisionBase<TDecision>.GetInstance();
            BrainContext context = new BrainContext(this);
            return decision.TryDecide(context, out decisionResult);
        }

        /// <summary>
        ///     Executes a strongly typed assessment.
        /// </summary>
        public OperationResult TryAssess<TAssessment, TAssessmentResult>([CanBeNull] out TAssessmentResult assessmentResult)
            where TAssessment : DecisionBase<TAssessment, TAssessmentResult>, new()
        {
            return TryDecide<TAssessment, TAssessmentResult>(out assessmentResult);
        }

        /// <summary>
        ///     Executes the cached instance of a strongly typed decision.
        /// </summary>
        public OperationResult TryDecide<TDecision, TDecisionResult>([CanBeNull] out TDecisionResult decisionResult)
            where TDecision : DecisionBase<TDecision, TDecisionResult>, new()
        {
            TDecision decision = DecisionBase<TDecision, TDecisionResult>.GetInstance();
            BrainContext context = new BrainContext(this);
            OperationResult result = decision.TryDecide(context, out object untypedResult);
            if (untypedResult is TDecisionResult typedResult)
            {
                decisionResult = typedResult;
                return result;
            }

            decisionResult = default;
            return result;
        }

        /// <summary>
        ///     Suspends ordinary brain and subprocess ticks after the coma-entry condition passes.
        /// </summary>
        public OperationResult TryEnterComa()
        {
            if (IsInComa) return BrainOperations.AlreadyInComa();

            OperationResult canEnterComaResult = CanEnterComa();
            if (!canEnterComaResult)
            {
                OnComaEntryFailed(canEnterComaResult);
                return canEnterComaResult;
            }

            IsInComa = true;
            OperationResult enteredComaResult = BrainOperations.ComaEntered();
            OnComaEntered(enteredComaResult);
            return enteredComaResult;
        }

        /// <summary>
        ///     Resumes ordinary brain and subprocess ticks after the coma-exit condition passes.
        /// </summary>
        public OperationResult TryExitComa()
        {
            if (!IsInComa) return BrainOperations.ComaExited();

            OperationResult canExitComaResult = CanExitComa();
            if (!canExitComaResult)
            {
                OnComaExitFailed(canExitComaResult);
                return canExitComaResult;
            }

            IsInComa = false;
            OperationResult exitedComaResult = BrainOperations.ComaExited();
            OnComaExited(exitedComaResult);
            return exitedComaResult;
        }

        internal OperationResult TryFinishSubprocess([CanBeNull] BrainSubprocessBase subprocess)
        {
            if (ReferenceEquals(subprocess, null)) return BrainOperations.SubprocessNotOwned();
            return _subprocessStorage.TryFinish(this, subprocess);
        }

        /// <summary>
        ///     Called once when the component is created.
        /// </summary>
        protected virtual void OnBrainBorn()
        {
        }

        /// <summary>
        ///     Called on the global tick system while this brain is not in a coma.
        /// </summary>
        protected virtual void OnBrainTick(float deltaTimeSeconds)
        {
        }

        /// <summary>
        ///     Called on the global tick system while this brain is in a coma.
        /// </summary>
        protected virtual void OnBrainComaTick(float deltaTimeSeconds)
        {
        }

        protected virtual OperationResult CanEnterComa() => BrainOperations.Permitted();

        protected virtual OperationResult CanExitComa() => BrainOperations.Permitted();

        protected virtual void OnComaEntered(in OperationResult result)
        {
            _subprocessStorage.PauseForComa(this);
        }

        protected virtual void OnComaEntryFailed(in OperationResult result)
        {
        }

        protected virtual void OnComaExited(in OperationResult result)
        {
            _subprocessStorage.ResumeAfterComa(this);
        }

        protected virtual void OnComaExitFailed(in OperationResult result)
        {
        }

        protected override void OnBehaviourAwake()
        {
            OnBrainBorn();
        }

        protected override void OnTick(float deltaTimeSeconds)
        {
            if (IsInComa)
            {
                OnBrainComaTick(deltaTimeSeconds);
                _subprocessStorage.TickAllowedInComa(this, deltaTimeSeconds);
                return;
            }

            OnBrainTick(deltaTimeSeconds);
            _subprocessStorage.Tick(this, deltaTimeSeconds);
        }
    }
}
