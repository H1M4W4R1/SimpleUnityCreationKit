using Systems.SimpleCore.Operations;

namespace Systems.SimpleLoading.Data
{
    /// <summary>One non-allocating progress report produced by a loading stage operation.</summary>
    public readonly ref struct LoadingStageUpdate
    {
        /// <summary>Result of this update. An error completes the request as failed.</summary>
        public readonly OperationResult result;

        /// <summary>Whether the current stage has completed.</summary>
        public readonly bool isComplete;

        /// <summary>Current stage progress, clamped by the API to the 0..1 range.</summary>
        public readonly float progress;

        private LoadingStageUpdate(in OperationResult result, bool isComplete, float progress)
        {
            this.result = result;
            this.isComplete = isComplete;
            this.progress = progress;
        }

        /// <summary>Reports work that is still in progress.</summary>
        public static LoadingStageUpdate Continue(float progress)
        {
            OperationResult result = Operations.LoadingOperations.Permitted();
            return new LoadingStageUpdate(in result, false, progress);
        }

        /// <summary>Reports a successfully completed stage.</summary>
        public static LoadingStageUpdate Complete()
        {
            OperationResult result = Operations.LoadingOperations.Completed();
            return new LoadingStageUpdate(in result, true, 1f);
        }

        /// <summary>Reports a failed stage.</summary>
        public static LoadingStageUpdate Fail(in OperationResult result)
            => new LoadingStageUpdate(in result, true, 0f);
    }
}
