using AmphetamineSerializer.Common;
using AmphetamineSerializer.Helpers;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AmphetamineSerializer.Chain
{
    public class DefaultHandlerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;
        FuzzyFunctionResolver resolver;

        Type[] defaultHelpers = new Type[]
        {
            typeof(ByteArrayDeserialization),
            typeof(ByteArraySerialization),
            typeof(StreamDeserializationCtx),
        };

        public DefaultHandlerFinder()
        {
            resolver = new FuzzyFunctionResolver(defaultHelpers);

            managedRequests = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleBuildRequest}
            };
        }

        public string Name { get { return "DefaultHandlerFinder"; } }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequests; } }

        public IResponse HandleBuildRequest(IRequest request)
        {
            var req = request as SerializationBuildRequest;
            var foundMethod = resolver.ResolveFromSignature(req.RootType, req.InputTypes, req.OutputType);
            if (foundMethod == null)
                return null;

            if (foundMethod.GetParameters()[0].ParameterType == req.AdditionalContext.GetType())
            {
                foundMethod = (MethodInfo)foundMethod.Invoke(null, new object[] { req.AdditionalContext });

                return new SerializationBuildResponse()
                {
                    DynMethod = foundMethod
                };
            }

            return new SerializationBuildResponse()
            {
                Method = foundMethod
            };
        }
    }
}
