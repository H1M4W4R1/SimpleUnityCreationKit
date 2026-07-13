using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Systems.SimpleCore.Identifiers;
using Systems.SimpleUI.Components.Abstract.Markers;

namespace Systems.SimpleUI.Utility.Internal
{
    /// <summary>
    ///     Overcomplicated method of rendering interface objects, but works so...
    ///     it should be fine... I suppose.
    /// </summary>
    public static class UserInterfaceRenderHelper
    {
        /// <summary>
        ///     Delegate that invokes rendering on object
        /// </summary>
        private delegate void RenderDelegate([NotNull] IRenderable instance);

        /// <summary>
        ///     Dictionary that caches render delegates
        /// </summary>
        private static readonly ConcurrentDictionary<HashIdentifier, RenderDelegate> _cache = new();

        /// <summary>
        ///     Invoke rendering on object
        /// </summary>
        /// <param name="instance">IRenderable instance</param>
        public static void Invoke([NotNull] IRenderable instance)
        {
            // Get type, should be always the topmost, most likely also sealed implementation
            // so we can access all interfaces
            Type instanceType = instance.GetType();
            
            // Convert to hash for caching, maybe useless but keep for now
            HashIdentifier hashIdentifier = HashIdentifier.New(instanceType);
            
            // Search for invoker and execute it
            RenderDelegate invoker = _cache.GetOrAdd(hashIdentifier, BuildInvoker(instanceType));
            invoker.Invoke(instance);
        }

        /// <summary>
        ///     Build invoker for type
        /// </summary>
        /// <param name="type">Type to build invoker for</param>
        /// <returns>Method group to invoke rendering</returns>
        [NotNull] private static RenderDelegate BuildInvoker([NotNull] Type type)
        {
            List<RenderDelegate> renderables = new();

            // Find all implemented IRenderable<T>
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type interfaceType = interfaces[i];
                if (!interfaceType.IsGenericType) continue;
                if (interfaceType.GetGenericTypeDefinition() != typeof(IRenderable<>)) continue;

                renderables.Add(BuildSingleInvoker(interfaceType));
            }

            return InvokeAllRenderables;

            // Method to return
            void InvokeAllRenderables([NotNull] IRenderable instance)
            {
                // Invoke all renderables
                for (int index = 0; index < renderables.Count; index++)
                {
                    RenderDelegate renderable = renderables[index];
                    renderable(instance);
                }
            }
        }
        
        /// <summary>
        ///     Build invoker for single <see cref="IRenderable{TContextType}"/>
        /// </summary>
        /// <param name="interfaceType">Type of <see cref="IRenderable{TContextType}"/></param>
        /// <returns>Method group to execute rendering</returns>
        [NotNull] private static RenderDelegate BuildSingleInvoker([NotNull] Type interfaceType)
        {
            // Context type can be anything as method will be always of same name
            MethodInfo renderMethod = interfaceType.GetMethod(nameof(IRenderable<object>.RenderSelf),
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            return obj => renderMethod.Invoke(obj, null);
        }
    }
}