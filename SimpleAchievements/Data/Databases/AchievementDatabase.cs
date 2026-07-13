using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Storage.Lists;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
using System.Runtime.CompilerServices;
#endif

#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("SimpleAchievements.Tests")]
#endif

namespace Systems.SimpleAchievements.Data.Databases
{
    /// <summary>
    ///     Lazy-loaded Addressable database of all <see cref="AchievementData"/> assets in the project.
    ///     Assets are automatically registered via the <c>AutoCreate</c> attribute on
    ///     <see cref="AchievementData"/> and must carry the <see cref="LABEL"/> Addressable label.
    /// </summary>
    public sealed class AchievementDatabase : AddressableDatabase<AchievementDatabase, AchievementData>
    {
        /// <summary>Addressable label applied to all achievement assets.</summary>
        public const string LABEL = "SimpleAchievements.Achievements";

        [NotNull] protected override string AddressableLabel => LABEL;

       public static ROListAccess<AchievementData> GetAllAchievements()
        {
            _instance.EnsureLoaded();

            RWListAccess<AchievementData> access = RWListAccess<AchievementData>.Create();
            List<AchievementData> results = access.List;

            for (int entryIndex = 0; entryIndex < internalDataStorage.Count; entryIndex++)
            {
                AchievementData achievement = internalDataStorage[entryIndex].entryObject;
                if (ReferenceEquals(achievement, null)) continue;

                bool alreadyAdded = false;
                for (int resultIndex = 0; resultIndex < results.Count; resultIndex++)
                {
                    if (!ReferenceEquals(results[resultIndex], achievement)) continue;
                    alreadyAdded = true;
                    break;
                }

                if (!alreadyAdded) results.Add(achievement);
            }

            return access.ToReadOnly();
        }

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] AchievementData achievement)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<AchievementData>(
                    HashIdentifier.New(achievement.GetType()), achievement));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
