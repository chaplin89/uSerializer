using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer.FunctionProviders
{
    /// <summary>
    /// Manage a function that is persisted on disk.
    /// </summary>
    public class PersistentFunctionProvider : IFunctionProvider
    {
        AssemblyName assemblyName;
        AssemblyBuilder assemblyBuilder;
        ModuleBuilder moduleBuilder;
        TypeBuilder typeBuilder;
        public Dictionary<Type, MethodInfo> AlreadyBuildedMethods { get; set; }
        Stack<MethodBuilder> methods = new Stack<MethodBuilder>();
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;

        public PersistentFunctionProvider(string assemblyName)
        {
            AlreadyBuildedMethods = new Dictionary<Type, MethodInfo>();
            this.assemblyName = new AssemblyName(assemblyName);
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, AssemblyBuilderAccess.RunAndSave, "D:\\");
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + ".dll");
            typeBuilder = moduleBuilder.DefineType(string.Format("{0}.Handler", this.assemblyName.Name), TypeAttributes.Public);
        }

        public ILGenerator AddMethod(string methodName, Type[] inputParameters, Type outputType)
        {
            var localMethod = typeBuilder.DefineMethod(methodName, attributes, outputType, inputParameters);
            methods.Push(localMethod);
            AlreadyBuildedMethods.Add(inputParameters[0], localMethod);
            return localMethod.GetILGenerator();
        }

        public MethodInfo GetMethod(bool save)
        {
            if (!save)
                return methods.Pop();

            Type t = typeBuilder.CreateType();
            assemblyBuilder.Save(assemblyName.Name + ".dll");
            return t.GetMethods()[0];
        }
    }
}
