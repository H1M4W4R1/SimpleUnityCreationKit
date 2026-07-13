using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.SimpleRequests.Abstract;

namespace Systems.SimpleRequests.Utility
{
    /// <summary>
    ///     Stores handlers for one request and response type pair and sends requests to those handlers.
    /// </summary>
    /// <typeparam name="TRequestType">The value type that carries data for this request.</typeparam>
    /// <typeparam name="TResponseType">The value type that handlers update with their response.</typeparam>
    public static class Request<TRequestType, TResponseType>
        where TRequestType : struct, IRequestContext
        where TResponseType : struct
    {
        /// <summary>
        ///     Represents a method that handles a request and can update its response.
        /// </summary>
        /// <param name="data">The data supplied by the request sender.</param>
        /// <param name="response">The response value shared by every handler for this send.</param>
        public delegate void RequestHandler(in TRequestType data, ref TResponseType response);

        private static readonly List<HandlerRegistration> _registeredHandlers = new List<HandlerRegistration>();

        /// <summary>
        ///     Registers a handler when it is not already registered.
        /// </summary>
        /// <param name="handler">The handler to register.</param>
        /// <param name="priority">The execution priority. Higher values execute before lower values.</param>
        public static void RegisterHandler([NotNull] RequestHandler handler, int priority = 0)
        {
            if (ReferenceEquals(handler, null)) return;
            if (ContainsHandler(handler)) return;

            HandlerRegistration registration = new HandlerRegistration(handler, priority);
            int insertionIndex = GetInsertionIndex(priority);
            _registeredHandlers.Insert(insertionIndex, registration);
        }

        /// <summary>
        ///     Removes a previously registered handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public static void UnregisterHandler([NotNull] RequestHandler handler)
        {
            if (ReferenceEquals(handler, null)) return;
            for (int n = 0; n < _registeredHandlers.Count; n++)
            {
                if (_registeredHandlers[n].Handler != handler) continue;

                _registeredHandlers.RemoveAt(n);
                return;
            }
        }

        /// <summary>
        ///     Removes every registered handler for this request and response type pair.
        /// </summary>
        public static void ClearHandlers()
        {
            _registeredHandlers.Clear();
        }

        /// <summary>
        ///     Sends a request to all registered handlers, starting with the highest-priority handler.
        ///     Handlers with equal priority run in most-recent-registration-first order.
        /// </summary>
        /// <param name="request">The request data to send.</param>
        /// <param name="response">The response value that handlers can update.</param>
        public static void Send(in TRequestType request, ref TResponseType response)
        {
            for (int n = _registeredHandlers.Count - 1; n >= 0; n--)
            {
                _registeredHandlers[n].Handler(in request, ref response);
            }
        }

        private static bool ContainsHandler(RequestHandler handler)
        {
            for (int n = 0; n < _registeredHandlers.Count; n++)
            {
                if (_registeredHandlers[n].Handler == handler) return true;
            }

            return false;
        }

        private static int GetInsertionIndex(int priority)
        {
            int lowerBound = 0;
            int upperBound = _registeredHandlers.Count;

            while (lowerBound < upperBound)
            {
                int middleIndex = lowerBound + (upperBound - lowerBound) / 2;
                if (_registeredHandlers[middleIndex].Priority <= priority)
                    lowerBound = middleIndex + 1;
                else
                    upperBound = middleIndex;
            }

            return lowerBound;
        }

        private readonly struct HandlerRegistration
        {
            public readonly RequestHandler Handler;
            public readonly int Priority;

            public HandlerRegistration(RequestHandler handler, int priority)
            {
                Handler = handler;
                Priority = priority;
            }
        }
    }
}
