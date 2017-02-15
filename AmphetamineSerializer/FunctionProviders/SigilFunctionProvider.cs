using System;
using System.Collections.Generic;
using Sigil.NonGeneric;
using System.Reflection;

namespace AmphetamineSerializer
{
    public class SigilFunctionProvider
    {
        AssemblyName assemblyName;
        System.Reflection.Emit.AssemblyBuilder assemblyBuilder;
        System.Reflection.Emit.ModuleBuilder moduleBuilder;
        System.Reflection.Emit.TypeBuilder typeBuilder;

        public Dictionary<Type, MethodInfo> AlreadyBuildedMethods { get; set; }

        Stack<Emit> methods = new Stack<Emit>();
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;

        public SigilFunctionProvider(string assemblyName)
        {
            AlreadyBuildedMethods = new Dictionary<Type, MethodInfo>();
            this.assemblyName = new AssemblyName(assemblyName);
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave;
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, attributes,"D:\\");
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + ".dll");
            typeBuilder = moduleBuilder.DefineType(string.Format("{0}.Handler", this.assemblyName.Name), TypeAttributes.Public);
        }

        internal Emit AddMethod(string v, Type[] inputTypes, Type returnValue)
        {
            if (returnValue == null)
                returnValue = typeof(void);
            var localMethod = Emit.BuildStaticMethod(returnValue, inputTypes, typeBuilder, v, MethodAttributes.Public, true);
            methods.Push(localMethod);
            return localMethod;
        }

        internal MethodInfo GetMethod(bool isToPersist)
        {
            MethodInfo mi;
            if (!isToPersist)
            { 
                mi = methods.Pop().CreateMethod();
            }
            else
            {
                methods.Pop().CreateMethod();
                Type t = typeBuilder.CreateType();
                assemblyBuilder.Save(assemblyName.Name + ".dll");
                mi = t.GetMethods()[0];
            }

            AlreadyBuildedMethods.Add(mi.GetParameters()[0].ParameterType, mi);
            return mi;
        }
    }
}