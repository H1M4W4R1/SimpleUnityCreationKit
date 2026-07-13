using Systems.SimpleCore.Operations;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleLoading.Data;
using Systems.SimpleLoading.Operations;

namespace Systems.SimpleLoading.Abstract
{
    /// <summary>Generic stage that automatically starts and tracks an Addressables database load.</summary>
    public abstract class DatabaseLoadingStageBase<TDatabase> : LoadingStageBase
        where TDatabase : class, IDatabaseLoading
    {
        /// <summary>Returns the singleton instance of the database represented by this stage.</summary>
        protected abstract TDatabase Database { get; }

        /// <inheritdoc />
        public sealed override ILoadingStageOperation CreateOperation(in LoadingContext context)
            => new DatabaseOperation(Database);

        private sealed class DatabaseOperation : ILoadingStageOperation
        {
            private readonly TDatabase _database;

            public DatabaseOperation(TDatabase database)
            {
                _database = database;
            }

            public OperationResult Begin(in LoadingContext context)
            {
                if (ReferenceEquals(_database, null)) return LoadingOperations.AddressableDatabaseMissing();
                _database.BeginLoading();
                return LoadingOperations.Permitted();
            }

            public LoadingStageUpdate Update(in LoadingContext context, float deltaTime)
            {
                if (_database.IsLoaded) return LoadingStageUpdate.Complete();
                if (_database.IsLoadingComplete)
                {
                    OperationResult failedResult = LoadingOperations.AddressableDatabaseLoadingFailed();
                    return LoadingStageUpdate.Fail(in failedResult);
                }

                return LoadingStageUpdate.Continue(_database.CurrentLoadProgress);
            }

            public void Cancel(in LoadingContext context) { }
        }
    }
}
