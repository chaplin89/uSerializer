using System;
using System.Collections.Generic;

namespace AmphetamineSerializer.Interfaces
{
    public delegate IResponse RequestHandler(IRequest request);

    /// <summary>
    /// Interface for implementing the chain-of-responsibility.
    /// </summary>
    public interface IChainElement
    {
        /// <summary>
        /// 
        /// </summary>
        IResponse RequestHandler(IRequest request);

        /// <summary>
        /// 
        /// </summary>
        string Name { get; }
    }
}
