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

            decisionResult = Decide(context);
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
        [CanBeNull] protected abstract object Decide(in BrainContext context);

        /// <summary>
        ///     Called after <see cref="Decide"/> returns a value.
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
    ///     A strongly typed <see cref="DecisionBase"/> result.
    /// </summary>
    public abstract class DecisionBase<TDecisionResult> : DecisionBase
    {
        [CanBeNull] protected sealed override object Decide(in BrainContext context) => DecideTyped(context);

        /// <summary>
        ///     Produces this decision's strongly typed value.
        /// </summary>
        [CanBeNull] protected abstract TDecisionResult DecideTyped(in BrainContext context);
    }
}
