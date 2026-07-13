using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleLoot.Abstract.Generator;
#if UNITY_INCLUDE_TESTS
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using System.Runtime.CompilerServices;
#endif

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleLoot.Tests")]
#endif

namespace Systems.SimpleLoot.Data
{
    public sealed class LootGeneratorDatabase : AddressableDatabase<LootGeneratorDatabase, LootDropGeneratorBase>
    {
        public const string LABEL = "SimpleLoot.LootGenerators";
        protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] LootDropGeneratorBase generator)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<LootDropGeneratorBase>(
                    HashIdentifier.New(generator.GetType()), generator));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
