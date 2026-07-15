using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleGame.Abstract;

namespace Systems.SimpleGame.Data
{
    /// <summary>Addressable database for auto-created <see cref="GameModeBase"/> assets.</summary>
    public sealed class GameModeDatabase : AddressableDatabase<GameModeDatabase, GameModeBase>
    {
        /// <summary>Addressable label assigned to every game-mode asset.</summary>
        public const string LABEL = "SimpleGame.GameModes";

        /// <inheritdoc/>
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] GameModeBase gameMode)
        {
            UseTestStorage();
            internalDataStorage.Add(new AddressableDatabaseEntry<GameModeBase>(
                HashIdentifier.New(gameMode.GetType()), gameMode));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
