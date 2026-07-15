using Systems.SimpleCore.Automation.Attributes;
using Systems.SimpleIntegration.Data.Databases;
using UnityEngine;

namespace Systems.SimpleIntegration.Abstract
{
    /// <summary>Base configuration asset for an external platform integration.</summary>
    /// <remarks>
    ///     Concrete implementations are created under <c>Assets/Generated/IntegratedPlatforms/</c>
    ///     and registered in <see cref="IntegratedPlatformDatabase"/>. Implement feature contracts,
    ///     such as <c>IAchievementPlatform</c>, to make a platform available to a system.
    /// </remarks>
    [AutoCreate("IntegratedPlatforms", IntegratedPlatformDatabase.LABEL)]
    public abstract class IntegratedPlatformBase : ScriptableObject
    {
        /// <summary>Display name shown in the Integrations project settings.</summary>
        public abstract string PlatformName { get; }

        /// <summary>Whether this integration initialized successfully and can provide its contracts.</summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>Initializes the external SDK before feature contracts are used.</summary>
        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        /// <summary>Releases external SDK resources when the application quits.</summary>
        public virtual void Shutdown()
        {
            IsInitialized = false;
        }

#if UNITY_EDITOR
        /// <summary>Draws this integration's settings in the project settings window.</summary>
        public abstract void DrawSettings(UnityEditor.SerializedObject serializedObject);
#endif
    }
}
