using JetBrains.Annotations;
using Systems.SimpleUI.Components.Abstract.Markers.Context;
using Systems.SimpleUI.Utility.Internal;

namespace Systems.SimpleUI.Components.Abstract.Markers
{
    /// <summary>
    ///     Informs object that it should be rendered with a specific context type
    /// </summary>
    /// <typeparam name="TContextType">Context type</typeparam>
    public interface IRenderable<TContextType> : IRenderable, IWithContext<TContextType>
    {
        /// <summary>
        ///     Event that is called when the object is rendered
        /// </summary>
        void OnRender([CanBeNull] TContextType withContext);

        protected internal void RenderSelf()
        {
            if (!TryProvideContext(out TContextType context)) return;
            OnRender(context);
        } 
    }
    
    /// <summary>
    ///     Informs object that it should be rendered.
    ///     Do not use directly, see <see cref="IRenderable{TContextType}"/>
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        ///     Renders the object
        /// </summary>
        protected internal void Render()
        {
            // Invoke rendering in a bit overcomplicated manner to handle
            // everything as intended...
            UserInterfaceRenderHelper.Invoke(this);
        }
    }
}