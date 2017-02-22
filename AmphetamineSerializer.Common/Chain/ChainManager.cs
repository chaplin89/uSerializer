using System;
using AmphetamineSerializer.Interfaces;
using System.Collections.Generic;

namespace AmphetamineSerializer.Chain
{
    /// <summary>
    /// Simple implementation of a chain-of-responsibility.
    /// </summary>
    public class ChainManager : IChainManager
    {
        List<IChainElement> chain = new List<IChainElement>();

        /// <summary>
        /// Add an element to the chain.
        /// </summary>
        /// <param name="next">Element to add</param>
        /// <returns>This chain manager (for fluent syntax)</returns>
        public IChainManager SetNext(IChainElement next)
        {
            chain.Add(next);
            if (First == null)
                First = next;

            return this;
        }

        /// <summary>
        /// First element of the chain.
        /// </summary>
        public IChainElement First { get; private set; }

        /// <summary>
        /// Process a request.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response to the request</returns>
        /// <exception cref="NotSupportedException">If there are no node availiable to handle the request</exception>
        public IResponse Process(IRequest request)
        {
            IResponse response = null;

            if (request == null)
                throw new ArgumentNullException("request");

            foreach (var node in chain)
            {
                RequestHandler handler;
                node.ManagedRequestes.TryGetValue(request.GetType(), out handler);

                if (handler == null)
                    continue;

                response = handler(request);

                if (response == null)
                    continue;

                response.ProcessedBy = node.Name;
                return response;
            }

            throw new NotSupportedException("The chain is unable to process the request.");
        }
    }
}
