using AmphetamineSerializer.Backends;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AmphetamineSerializer
{
    public class DefaultHandlerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequests;

        List<Type> backends = new List<Type>();

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

            foreach (var item in backends)
            {
                IBuilder instance = (IBuilder)Activator.CreateInstance(item, new object[] { ctx });
                var response = instance.Make();
                if (response == null)
                    continue;

                Debugger.Log(0, "Info", $"Request {ctx.CurrentElement.ToString()} handled by {response.ProcessedBy}");
                return response;
            }
            return null;
        }

        public DefaultHandlerFinder Use<T>()
            where T : BuilderBase
        {
            backends.Add(typeof(T));
            return this;
        }

        public static DefaultHandlerFinder WithDefaultBackends()
        {
            return new DefaultHandlerFinder()
                        .Use<BinaryStreamBackend>()
                        //.Use<ByteCountBackend>()
                        // .Use<ByteArrayBackend>()
                        .Use<AssemblyBuilder>();
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
