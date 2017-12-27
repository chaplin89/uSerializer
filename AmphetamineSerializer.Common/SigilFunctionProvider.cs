#define RUN_ONLY
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Model;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SysEmit = System.Reflection.Emit;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Deal with type creation and IL code Emiter. 
    /// </summary>
    public class SigilFunctionProvider
    {
        private AssemblyName assemblyName;
        private SysEmit.AssemblyBuilder assemblyBuilder;
        private SysEmit.ModuleBuilder moduleBuilder;
        private SysEmit.TypeBuilder typeBuilder;
        private Stack<ElementBuildResponse> methods = new Stack<ElementBuildResponse>();

        /// <summary>
        /// Build a new provider
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to build.</param>
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

        /// <summary>
        /// Contains known methods.
        /// A method is associated with a status.
        /// </summary>
        public Dictionary<Type, ElementBuildResponse> AlreadyBuildedMethods { get; set; }

        /// <summary>
        /// Add a method to the generator.
        /// </summary>
        /// <param name="inputTypes"></param>
        /// <param name="returnValue"></param>
        /// <param name="v"></param>
        /// <remarks>
        /// For a complex object, there can be a chain of dependants functions used for handling the type.
        /// 
        /// Considering an object like this:
        /// class Class1 { Class2 field1; ... (other trivial types) }
        /// with Class2 defined like this:
        /// class Class2 { Class3 field1; ... (other trivial types) }
        /// and so on to ClassN, wich contains only trivial types.
        /// 
        /// Generally the workflow goes like this:
        /// 
        /// T1: Someone request to build the logic for handling Class1 -> AddMethod called for Class1 
        /// T2: While building the logic for Class1, the unknown type "Class2" is found.
        /// Whoever is building the logic for handling Class1, need the logic for handling Class2 to be 
        /// built as well before continuing. Another method is added for Class2.
        /// T3 to TN: Same as above. For every unknown type, other methods need to be built.
        ///
        /// At this point, we're in the innermost type and it does not contains any other complex object, 
        /// so no other methods need to be added.
        /// 
        /// TN+1: The logic for handling the innermost type (ClassN), can be finalized because there's 
        /// everything is needed for it. After that, GetMethod is called for ClassN.
        /// TN+2 to TN+N: Same as above for ClassI with I ranging from N-1 to 2.
        /// TN+N+1: Here we are in the outermost object (Class1). After finalizing the logic for handling Class1,
        /// a fully usable type can be returned.  At this point, in addition to add the method we also finalize the type.
        /// 
        /// Of course, this *DOES* change the way the returned method can be manipulated:
        /// 1) A method belonging to a "not yet finalized type" can be accessed only through their IL generator.
        ///    Without its generator, the method is useless.
        /// 2) A method belonging to a "finalized type" can be accessed and also invoked directly.
        /// </remarks>
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

        /// <summary>
        /// Get the builded method.
        /// </summary>
        /// <returns>Builded method</returns>
        /// <seealso cref="AddMethod(string, Type[], Type)"/>
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