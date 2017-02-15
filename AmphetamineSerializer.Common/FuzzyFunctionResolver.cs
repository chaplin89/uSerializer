using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    public class FuzzyFunctionResolver
    {
        Type[] registeredTypes;
        Dictionary<Type, List<MethodInfo>> handlers = new Dictionary<Type, List<MethodInfo>>();

        public FuzzyFunctionResolver(Type[] typeToRegister = null)
        {
            if (typeToRegister == null)
                return;

            registeredTypes = typeToRegister;
            foreach (var v in typeToRegister)
                Register(v);
        }

        public MethodInfo ResolveFromSignature(Type handlerType, Type[] inputType, Type returnType)
        {
            if (handlerType == null || inputType == null)
                throw new ArgumentNullException("inputTypes");
            if (returnType == null)
                returnType = typeof(void);

            List<MethodInfo> currentList;
            MethodInfo candidateMethod = null;
            int matchingParameters = 0;

            if (!handlers.TryGetValue(handlerType, out currentList))
                return null;

            foreach (var item in currentList)
            {
                bool[] matching = new bool[inputType.Length + 1];
                matching[inputType.Length] = (returnType == item.ReturnType);

                foreach (var handlerParameter in item.GetParameters())
                {
                    for (int i = 0; i < inputType.Length; i++)
                    {
                        if (inputType[i] == handlerParameter.ParameterType)
                        {
                            if (matching[i])
                                continue;
                            matching[i] = true;
                            break;
                        }
                    }
                }
                int currentMatchingParameters = matching.Count(x => x);
                if (currentMatchingParameters > matchingParameters)
                {
                    matchingParameters = currentMatchingParameters;
                    candidateMethod = item;
                }
            }

            return candidateMethod;
        }

        public object ResolveFromSignature(object rootType, object inputTypes, object outputType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Map types with the corresponding method that is decorated using the attribute SerializationHandler.
        /// </summary>
        /// <returns>Dictionary that map a type with a method that handle that type.</returns>
        public void Register(Type t)
        {
            foreach (var method in t.GetMethods(BindingFlags.Static | BindingFlags.Instance|  BindingFlags.Public | BindingFlags.InvokeMethod))
            {
                if (method.IsSpecialName)
                    continue;
                if (method.GetParameters().Length == 0)
                    continue;

                var currentParameter = method.GetParameters().First().ParameterType;
                List<MethodInfo> currentList;
                if (!handlers.TryGetValue(currentParameter, out currentList))
                {
                    currentList = new List<MethodInfo>();
                    handlers.Add(currentParameter, currentList);
                }

                if (!currentList.Contains(method))
                    currentList.Add(method);
            }
        }
    }
}