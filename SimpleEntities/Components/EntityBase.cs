using UnityEngine;

using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;

namespace Systems.SimpleEntities.Components
{
    /// <summary>
    ///     Simple entity that is used to represent in-game objects
    /// </summary>
    public abstract class EntityBase : SimpleBehaviour, IStartBehaviour, IEnableBehaviour,
        IDisableBehaviour, IDestroyBehaviour
    {
        protected virtual void AssignComponents()
        {
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnEntitySetupComplete()
        {
        }

        protected virtual void OnEntityActivated()
        {
        }

        protected virtual void OnEntityDeactivated()
        {
        }

        protected virtual void OnTeardown()
        {
            
        }

#region Unity Lifecycle

        /// <summary>
        ///     Unity lifecycle methods are intentionally non-virtual (protected, not protected virtual).
        ///     Subclasses must use the virtual hooks (OnInitialized, OnEntitySetupComplete, etc.) instead.
        ///     Declaring Awake/Start/OnEnable/OnDisable in a subclass will produce a compiler warning
        ///     about hiding the base member, which is the intended safeguard.
        /// </summary>
        protected override void SetupAndValidateComponents()
        {
            AssignComponents();
        }

        protected override void Initialize()
        {
            OnInitialized();
        }

        protected override void OnBehaviourStarted()
        {
            OnEntitySetupComplete();
        }

        protected override void OnBehaviourEnabled()
        {
            OnEntityActivated();
        }

        protected override void OnBehaviourDisabled()
        {
            OnEntityDeactivated();
        }

        protected override void OnBehaviourDestroyed()
        {
            OnTeardown();
        }

#endregion
    }
}
