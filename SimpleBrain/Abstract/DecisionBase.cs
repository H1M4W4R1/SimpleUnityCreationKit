using JetBrains.Annotations;
using Systems.SimpleBrain.Components;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;

namespace Systems.SimpleBrain.Abstract
{
    /// <summary>
    ///     A decision that produces a result for a particular <see cref="BrainBase"/>.
    /// </summary>
    public abstract class DecisionBase
    {
        internal OperationResult TryDecide(in BrainContext context, [CanBeNull] out object decisionResult)
        {
            decisionResult = null;

            OperationResult canDecideResult = CanDecide(context);
            if (!canDecideResult)
            {
                OnDecisionFailed(context, canDecideResult);
                return canDecideResult;
            }

            decisionResult = DecideUnsafe(context);
            OperationResult decidedResult = BrainOperations.DecisionMade();
            OnDecided(context, decisionResult, decidedResult);
            return decidedResult;
        }

        /// <summary>
        ///     Determines whether this decision can currently be made.
        /// </summary>
        protected virtual OperationResult CanDecide(in BrainContext context) => BrainOperations.Permitted();

        /// <summary>
        ///     Produces this decision's value.
        /// </summary>
        [CanBeNull] protected abstract object DecideUnsafe(in BrainContext context);

        /// <summary>
        ///     Called after <see cref="DecideUnsafe"/> returns a value.
        /// </summary>
        protected virtual void OnDecided(
            in BrainContext context,
            [CanBeNull] object decisionResult,
            in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when <see cref="CanDecide"/> rejects the decision.
        /// </summary>
        protected virtual void OnDecisionFailed(in BrainContext context, in OperationResult result)
        {
        }
    }

    /// <summary>
    ///     A cached decision implementation that returns an untyped result.
    /// </summary>
    /// <remarks>
    ///     The closed generic type owns one decision instance. Implementations must not store state that varies per
    ///     brain or invocation; keep that state on the brain, its actor, or a knowledge instance instead.
    /// </remarks>
    public abstract class DecisionBase<TSelf> : DecisionBase
        where TSelf : DecisionBase<TSelf>, new()
    {
        private static TSelf _instance;

        /// <summary>
        ///     Retrieves the single cached instance for this concrete decision type.
        /// </summary>
        public static TSelf GetInstance()
        {
            if (!ReferenceEquals(_instance, null)) return _instance;

            _instance = new TSelf();
            return _instance;
        }
    }

    /// <summary>
    ///     A cached decision implementation that returns a strongly typed result.
    /// </summary>
    public abstract class DecisionBase<TSelf, TDecisionResult> : DecisionBase<TSelf>
        where TSelf : DecisionBase<TSelf, TDecisionResult>, new()
    {
        [CanBeNull] protected sealed override object DecideUnsafe(in BrainContext context) => Decide(context);

        /// <summary>
        ///     Produces this decision's strongly typed value.
        /// </summary>
        [CanBeNull] protected abstract TDecisionResult Decide(in BrainContext context);
    }
}
