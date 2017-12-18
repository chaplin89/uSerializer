using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;

namespace AmphetamineSerializer.Chain.Nodes
{
    /// <summary>
    /// Manage a general-purpouse cache for already builded serializators.
    /// </summary>
    public class CacheManager : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;

        public CacheManager()
        {
            managedRequests = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleSerializationBuild }
            };
        }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequests; } }

        public string Name { get { return "CacheManager"; } }

        public IResponse HandleSerializationBuild(IRequest request)
        {
            return null;
        }
    }
}
