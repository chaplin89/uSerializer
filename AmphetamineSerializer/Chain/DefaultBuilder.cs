using System;
using System.Collections.Generic;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System.Linq;

namespace AmphetamineSerializer
{
    public class DefaultBuilder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequestes;

        public DefaultBuilder()
        {
            managedRequestes = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleBuildRequest}
            };
        }

        public string Name { get { return "DefaultBuilder"; } }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequestes; } }

        public IResponse HandleBuildRequest(IRequest request)
        {
            var req = request as SerializationBuildRequest;
            FoundryContext ctx;
           if (req.AdditionalContext is FoundryContext)
           {
               ctx = req.AdditionalContext as FoundryContext;
               ctx.InputParameters = req.DelegateType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray();
           }
           else
            {
                ctx = FoundryContext.MakeContext(req.DelegateType, req.AdditionalContext);
            }

            var foundry = new AssemblyFoundry(ctx);
            return new SerializationBuildResponse()
            {
                Method = foundry.Method
            };
        }
    }
}
