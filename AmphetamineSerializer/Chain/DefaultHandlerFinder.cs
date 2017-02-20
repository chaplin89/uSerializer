using AmphetamineSerializer.Common;
using AmphetamineSerializer.Helpers;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AmphetamineSerializer.Chain
{
    public class DefaultHandlerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;

        Type[] defaultHelpers = new Type[]
        {
            // typeof(ByteArrayDeserialization),
            // typeof(ByteArraySerialization),
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
                using (new StatusSaver(ctx))
                {
                    IBuilder instance = (IBuilder)Activator.CreateInstance(item, new object[] { ctx });
                    if (instance.Method == null)
                        continue;

                    return new SerializationBuildResponse()
                    {
                        Method = instance.Method
                    };
                }
            }
            return null;
        }
    }
}
