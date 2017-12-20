using AmphetamineSerializer.Interfaces;
using System;
using System.Reflection;
using System.Collections.Generic;
using AmphetamineSerializer.Model.Attributes;
using System.Linq;

namespace AmphetamineSerializer.Chain.Nodes
{
    /// <summary>
    /// Manage the request for object that require custom building 
    /// logic through attribute specification.
    /// </summary>
    public class CustomBuilderFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequestes;

        public CustomBuilderFinder()
        {
            managedRequestes = new Dictionary<Type, RequestHandler>()
            {
                { typeof(DelegateBuildRequest), HandleDelegateBuild}
            };
        }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequestes; } }

        public IResponse HandleDelegateBuild(IRequest request)
        {
            var localRequest = request as DelegateBuildRequest;
            var inputTypes = localRequest.DelegateType.GetMethod("Invoke").GetParameters().Select(_ => _.ParameterType).ToArray();

            Type normalizedType;
            if (inputTypes.First().IsByRef)
                normalizedType = inputTypes.First().GetElementType();
            else
                normalizedType = inputTypes.First();

            var attribute = normalizedType.GetCustomAttribute<BuildedWithAttribute>(false);

            if (attribute == null)
                return null;

            IBuilder builder = Activator.CreateInstance(attribute.BuilderType, new object[] { localRequest }) as IBuilder;

            return builder.Make();
        }

        public string Name { get { return "CustomBuilderFinder"; } }
    }
}
