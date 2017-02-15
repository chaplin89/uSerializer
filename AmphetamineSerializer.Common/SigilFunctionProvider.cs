using System;
using System.Collections.Generic;
using Sigil.NonGeneric;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    public class SigilFunctionProvider
    {
        AssemblyName assemblyName;
        System.Reflection.Emit.AssemblyBuilder assemblyBuilder;
        System.Reflection.Emit.ModuleBuilder moduleBuilder;
        System.Reflection.Emit.TypeBuilder typeBuilder;

        public Dictionary<Type, Emit> AlreadyBuildedMethods { get; set; }

        Stack<Emit> methods = new Stack<Emit>();
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;

        public SigilFunctionProvider(string assemblyName)
        {
            AlreadyBuildedMethods = new Dictionary<Type, Emit>();
            this.assemblyName = new AssemblyName(assemblyName);
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave;
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, attributes,"D:\\");
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + ".dll");
            typeBuilder = moduleBuilder.DefineType(string.Format("{0}.Handler", this.assemblyName.Name), TypeAttributes.Public);
        }
        Type lastType;
        public Emit AddMethod(string v, Type[] inputTypes, Type returnValue)
        {
            if (returnValue == null)
                returnValue = typeof(void);
            var localMethod = Emit.BuildStaticMethod(returnValue, inputTypes, typeBuilder, v, MethodAttributes.Public, true);
            methods.Push(localMethod);
            lastType = inputTypes[0];
            return localMethod;
        }

        public MethodInfo GetMethod(bool isToPersist)
        {
            MethodInfo mi;
            Emit emit;
            if (!isToPersist)
            {
                emit = methods.Pop();
                mi = emit.CreateMethod();
            }
            else
            {
                emit = methods.Pop();
                mi = emit.CreateMethod();
                Type t = typeBuilder.CreateType();
                assemblyBuilder.Save(assemblyName.Name + ".dll");
            }

            AlreadyBuildedMethods.Add(lastType, emit);
            return mi;
        }

        public Emit GetEmit(bool isToPersist)
        {
            Emit mi;
            if (!isToPersist)
            {
                mi = methods.Pop();
            }
            else
            {
                mi = methods.Pop();
                Type t = typeBuilder.CreateType();
                assemblyBuilder.Save(assemblyName.Name + ".dll");
            }

            AlreadyBuildedMethods.Add(lastType, mi);
            return mi;
        }
    }
}