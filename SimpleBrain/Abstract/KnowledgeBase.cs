using Systems.SimpleBrain.Components;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleBrain.Operations;
using Systems.SimpleCore.Operations;

namespace Systems.SimpleBrain.Abstract
{
    /// <summary>
    ///     Describes a single capability or fact that a <see cref="BrainBase"/> can learn.
    /// </summary>
    /// <remarks>
    ///     Knowledge instances are owned by their brain's runtime storage. Keep implementation state on the brain or
    ///     actor rather than on this abstraction when the state is not intrinsic to the knowledge itself.
    /// </remarks>
    [System.Serializable]
    public abstract class KnowledgeBase
    {
        internal OperationResult CanBeLearnedBy(in BrainContext context)
        {
            return CanLearn(context);
        }

        internal bool IsKnownBy(in BrainContext context) => IsKnown(context);

        internal void NotifyLearned(in BrainContext context, in OperationResult result)
        {
            OnLearned(context, result);
        }

        internal void NotifyLearningFailed(in BrainContext context, in OperationResult result)
        {
            OnLearningFailed(context, result);
        }

        /// <summary>
        ///     Determines whether this knowledge may be learned by <paramref name="brain"/>.
        /// </summary>
        protected virtual OperationResult CanLearn(in BrainContext context) => BrainOperations.Permitted();

        /// <summary>
        ///     Determines whether learned knowledge is currently available to <paramref name="brain"/>.
        /// </summary>
        protected virtual bool IsKnown(in BrainContext context) => true;

        /// <summary>
        ///     Called after this knowledge has been stored by <paramref name="brain"/>.
        /// </summary>
        protected virtual void OnLearned(in BrainContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called when <see cref="CanLearn"/> prevents the brain from learning this knowledge.
        /// </summary>
        protected virtual void OnLearningFailed(in BrainContext context, in OperationResult result)
        {
        }
    }
}
