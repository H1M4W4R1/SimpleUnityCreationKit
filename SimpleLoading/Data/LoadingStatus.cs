namespace Systems.SimpleLoading.Data
{
    /// <summary>Lifetime state of a loading request.</summary>
    public enum LoadingStatus : byte
    {
        Invalid = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
