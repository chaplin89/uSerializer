using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer.FunctionProviders
{
    /// <summary>
    /// Manage a function that is not stored on disk.
    /// </summary>
    public class DynamicFunctionProvider : IFunctionProvider
    {
        Stack<DynamicMethod> dynmethod = new Stack<DynamicMethod>();
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;

        public Dictionary<Type, MethodInfo> AlreadyBuildedMethods { get; set; }

        public DynamicFunctionProvider(string assemblyName)
        {
            AlreadyBuildedMethods = new Dictionary<Type, MethodInfo>();
        }

        public ILGenerator AddMethod(string methodName, Type[] inputParameters, Type outputType)
        {
            CallingConventions callingConvention = CallingConventions.Standard;
            var localDynMethod = new DynamicMethod(methodName, attributes, callingConvention, outputType, inputParameters, GetType().Module, false);
            dynmethod.Push(localDynMethod);
            AlreadyBuildedMethods.Add(inputParameters[0], localDynMethod);
            return localDynMethod.GetILGenerator();
        }

        public MethodInfo GetMethod(bool save)
        {
            return dynmethod.Pop();
        }
    }
}
