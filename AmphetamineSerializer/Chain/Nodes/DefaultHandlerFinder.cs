using AmphetamineSerializer.Backends;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmphetamineSerializer.Chain.Nodes
{
    public class DefaultHandlerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;

        Type[] defaultHelpers = new Type[]
        {
            typeof(StreamBackend),
            typeof(AssemblyFoundry)
        };

        public DefaultHandlerFinder()
        {
            managedRequests = new Dictionary<Type, RequestHandler>()
            {
                { typeof(ElementBuildRequest), HandleBuildRequest},
                { typeof(DelegateBuildRequest), HandleDelegateRequest }
            };
        }

        public string Name { get { return "DefaultHandlerFinder"; } }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequests; } }

        public IResponse HandleBuildRequest(IRequest genericRequest)
        {
            var request = genericRequest as ElementBuildRequest;

            var ctx = new Context(request.InputTypes,
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

                return method;
            }
            return null;
        }

        private IResponse HandleDelegateRequest(IRequest genericRequest)
        {
            var request = genericRequest as DelegateBuildRequest;

            var elementRequest = new ElementBuildRequest()
            {
                AdditionalContext = request.AdditionalContext,
                InputTypes = request.DelegateType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray()
            };

            var response = HandleBuildRequest(elementRequest) as ElementBuildResponse;

            return new DelegateBuildResponse()
            {
                Delegate = response.Method.CreateDelegate(request.DelegateType)
            };
        }
    }
}
