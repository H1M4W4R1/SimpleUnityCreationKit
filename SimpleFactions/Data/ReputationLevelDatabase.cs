using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data
{
    /// <summary>
    ///     Addressable database for all <see cref="ReputationLevelBase"/> assets.
    ///     Assets are automatically registered via the <c>AutoCreate</c> attribute and
    ///     loaded at runtime with the label <see cref="LABEL"/>.
    /// </summary>
    public sealed class ReputationLevelDatabase : AddressableDatabase<ReputationLevelDatabase, ReputationLevelBase>
    {
        /// <summary>Addressable label assigned to every reputation level asset.</summary>
        public const string LABEL = "SimpleFactions.ReputationLevels";

        /// <inheritdoc/>
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] ReputationLevelBase level)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<ReputationLevelBase>(HashIdentifier.New(level.GetType()), level));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
