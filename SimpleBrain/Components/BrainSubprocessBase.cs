using Systems.SimpleBrain.Operations;
using Systems.SimpleBrain.Data.Context;
using Systems.SimpleCore.Operations;

namespace Systems.SimpleBrain.Components
{
    /// <summary>
    ///     A reusable, brain-owned decision process with running and paused states.
    /// </summary>
    /// <remarks>
    ///     State and ownership are held by <see cref="BrainBase"/>, so subprocess implementations do not need an
    ///     owner field. Every callback receives a stack-only operation context.
    /// </remarks>
    [System.Serializable]
    public abstract class BrainSubprocessBase
    {
        internal OperationResult CanBeStartedBy(in BrainSubprocessContext context)
        {
            return CanStart(context);
        }

        internal OperationResult CanBeStoppedBy(in BrainSubprocessContext context)
        {
            return CanStop(context);
        }

        internal OperationResult CanBePausedBy(in BrainSubprocessContext context)
        {
            return CanPause(context);
        }

        internal OperationResult CanBeResumedBy(in BrainSubprocessContext context)
        {
            return CanResume(context);
        }

        internal void NotifyStarted(in BrainSubprocessContext context, in OperationResult result)
        {
            OnStarted(context, result);
        }

        internal void NotifyStartFailed(in BrainSubprocessContext context, in OperationResult result)
        {
            OnStartFailed(context, result);
        }

        internal void NotifyStopped(in BrainSubprocessContext context, in OperationResult result)
        {
            OnStopped(context, result);
        }

        internal void NotifyStopFailed(in BrainSubprocessContext context, in OperationResult result)
        {
            OnStopFailed(context, result);
        }

        internal void NotifyPaused(in BrainSubprocessContext context, in OperationResult result)
        {
            OnPaused(context, result);
        }

        internal void NotifyPauseFailed(in BrainSubprocessContext context, in OperationResult result)
        {
            OnPauseFailed(context, result);
        }

        internal void NotifyResumed(in BrainSubprocessContext context, in OperationResult result)
        {
            OnResumed(context, result);
        }

        internal void NotifyResumeFailed(in BrainSubprocessContext context, in OperationResult result)
        {
            OnResumeFailed(context, result);
        }

        internal void Tick(in BrainSubprocessContext context)
        {
            OnTick(context);
        }

        internal void NotifyFinished(in BrainSubprocessContext context, in OperationResult result)
        {
            OnFinished(context, result);
        }

        /// <summary>
        ///     Stops this subprocess as successfully finished.
        /// </summary>
        protected OperationResult Finish(in BrainSubprocessContext context)
        {
            return context.brain.TryFinishSubprocess(this);
        }

        /// <summary>
        ///     Determines whether this process can start from its stopped state.
        /// </summary>
        protected virtual OperationResult CanStart(in BrainSubprocessContext context) => BrainOperations.Permitted();

        /// <summary>
        ///     Determines whether this process can stop.
        /// </summary>
        protected virtual OperationResult CanStop(in BrainSubprocessContext context) => BrainOperations.Permitted();

        /// <summary>
        ///     Determines whether this process can pause.
        /// </summary>
        protected virtual OperationResult CanPause(in BrainSubprocessContext context) => BrainOperations.Permitted();

        /// <summary>
        ///     Determines whether this process can resume.
        /// </summary>
        protected virtual OperationResult CanResume(in BrainSubprocessContext context) => BrainOperations.Permitted();

        protected virtual void OnStarted(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnStartFailed(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnStopped(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnStopFailed(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnPaused(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnPauseFailed(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnResumed(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        protected virtual void OnResumeFailed(in BrainSubprocessContext context, in OperationResult result)
        {
        }

        /// <summary>
        ///     Called by the brain tick system while this subprocess is running.
        /// </summary>
        protected virtual void OnTick(in BrainSubprocessContext context)
        {
        }

        /// <summary>
        ///     Called after <see cref="Finish"/> changes this subprocess to its stopped state.
        /// </summary>
        protected virtual void OnFinished(in BrainSubprocessContext context, in OperationResult result)
        {
        }
    }
}
