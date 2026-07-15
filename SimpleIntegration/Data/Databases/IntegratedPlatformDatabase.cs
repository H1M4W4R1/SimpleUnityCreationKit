using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleIntegration.Abstract;
using Systems.SimpleCore.Storage.Databases;
using Systems.SimpleCore.Storage.Lists;
#if UNITY_INCLUDE_TESTS
using Systems.SimpleCore.Identifiers;
#endif

[assembly: InternalsVisibleTo("SimpleAchievements.Tests")]
[assembly: InternalsVisibleTo("SimpleIntegration.Tests")]

namespace Systems.SimpleIntegration.Data.Databases
{
    /// <summary>Lazy-loaded Addressables database for all configured platform integrations.</summary>
    public sealed class IntegratedPlatformDatabase
        : AddressableDatabase<IntegratedPlatformDatabase, IntegratedPlatformBase>
    {
        /// <summary>Addressables label applied to all integration configuration assets.</summary>
        public const string LABEL = "SimpleIntegration.Platforms";

        [NotNull] protected override string AddressableLabel => LABEL;

        /// <summary>Gets every configured integration without duplicate base-type entries.</summary>
        public static ROListAccess<IntegratedPlatformBase> GetAllPlatforms()
        {
            _instance.EnsureLoaded();

            RWListAccess<IntegratedPlatformBase> access = RWListAccess<IntegratedPlatformBase>.Create();
            List<IntegratedPlatformBase> results = access.List;

            for (int entryIndex = 0; entryIndex < internalDataStorage.Count; entryIndex++)
            {
                IntegratedPlatformBase platform = internalDataStorage[entryIndex].entryObject;
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

        /// <summary>Gets integrations that implement a requested feature contract.</summary>
        internal static ROListAccess<TContract> GetAllWithContract<TContract>()
            where TContract : class
        {
            ROListAccess<IntegratedPlatformBase> platforms = GetAllPlatforms();
            RWListAccess<TContract> access = RWListAccess<TContract>.Create();
            List<TContract> results = access.List;
            IReadOnlyList<IntegratedPlatformBase> platformList = platforms.List;

            for (int platformIndex = 0; platformIndex < platformList.Count; platformIndex++)
            {
                IntegratedPlatformBase platform = platformList[platformIndex];
                if (platform is TContract contract) results.Add(contract);
            }

            platforms.Release();
            return access.ToReadOnly();
        }

#if UNITY_INCLUDE_TESTS
        internal static void RegisterForTests([NotNull] IntegratedPlatformBase platform)
        {
            internalDataStorage.Add(
                new AddressableDatabaseEntry<IntegratedPlatformBase>(
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
