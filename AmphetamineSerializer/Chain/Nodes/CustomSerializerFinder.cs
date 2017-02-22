using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AmphetamineSerializer.Chain.Nodes
{
    public class CustomSerializerFinder : IChainElement
    {
        readonly Dictionary<Type, RequestHandler> managedRequestes;

        public CustomSerializerFinder()
        {
            managedRequestes = new Dictionary<Type, RequestHandler>()
            {
                { typeof(SerializationBuildRequest), HandleBuildRequest }
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
            var localRequest = request as SerializationBuildRequest;
            SerializedWithAttribute attribute;
            if (localRequest.RootType.IsByRef)
                attribute = localRequest.RootType.GetElementType().GetCustomAttribute<SerializedWithAttribute>(false);
            else
                attribute = localRequest.RootType.GetCustomAttribute<SerializedWithAttribute>(false);

            var resolver = new FuzzyFunctionResolver();
            var dlgMi = localRequest.DelegateType.GetMethod("Invoke");

            if (attribute == null)
                return null;

            var instance = Activator.CreateInstance(attribute.SerializatorType, localRequest.AdditionalContext);
            resolver.Register(attribute.SerializatorType);
            var inputType = dlgMi.GetParameters().Select(x => x.ParameterType).ToArray();

            var method = new BuildedFunction()
            {
                Method = resolver.ResolveFromSignature(localRequest.RootType, inputType, dlgMi.ReturnType),
                Status = BuildedFunctionStatus.TypeFinalized
            };

            if (method.Method == null)
                throw new InvalidOperationException("Unable to retrieve the method from the custom deserializator.");
            
            return new SerializationBuildResponse()
            {
                ResponseType = TypeOfRequest.Method,
                Response = method,
                Instance = instance
            };
        }
    }
}
