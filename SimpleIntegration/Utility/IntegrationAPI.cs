using JetBrains.Annotations;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleIntegration.Abstract;
using Systems.SimpleIntegration.Data.Databases;
using UnityEngine;

namespace Systems.SimpleIntegration.Utility
{
    /// <summary>Provides feature-contract discovery for configured platform integrations.</summary>
    public static class IntegrationAPI
    {
        private static bool _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Application.quitting -= ShutdownAll;
            Application.quitting += ShutdownAll;
            _isInitialized = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeAtStartup()
        {
            EnsureInitialized();
        }

        /// <summary>Returns whether at least one configured integration implements a contract.</summary>
        public static bool IsAvailable<TContract>()
            where TContract : class
        {
            ROListAccess<TContract> integrations = GetAvailable<TContract>();
            bool isAvailable = integrations.List.Count > 0;
            integrations.Release();
            return isAvailable;
        }

        /// <summary>
        ///     Gets every initialized integration that implements a feature contract.
        ///     The returned list must be released after use.
        /// </summary>
        public static ROListAccess<TContract> GetAvailable<TContract>()
            where TContract : class
        {
            EnsureInitialized();

            ROListAccess<TContract> contracts =
                IntegratedPlatformDatabase.GetAllWithContract<TContract>();
            RWListAccess<TContract> access = RWListAccess<TContract>.Create();
            for (int contractIndex = 0; contractIndex < contracts.List.Count; contractIndex++)
            {
                TContract contract = contracts.List[contractIndex];
                if (contract is not IntegratedPlatformBase platform) continue;
                if (!platform || !platform.IsInitialized) continue;
                access.List.Add(contract);
            }

            contracts.Release();
            return access.ToReadOnly();
        }

        private static void EnsureInitialized()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            ROListAccess<IntegratedPlatformBase> platforms = IntegratedPlatformDatabase.GetAllPlatforms();
            for (int platformIndex = 0; platformIndex < platforms.List.Count; platformIndex++)
            {
                IntegratedPlatformBase platform = platforms.List[platformIndex];
                if (platform) platform.Initialize();
            }

            platforms.Release();
        }

        private static void ShutdownAll()
        {
            if (!_isInitialized) return;

            ROListAccess<IntegratedPlatformBase> platforms = IntegratedPlatformDatabase.GetAllPlatforms();
            for (int platformIndex = 0; platformIndex < platforms.List.Count; platformIndex++)
            {
                IntegratedPlatformBase platform = platforms.List[platformIndex];
                if (platform && platform.IsInitialized) platform.Shutdown();
            }

            platforms.Release();
            _isInitialized = false;
        }

#if UNITY_INCLUDE_TESTS
        internal static void ResetForTests()
        {
            ShutdownAll();
            _isInitialized = false;
        }
#endif
    }
}
