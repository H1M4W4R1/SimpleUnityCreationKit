using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract.Platforms;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Storage.Lists;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif

namespace Systems.SimpleAchievements.Data.Databases
{
    /// <summary>
    ///     Lazy-loaded Addressable database of all <see cref="AchievementPlatformBase"/> assets in the project.
    ///     Assets are automatically registered via the <c>AutoCreate</c> attribute on
    ///     <see cref="AchievementPlatformBase"/> and must carry the <see cref="LABEL"/> Addressable label.
    /// </summary>
    public sealed class AchievementPlatformDatabase
        : AddressableDatabase<AchievementPlatformDatabase, AchievementPlatformBase>
    {
        /// <summary>Addressable label applied to all platform assets.</summary>
        public const string LABEL = "SimpleAchievements.Platforms";

        [NotNull] protected override string AddressableLabel => LABEL;

        public static ROListAccess<AchievementPlatformBase> GetAllPlatforms()
        {
            _instance.EnsureLoaded();

            RWListAccess<AchievementPlatformBase> access = RWListAccess<AchievementPlatformBase>.Create();
            List<AchievementPlatformBase> results = access.List;

            for (int entryIndex = 0; entryIndex < internalDataStorage.Count; entryIndex++)
            {
                AchievementPlatformBase platform = internalDataStorage[entryIndex].entryObject;
                if (ReferenceEquals(platform, null)) continue;

                bool alreadyAdded = false;
                for (int resultIndex = 0; resultIndex < results.Count; resultIndex++)
                {
                    if (!ReferenceEquals(results[resultIndex], platform)) continue;
                    alreadyAdded = true;
                    break;
                }

                if (!alreadyAdded) results.Add(platform);
            }

            return access.ToReadOnly();
        }

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] AchievementPlatformBase platform)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<AchievementPlatformBase>(
                    HashIdentifier.New(platform.GetType()), platform));
            internalDataStorage.Sort((left, right) => left.hashIdentifier.CompareTo(right.hashIdentifier));
        }

        internal static void ClearForTests()
        {
            internalDataStorage.Clear();
        }
#endif
    }
}
