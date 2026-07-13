namespace Systems.SimpleCore.Storage.Databases
{
    /// <summary>Asynchronous loading contract shared by Addressables-backed databases.</summary>
    /// <remarks>
    ///     The contract uses state polling rather than callbacks so consumers such as SimpleLoading stages do not
    ///     need public event APIs or allocations for subscriptions.
    /// </remarks>
    public interface IDatabaseLoading
    {
        /// <summary>Whether an Addressables request is currently running.</summary>
        bool IsLoading { get; }

        /// <summary>Whether the latest loading attempt has reached a terminal state.</summary>
        bool IsLoadingComplete { get; }

        /// <summary>Whether the latest loading attempt completed successfully.</summary>
        bool IsLoaded { get; }

        /// <summary>Current Addressables request progress in the 0..1 range.</summary>
        float CurrentLoadProgress { get; }

        /// <summary>Begins loading when the database is not already loading or loaded.</summary>
        void BeginLoading();
    }
}
