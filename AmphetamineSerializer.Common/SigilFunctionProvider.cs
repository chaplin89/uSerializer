using System;
using System.Collections.Generic;
using Sigil.NonGeneric;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;

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
        bool dynamic = true;
        public SigilFunctionProvider(string assemblyName = null)
        {
            AlreadyBuildedMethods = new Dictionary<Type, BuildedFunction>();

            if (string.IsNullOrEmpty(assemblyName))
                return;

            dynamic = false;
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

            Emit emiter;
            if (typeBuilder != null)
                emiter = Emit.BuildStaticMethod(returnValue, inputTypes, typeBuilder, v, MethodAttributes.Public, true, false);
            else
                emiter = Emit.NewDynamicMethod(returnValue, inputTypes);

            BuildedFunction bf = new BuildedFunction()
            {
                Emiter = emiter,
                Status = BuildedFunctionStatus.FunctionNotFinalized,
                Input = inputTypes,
                Return = returnValue
            };
            methods.Push(bf);
            return bf.Emiter;
        }

        public BuildedFunction GetMethod()
        {
            BuildedFunction bf = methods.Pop();

            if (!dynamic)
            { 
                bf.Method = bf.Emiter.CreateMethod();
            }
            else
            {
                var delegateType = Expression.GetDelegateType(bf.Input.Concat(new Type[] { bf.Return }).ToArray());
                bf.Method = bf.Emiter.CreateDelegate(delegateType).Method;
            }

            bf.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;

            if (methods.Count == 0)
            {
                foreach (var item in methods)
                {
                    BuildedFunction tempBf = item;
                    tempBf.Method = tempBf.Emiter.CreateMethod();
                    tempBf.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;
                }

                if (typeBuilder != null)
                {
                    Type currentType = typeBuilder.CreateType();
                    bf.Method = currentType.GetMethod(bf.Method.Name, bf.Method.GetParameters().Select(x => x.ParameterType).ToArray());
                    Debug.Assert(assemblyBuilder != null);
                    assemblyBuilder.Save(assemblyName.Name + ".dll");
                }

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