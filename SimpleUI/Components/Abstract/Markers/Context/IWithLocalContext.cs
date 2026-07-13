using JetBrains.Annotations;

namespace Systems.SimpleUI.Components.Abstract.Markers.Context
{
    /// <summary>
    ///     Represents an object that has a local context
    /// </summary>
    /// <typeparam name="TContextType">Context type</typeparam>
    /// <remarks>
    ///     Usage of local context overrides search for context providers, so use with caution. Moreover, it's not
    ///     recommended to use local context at all as it breaks model-view separation, but in some cases it
    ///     significantly simplifies code - a good example is build version display text.
    /// </remarks>
    public interface IWithLocalContext<TContextType> : IWithContext<TContextType>
    {
        /// <summary>
        ///     Gets the context of the object
        /// </summary>
        /// <returns>Context of the object or null if no context is set</returns>
        public bool TryGetContext([CanBeNull] out TContextType context);
        
    }
}