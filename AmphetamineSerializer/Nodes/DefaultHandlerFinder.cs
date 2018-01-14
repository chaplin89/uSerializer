using AmphetamineSerializer.Backends;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmphetamineSerializer
{
    public class DefaultHandlerFinder : IChainElement
    {
        List<Type> backends = new List<Type>();

        public string Name { get { return "DefaultHandlerFinder"; } }

        public DefaultHandlerFinder Use<T>()
            where T : BuilderBase
        {
            backends.Add(typeof(T));
            return this;
        }

        public static DefaultHandlerFinder StreamTemplate()
        {
            return new DefaultHandlerFinder()
                        .Use<BinaryStreamBackend>();
        }

        private IResponse HandleDelegateRequest(IRequest genericRequest)
        {
            var request = genericRequest as DelegateBuildRequest;

            var elementRequest = new ElementBuildRequest()
            {
                AdditionalContext = request.AdditionalContext,
                InputTypes = request.DelegateType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray()
            };

            var response = HandleElementRequest(elementRequest);

            var fin = response as TypeFinalizedBuildResponse;

            return new DelegateBuildResponse()
            {
                Delegate = fin.Method.CreateDelegate(request.DelegateType)
            };
        }

        private IResponse HandleElementRequest(IRequest genericRequest)
        {
            var request = genericRequest as ElementBuildRequest;
            var backends = GetMatchingBackends(request.InputTypes);

            var ctx = new Context(request.InputTypes,
                                  request.AdditionalContext,
                                  request.Element,
                                  request.Provider,
                                  request.G,
                                  backends);

            var instance = new AssemblyBuilder(ctx);
            var response = instance.Make();
            return response;
        }

        private Type[] GetMatchingBackends(Type[] inputTypes)
        {
            return new Type[] { typeof(BinaryStreamBackend) };
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
