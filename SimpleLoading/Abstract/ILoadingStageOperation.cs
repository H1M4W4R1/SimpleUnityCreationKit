using Systems.SimpleCore.Operations;
using Systems.SimpleLoading.Data;

namespace Systems.SimpleLoading.Abstract
{
    /// <summary>Per-request runtime work created from a <see cref="LoadingStageBase"/> asset.</summary>
    public interface ILoadingStageOperation
    {
        /// <summary>Starts this stage's work.</summary>
        OperationResult Begin(in LoadingContext context);

        /// <summary>Advances this stage's work and reports its current state.</summary>
        LoadingStageUpdate Update(in LoadingContext context, float deltaTime);

        /// <summary>Releases request-local work when the request is cancelled or released.</summary>
        void Cancel(in LoadingContext context);
    }
}
