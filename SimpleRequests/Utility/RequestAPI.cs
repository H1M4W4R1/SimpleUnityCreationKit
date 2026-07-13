using Systems.SimpleRequests.Abstract;

namespace Systems.SimpleRequests.Utility
{
    /// <summary>
    ///     Provides a type-safe entry point for registering, sending, and clearing requests.
    /// </summary>
    public static class RequestAPI
    {
        /// <summary>
        ///     Sends a request to every handler registered for its context type.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <param name="data">The request data to send.</param>
        public static void Send<TRequestContext>(in TRequestContext data)
            where TRequestContext : struct, IRequestContext
        {
            Request<TRequestContext>.Send(in data);
        }

        /// <summary>
        ///     Registers a handler for a request context type.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <param name="handler">The handler to register.</param>
        /// <param name="priority">The execution priority. Higher values execute before lower values.</param>
        public static void RegisterHandler<TRequestContext>(
            Request<TRequestContext>.RequestHandler handler, int priority = 0)
            where TRequestContext : struct, IRequestContext
        {
            Request<TRequestContext>.RegisterHandler(handler, priority);
        }

        /// <summary>
        ///     Removes a handler for a request context type.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        public static void UnregisterHandler<TRequestContext>(Request<TRequestContext>.RequestHandler handler)
            where TRequestContext : struct, IRequestContext
        {
            Request<TRequestContext>.UnregisterHandler(handler);
        }

        /// <summary>
        ///     Removes every handler registered for a request context type.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        public static void ClearHandlers<TRequestContext>()
            where TRequestContext : struct, IRequestContext
        {
            Request<TRequestContext>.ClearHandlers();
        }

        /// <summary>
        ///     Sends a request to every handler registered for its request and response type pair.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <typeparam name="TResponseType">The value type returned after handlers update the response.</typeparam>
        /// <param name="data">The request data to send.</param>
        /// <returns>The response after every registered handler has run.</returns>
        public static TResponseType Send<TRequestContext, TResponseType>(in TRequestContext data)
            where TRequestContext : struct, IRequestContext
            where TResponseType : struct
        {
            TResponseType result = default(TResponseType);
            Request<TRequestContext, TResponseType>.Send(in data, ref result);
            return result;
        }

        /// <summary>
        ///     Registers a handler for a request and response type pair.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <typeparam name="TResponseType">The value type that handlers update with their response.</typeparam>
        /// <param name="handler">The handler to register.</param>
        /// <param name="priority">The execution priority. Higher values execute before lower values.</param>
        public static void RegisterHandler<TRequestContext, TResponseType>(
            Request<TRequestContext, TResponseType>.RequestHandler handler, int priority = 0)
            where TRequestContext : struct, IRequestContext
            where TResponseType : struct
        {
            Request<TRequestContext, TResponseType>.RegisterHandler(handler, priority);
        }

        /// <summary>
        ///     Removes a handler for a request and response type pair.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <typeparam name="TResponseType">The value type that handlers update with their response.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        public static void UnregisterHandler<TRequestContext, TResponseType>(
            Request<TRequestContext, TResponseType>.RequestHandler handler)
            where TRequestContext : struct, IRequestContext
            where TResponseType : struct
        {
            Request<TRequestContext, TResponseType>.UnregisterHandler(handler);
        }

        /// <summary>
        ///     Removes every handler registered for a request and response type pair.
        /// </summary>
        /// <typeparam name="TRequestContext">The value type that carries data for this request.</typeparam>
        /// <typeparam name="TResponseType">The value type that handlers update with their response.</typeparam>
        public static void ClearHandlers<TRequestContext, TResponseType>()
            where TRequestContext : struct, IRequestContext
            where TResponseType : struct
        {
            Request<TRequestContext, TResponseType>.ClearHandlers();
        }
    }
}
