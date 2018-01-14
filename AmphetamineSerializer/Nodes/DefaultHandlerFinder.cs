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
        List<Type> backends = new List<Type>();

        public string Name { get { return "DefaultHandlerFinder"; } }
        
        public IResponse HandleElementRequest(IRequest genericRequest)
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

            var response = HandleElementRequest(elementRequest) ;

            var fin = response as TypeFinalizedBuildResponse;

            return new DelegateBuildResponse()
            {
                Delegate = fin.Method.CreateDelegate(request.DelegateType)
            };
        }

        public IResponse RequestHandler(IRequest request)
        {
            if (request is DelegateBuildRequest)
                return HandleDelegateRequest(request);
            else
                return HandleElementRequest(request);
        }
    }
}
