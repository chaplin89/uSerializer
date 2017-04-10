using AmphetamineSerializer.Common;
using AmphetamineSerializer.Helpers;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;

namespace AmphetamineSerializer.Chain.Nodes
{
    public class DefaultHandlerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;

        Type[] defaultHelpers = new Type[]
        {
            typeof(StreamDeserializationCtx),
            typeof(AssemblyFoundry)
        };

        public DefaultHandlerFinder()
        {
            managedRequests = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleBuildRequest}
            };
        }

        public string Name { get { return "DefaultHandlerFinder"; } }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequests; } }

        public IResponse HandleBuildRequest(IRequest genericRequest)
        {
            var request = genericRequest as SerializationBuildRequest;

            FoundryContext ctx = FoundryContext.MakeContext(request.DelegateType,
                                                            request.AdditionalContext,
                                                            request.Element,
                                                            request.Provider,
                                                            request.G);

            foreach (var item in defaultHelpers)
            {
                IBuilder instance = (IBuilder)Activator.CreateInstance(item, new object[] { ctx });
                var method = instance.Make();
                if (method == null)
                    continue;

                if (request.RequestType == TypeOfRequest.Delegate)
                {
                    method.Delegate = method.Method.CreateDelegate(request.DelegateType);
                    method.Method = null;
                }

                return new SerializationBuildResponse()
                {
                    Response = method
                };
            }
            return null;
        }
    }
}
