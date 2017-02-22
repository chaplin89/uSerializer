using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer
{
    public interface IChainManager
    {
        /// <summary>
        /// First element of the chain.
        /// </summary>
        IChainElement First { get; }

        /// <summary>
        /// Add an element to the chain.
        /// </summary>
        /// <param name="next">Element to add</param>
        /// <returns>The manager itself (for fluent syntax)</returns>
        IChainManager SetNext(IChainElement next);

        /// <summary>
        /// Process a request.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Response to the request</returns>
        IResponse Process(IRequest request);
    }
}