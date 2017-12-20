#define RUN_ONLY
using System;
using System.Collections.Generic;
using Sigil.NonGeneric;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using AmphetamineSerializer.Chain;
using System.IO;
using SysEmit = System.Reflection.Emit;

namespace AmphetamineSerializer.Common
{
    public class SigilFunctionProvider
    {
        AssemblyName assemblyName;
        SysEmit.AssemblyBuilder assemblyBuilder;
        SysEmit.ModuleBuilder moduleBuilder;
        SysEmit.TypeBuilder typeBuilder;

        public Dictionary<Type, ElementBuildResponse> AlreadyBuildedMethods { get; set; }
        Stack<ElementBuildResponse> methods = new Stack<ElementBuildResponse>();

        public SigilFunctionProvider(string assemblyName = null)
        {
            AlreadyBuildedMethods = new Dictionary<Type, ElementBuildResponse>();

            if (string.IsNullOrEmpty(assemblyName))
                return;

            this.assemblyName = new AssemblyName(assemblyName);
#if RUN_ONLY
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.Run;
#else
            var attributes = System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave;
#endif
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.assemblyName, attributes, Path.GetTempPath());
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

            ElementBuildResponse response = new ElementBuildResponse()
            {
                Emiter = emiter,
                Status = BuildedFunctionStatus.FunctionNotFinalized,
                Input = inputTypes,
                Return = returnValue
            };

            AlreadyBuildedMethods.Add(inputTypes[0], response);

            methods.Push(response);
            return response.Emiter;
        }

        public ElementBuildResponse GetMethod()
        {
            ElementBuildResponse response = methods.Pop();

            response.Method = response.Emiter.CreateMethod();
            response.Status = BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized;

            if (methods.Count == 0)
            {
                if (typeBuilder != null)
                {
                    Type currentType = typeBuilder.CreateType();
                    response.Method = currentType.GetMethod(response.Method.Name, response.Method.GetParameters().Select(x => x.ParameterType).ToArray());
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

                response.Status = BuildedFunctionStatus.TypeFinalized;
            }

            return response;
        }
    }
}