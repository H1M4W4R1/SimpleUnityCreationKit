using Systems.SimpleCore.Operations;

namespace Systems.SimpleLoading.Data
{
    /// <summary>Result returned by <c>LoadingAPI.TryLoad</c>, including its type-safe handle on success.</summary>
    public readonly ref struct LoadingSequenceStartResult<TSequence>
        where TSequence : Abstract.LoadingSequenceBase
    {
        public readonly OperationResult result;
        public readonly LoadingSequenceHandle<TSequence> handle;

        internal LoadingSequenceStartResult(
            in OperationResult result,
            in LoadingSequenceHandle<TSequence> handle)
        {
            this.result = result;
            this.handle = handle;
        }
    }
}
