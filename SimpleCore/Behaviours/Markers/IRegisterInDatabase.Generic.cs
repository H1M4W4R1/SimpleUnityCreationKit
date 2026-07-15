using Systems.SimpleCore.Storage.Databases;

namespace Systems.SimpleCore.Behaviours.Markers
{
    /// <summary>
    ///     Selects <typeparamref name="TDatabase"/> for automatic behaviour registration. A behaviour normally
    ///     implements one such contract. For several contracts, implement <see cref="IRegisterInDatabase"/>
    ///     directly and select the one database to receive automatic registration.
    /// </summary>
    /// <typeparam name="TDatabase">Concrete runtime database that stores this behaviour.</typeparam>
    public interface IRegisterInDatabase<TDatabase> : IRegisterInDatabase
        where TDatabase : IRuntimeDatabase, new()
    {
        bool IRegisterInDatabase.RegisterInDatabase(object item)
        {
            return RuntimeDatabaseProvider<TDatabase>.Instance.Register(item);
        }

        void IRegisterInDatabase.UnregisterFromDatabase(object item)
        {
            RuntimeDatabaseProvider<TDatabase>.Instance.Unregister(item);
        }
    }

    internal static class RuntimeDatabaseProvider<TDatabase>
        where TDatabase : IRuntimeDatabase, new()
    {
        internal static readonly TDatabase Instance = new TDatabase();
    }
}
