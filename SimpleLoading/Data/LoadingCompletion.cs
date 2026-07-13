namespace Systems.SimpleLoading.Data
{
    /// <summary>Persistent terminal information for a request.</summary>
    public readonly struct LoadingCompletion
    {
        public readonly LoadingStatus status;
        public readonly ushort systemCode;
        public readonly ushort resultCode;
        public readonly uint userCode;

        internal LoadingCompletion(LoadingStatus status, ushort systemCode, ushort resultCode, uint userCode)
        {
            this.status = status;
            this.systemCode = systemCode;
            this.resultCode = resultCode;
            this.userCode = userCode;
        }

        /// <summary>Whether the request ended successfully.</summary>
        public bool IsSuccess => status == LoadingStatus.Completed;
    }
}
