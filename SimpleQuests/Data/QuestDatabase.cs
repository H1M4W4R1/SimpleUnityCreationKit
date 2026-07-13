using JetBrains.Annotations;
#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;
using Systems.SimpleCore.Identifiers;
#endif
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleQuests.Abstract;

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleQuests.Tests")]
#endif

namespace Systems.SimpleQuests.Data
{
    /// <summary>
    ///     Database containing all in-game quests and tasks.
    /// </summary>
    public sealed class QuestDatabase : AddressableDatabase<QuestDatabase, Quest>
    {
        public const string LABEL = "SimpleQuests.Quests";

        [NotNull] protected override string AddressableLabel => LABEL;

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] Quest quest)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<Quest>(HashIdentifier.New(quest.GetType()), quest));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
