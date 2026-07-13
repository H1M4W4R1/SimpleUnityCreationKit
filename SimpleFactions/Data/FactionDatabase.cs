using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleFactions.Abstract;

namespace Systems.SimpleFactions.Data
{
    /// <summary>
    ///     Addressable database for all <see cref="FactionBase"/> assets.
    ///     Assets are automatically registered via the <c>AutoCreate</c> attribute and
    ///     loaded at runtime with the label <see cref="LABEL"/>.
    /// </summary>
    public sealed class FactionDatabase : AddressableDatabase<FactionDatabase, FactionBase>
    {
        /// <summary>Addressable label assigned to every faction asset.</summary>
        public const string LABEL = "SimpleFactions.Factions";

        /// <inheritdoc/>
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] FactionBase faction)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<FactionBase>(HashIdentifier.New(faction.GetType()), faction));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
