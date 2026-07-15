using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleRelations.Abstract;

namespace Systems.SimpleRelations.Data
{
    /// <summary>Addressable database containing every generated <see cref="RelationTypeBase"/> asset.</summary>
    public sealed class RelationTypeDatabase : AddressableDatabase<RelationTypeDatabase, RelationTypeBase>
    {
        /// <summary>Addressables label assigned to relation type assets.</summary>
        public const string LABEL = "SimpleRelations.Types";

        /// <inheritdoc />
        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] RelationTypeBase relationType)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<RelationTypeBase>(HashIdentifier.New(relationType.GetType()), relationType));
            internalDataStorage.Add(
                new AddressableDatabaseEntry<RelationTypeBase>(HashIdentifier.New(typeof(RelationTypeBase)), relationType));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
