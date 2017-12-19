using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AmphetamineSerializer.Chain.Nodes
{
    public class CustomSerializerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequestes;

        public CustomSerializerFinder()
        {
            managedRequestes = new Dictionary<Type, RequestHandler>()
            {
                { typeof(ElementBuildRequest), HandleBuildRequest }
            };
        }

        public Dictionary<Type, RequestHandler> ManagedRequestes { get { return managedRequestes; } }

        public string Name { get { return "CustomSerializerFinder"; } }

        /// <summary>
        /// If the type is managed by a custom serializator, retrieve it 
        /// and create a delegate.
        /// </summary>
        /// <param name="rootType"></param>
        /// <param name="additionalContext"></param>
        /// <param name="delegateType"></param>
        /// <returns></returns>
        public IResponse HandleBuildRequest(IRequest request)
        {
            var localRequest = request as ElementBuildRequest;
            Type rootType = localRequest.InputTypes.First();
            SerializedWithAttribute attribute;
            if (rootType.IsByRef)
                attribute = rootType.GetCustomAttribute<SerializedWithAttribute>(false);
            else
                attribute = rootType.GetCustomAttribute<SerializedWithAttribute>(false);

            var resolver = new FuzzyFunctionResolver();

            if (attribute == null)
                return null;

            var instance = Activator.CreateInstance(attribute.SerializatorType, localRequest.AdditionalContext);
            resolver.Register(attribute.SerializatorType);

            var method = new BuildedFunction()
            {
                Method = resolver.ResolveFromSignature(rootType, localRequest.InputTypes, typeof(void)),
                Status = BuildedFunctionStatus.TypeFinalized
            };

            if (method.Method == null)
                throw new InvalidOperationException("Unable to retrieve the method from the custom deserializator.");
            
            return new SerializationBuildResponse()
            {
                Function = method,
                Instance = instance
            };
        }
    }
}
