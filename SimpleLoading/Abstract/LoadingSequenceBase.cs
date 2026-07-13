using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Data;
using UnityEngine;

namespace Systems.SimpleLoading.Abstract
{
    /// <summary>Ordered, reusable configuration for loading save, gameplay, or scene data.</summary>
    public abstract class LoadingSequenceBase : ScriptableObject
    {
        [SerializeReference] private LoadingStageBase[] _stages = System.Array.Empty<LoadingStageBase>();

        /// <summary>Number of configured stages. Override for code-defined sequences.</summary>
        protected internal virtual int GetStageCount() => _stages.Length;

        /// <summary>Gets a stage by index. Override for code-defined sequences.</summary>
        protected internal virtual LoadingStageBase GetStage(int stageIndex) => _stages[stageIndex];

        /// <summary>Validates a request before any stage begins.</summary>
        protected internal virtual OperationResult CanStartLoading(in LoadingContext context)
            => Operations.LoadingOperations.Permitted();

        /// <summary>Called after the request becomes visible to loading screens.</summary>
        protected internal virtual void OnLoadingStarted(in LoadingContext context) { }

        /// <summary>Called immediately before a stage operation begins.</summary>
        protected internal virtual void OnStageStarted(in LoadingContext context, int stageIndex) { }

        /// <summary>Called after a stage operation succeeds.</summary>
        protected internal virtual void OnStageCompleted(in LoadingContext context, int stageIndex) { }

        /// <summary>Called after every stage progress change.</summary>
        protected internal virtual void OnLoadingProgressed(in LoadingContext context, float progress) { }

        /// <summary>Called once after all stages complete successfully.</summary>
        protected internal virtual void OnLoadingCompleted(in LoadingContext context, in OperationResult result) { }

        /// <summary>Called when validation or a stage fails.</summary>
        protected internal virtual void OnLoadingFailed(in LoadingContext context, in OperationResult result) { }

        /// <summary>Called when the caller cancels a request.</summary>
        protected internal virtual void OnLoadingCancelled(in LoadingContext context) { }
    }
}
