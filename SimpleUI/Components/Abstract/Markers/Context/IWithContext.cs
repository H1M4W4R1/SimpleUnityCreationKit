using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleUI.Context.Abstract;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.SimpleUI.Components.Abstract.Markers.Context
{
    /// <summary>
    ///     Represents an object that has a context
    /// </summary>
    /// <typeparam name="TContextType">Context type</typeparam>
    public interface IWithContext<TContextType> : IWithContext
    {
        /// <summary>
        ///     Provides the context of the object
        /// </summary>
        /// <returns>The context of the object or null if context is not available</returns>
        protected internal bool TryProvideContext([CanBeNull] out TContextType context)
        {
            // Handle local context override against context providers
            if (this is IWithLocalContext<TContextType> localContext)
                return localContext.TryGetContext(out context);
            
            // Search for context provider
            IContextProvider provider = GetContextProviderFor<TContextType>();
            
            // Provide context if provider found, otherwise return default
            if (provider != null) return provider.TryProvideContext(out context);
            context = default;
            return false;
        }

        /// <summary>
        ///     Called when the context changes - marks the object as dirty
        /// </summary>
        internal void OnContextChanged()
        {
            SetDirty();
        }
    }

    /// <summary>
    ///     Marker interface for objects that have a context
    ///     Do not use directly, see <see cref="IWithContext{TContextType}"/>
    /// </summary>
    public interface IWithContext
    {
        /// <summary>
        ///     List of all available context providers for this context object
        /// </summary>
        protected List<IContextProvider> AvailableContextProviders { get; }

        /// <summary>
        ///     Gets the context provider for the specified context type
        /// </summary>
        /// <typeparam name="TContextType">Type of the context</typeparam>
        /// <returns>The context provider for the specified context type or null if not found</returns>
        [CanBeNull] internal IContextProvider GetContextProviderFor<TContextType>()
        {
            // Acquire unity object and validate if correct
            Component thisComponent = this as Component;
            Assert.IsNotNull(thisComponent, "Object with IWithContext must be a " +
                                            "Unity Component (for example MonoBehaviour)");

            // Scan list of available context providers to find suitable one
            for (int providerIndex = 0; providerIndex < AvailableContextProviders.Count; providerIndex++)
            {
                if (!AvailableContextProviders[providerIndex].CanProvideContext<TContextType>()) continue;
                return AvailableContextProviders[providerIndex];
            }

            // No suitable context provider found, clear the list and
            // re-search for new context providers
            AvailableContextProviders.Clear();

            // Get all context providers and acquire first that can provide context
            IContextProvider[] contextProviders = thisComponent.GetComponentsInParent<IContextProvider>();
            AvailableContextProviders.AddRange(contextProviders);

            // Scan list of available context providers to find suitable one
            for (int providerIndex = 0; providerIndex < AvailableContextProviders.Count; providerIndex++)
            {
                if (!AvailableContextProviders[providerIndex].CanProvideContext<TContextType>()) continue;
                return AvailableContextProviders[providerIndex];
            }

            // No suitable context provider found
            Debug.LogError("No suitable context provider found for " + typeof(TContextType).Name);
            return null;
        }

        /// <summary>
        ///     Changes the dirty status of the object
        /// </summary>
        public bool SetDirty(bool isNowDirty = true) => IsDirty = isNowDirty;

        /// <summary>
        ///     Indicates if the context is dirty, should be triggered
        ///     each time context changed (recommended to use events if provided)
        /// </summary>
        protected internal bool IsDirty { get; set; }

        /// <summary>
        ///     Acquires the context of the object
        /// </summary>
        /// <typeparam name="TContextType">Context type</typeparam>
        /// <returns>The context of the object or default if context is not available / supported</returns>
        [CanBeNull] public TContextType ProvideContextFor<TContextType>()
        {
            if (this is not IWithContext<TContextType> context) return default;
            if (context.TryProvideContext(out TContextType providedContext)) return providedContext;
            return default;
        }

        /// <summary>
        ///     Notification received whenever context provider is destroyed to remove it from list
        ///     and prevent destroyed object accessing which Unity does not like
        /// </summary>
        internal void NotifyContextProviderDestroyed([NotNull] IContextProvider provider)
        {
            AvailableContextProviders.Remove(provider);
        }

        /// <summary>
        ///     Used to update the dirty status of the object if context has changed
        ///     does nothing if returns false.
        /// </summary>
        public void ValidateContext();
    }
}