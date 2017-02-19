using System;
using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Interfaces;
using System.Collections.Generic;

namespace AmphetamineSerializer
{
    public class ChainManager : IChainManager
    {
        public IChainElement First { get; private set; }
        List<IChainElement> chain = new List<IChainElement>();

        public ChainManager()
        {
        }

        static public IChainManager MakeDefaultChain()
        {
            var manager = new ChainManager()
                       .SetNext(new CustomSerializerFinder())
                       .SetNext(new CustomBuilderFinder())
                       .SetNext(new DefaultHandlerFinder())
                       .SetNext(new CacheManager());
            return manager;
        }

        public IChainManager SetNext(IChainElement next)
        {
            chain.Add(next);
            if (First == null)
                First = next;

            return this;
        }

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

            throw new NotSupportedException("The chain is unable to process the request");
        }
    }
}
