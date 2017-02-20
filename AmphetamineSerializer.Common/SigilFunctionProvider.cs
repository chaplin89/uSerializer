using System;
using System.Collections.Generic;
using Sigil.NonGeneric;
using System.Reflection;
using System.Linq;

namespace AmphetamineSerializer.Common
{
    public class SigilFunctionProvider
    {
        AssemblyName assemblyName;
        System.Reflection.Emit.AssemblyBuilder assemblyBuilder;
        System.Reflection.Emit.ModuleBuilder moduleBuilder;
        System.Reflection.Emit.TypeBuilder typeBuilder;

        public Dictionary<Type, BuildedFunction> AlreadyBuildedMethods { get; set; }
        Stack<BuildedFunction> methods = new Stack<BuildedFunction>();
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;

        public SigilFunctionProvider(string assemblyName)
        {
            AlreadyBuildedMethods = new Dictionary<Type, BuildedFunction>();
            this.assemblyName = new AssemblyName(assemblyName);
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave;
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, attributes, "D:\\");
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + ".dll");
            typeBuilder = moduleBuilder.DefineType(string.Format("{0}.Handler", this.assemblyName.Name), TypeAttributes.Public);
        }

        public Emit AddMethod(string v, Type[] inputTypes, Type returnValue)
        {
            if (v == null)
                throw new ArgumentNullException("v");
            if (inputTypes == null)
                throw new ArgumentNullException("inputTypes");
            if (returnValue == null)
                returnValue = typeof(void);

            BuildedFunction bf = new BuildedFunction()
            {
                Emiter = Emit.BuildStaticMethod(returnValue, inputTypes, typeBuilder, v, MethodAttributes.Public, true, false),
                Status = BuildedFunctionStatus.FunctionNotFinalized,
            };

            methods.Push(bf);
            return bf.Emiter;
        }

        public BuildedFunction GetMethod()
        {
            BuildedFunction bf = methods.Pop();

            bf.Method = bf.Emiter.CreateMethod();
            bf.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;

            if (methods.Count == 0)
            {
                foreach (var item in methods)
                {
                    BuildedFunction tempBf = item;
                    tempBf.Method = tempBf.Emiter.CreateMethod();
                    tempBf.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;
                }

                Type currentType = typeBuilder.CreateType();
                bf.Method = currentType.GetMethod(bf.Method.Name, bf.Method.GetParameters().Select(x=>x.ParameterType).ToArray());
                assemblyBuilder.Save(assemblyName.Name + ".dll");

                foreach (var v in AlreadyBuildedMethods)
                {
                    v.Value.Emiter = null;
                    v.Value.Status = BuildedFunctionStatus.TypeFinalized;
                }

                bf.Status = BuildedFunctionStatus.TypeFinalized;
            }

            return bf;
        }
    }
}