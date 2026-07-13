using JetBrains.Annotations;

namespace Systems.SimpleUI.Context.Abstract
{
    /// <summary>
    ///     Interface for context providers
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        ///     Tries to provide the context to the object
        /// </summary>
        /// <param name="context">Found context</param>
        /// <typeparam name="TContextType">Desired context type</typeparam>
        /// <returns>True if the context was provided</returns>
        public bool TryProvideContext<TContextType>([CanBeNull] out TContextType context);
        
        /// <summary>
        ///     Checks if the context can be provided
        /// </summary>
        /// <typeparam name="TContextType">Desired context type</typeparam>
        /// <returns>True if the context can be provided</returns>
        public bool CanProvideContext<TContextType>();
    }
}