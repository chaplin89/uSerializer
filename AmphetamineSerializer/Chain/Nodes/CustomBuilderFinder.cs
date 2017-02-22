using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace AmphetamineSerializer.Chain.Nodes
{
    /// <summary>
    /// Module that is capable of handling request for custom builders.
    /// </summary>
    public class CustomBuilderFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequestes;

        public CustomBuilderFinder()
        {
            managedRequestes = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleSerializationBuild}
            };
        }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequestes; } }

        public IResponse HandleSerializationBuild(IRequest request)
        {
            var localRequest = request as SerializationBuildRequest;
            Type normalizedType;
            if (localRequest.RootType.IsByRef)
                normalizedType = localRequest.RootType.GetElementType();
            else
                normalizedType = localRequest.RootType;

            var attribute = normalizedType.GetCustomAttribute<BuildedWithAttribute>(false);

            if (attribute == null)
                return null;

            IBuilder builder = Activator.CreateInstance(attribute.BuilderType, new object[] { localRequest }) as IBuilder;

            return new SerializationBuildResponse()
            {
                Response = builder.Make(),
                Instance = null
            };
        }

        public string Name { get { return "CustomBuilderFinder"; } }
    }
}
