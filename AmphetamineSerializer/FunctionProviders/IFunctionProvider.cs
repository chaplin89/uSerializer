using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer.FunctionProviders
{
    public interface IFunctionProvider
    {
        MethodInfo GetMethod(bool save);
        ILGenerator AddMethod(string functionName, Type[] inputParameters, Type outputType);
        Dictionary<Type, MethodInfo> AlreadyBuildedMethods { get; set; }
    }
}
