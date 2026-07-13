namespace Systems.SimpleLoading.Data
{
    /// <summary>Type-safe handle for a request created from <typeparamref name="TSequence"/>.</summary>
    public readonly struct LoadingSequenceHandle<TSequence>
        where TSequence : Abstract.LoadingSequenceBase
    {
        internal readonly LoadingHandle handle;

        internal LoadingSequenceHandle(in LoadingHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Whether this handle was produced by a successful load request.</summary>
        public bool IsValid => handle.IsValid;
    }
}
