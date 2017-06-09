//#define RUN_ONLY
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
#if RUN_ONLY
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.Run;
#else
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave;
#endif
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, attributes, "D:\\");
            moduleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");
            typeBuilder = moduleBuilder.DefineType($"{this.assemblyName.Name}.Handler", TypeAttributes.Public);
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
                emiter = Emit.BuildStaticMethod(returnValue, inputTypes, typeBuilder, v, MethodAttributes.Public, true, true);
            else
                emiter = Emit.NewDynamicMethod(returnValue, inputTypes);

            BuildedFunction bf = new BuildedFunction()
            {
                Emiter = emiter,
                Status = BuildedFunctionStatus.FunctionNotFinalized,
                Input = inputTypes,
                Return = returnValue
            };

            AlreadyBuildedMethods.Add(inputTypes[0], bf);

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
                bf.Delegate = bf.Emiter.CreateDelegate(delegateType);
            }

            bf.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;

            if (methods.Count == 0)
            {
                if (typeBuilder != null)
                {
                    Type currentType = typeBuilder.CreateType();
                    bf.Method = currentType.GetMethod(bf.Method.Name, bf.Method.GetParameters().Select(x => x.ParameterType).ToArray());
                    Debug.Assert(assemblyBuilder != null);
#if !RUN_ONLY
                    assemblyBuilder.Save(assemblyName.Name + ".dll");
#endif
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