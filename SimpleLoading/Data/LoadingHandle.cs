namespace Systems.SimpleLoading.Data
{
    /// <summary>Stable identifier for a request started through <see cref="Utility.LoadingAPI"/>.</summary>
    public readonly struct LoadingHandle
    {
        internal readonly int id;
        internal readonly int version;

        internal LoadingHandle(int id, int version)
        {
            this.id = id;
            this.version = version;
        }

        /// <summary>Whether this handle was produced by a successful start operation.</summary>
        public bool IsValid => id > 0;
    }
}
