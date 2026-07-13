using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems.SimpleCore.Utility
{
    /// <summary>
    ///     Starts the Unity Localization initialization operation before scene content requests localized values.
    /// </summary>
    public static class LocalizationAPI
    {
        /// <summary>
        ///     Gets whether Unity Localization has completed initialization.
        /// </summary>
        public static bool IsInitialized => LocalizationSettings.InitializationOperation.IsDone;

        /// <summary>
        ///     Starts and returns the shared Unity Localization initialization operation.
        /// </summary>
        public static AsyncOperationHandle<LocalizationSettings> Initialize() =>
            LocalizationSettings.InitializationOperation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            Initialize();
        }
    }
}
