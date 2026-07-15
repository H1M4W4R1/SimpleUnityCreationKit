using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleGame.Abstract;

namespace Systems.SimpleGame.Data
{
    /// <summary>Addressable database for auto-created <see cref="GameStateBase"/> assets.</summary>
    public sealed class GameStateDatabase : AddressableDatabase<GameStateDatabase, GameStateBase>
    {
        /// <summary>Addressable label assigned to every game-state asset.</summary>
        public const string LABEL = "SimpleGame.GameStates";

        /// <inheritdoc/>
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] GameStateBase gameState)
        {
            UseTestStorage();
            internalDataStorage.Add(new AddressableDatabaseEntry<GameStateBase>(
                HashIdentifier.New(gameState.GetType()), gameState));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
