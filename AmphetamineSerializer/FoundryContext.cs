using AmphetamineSerializer.Common;
using AmphetamineSerializer.FunctionProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer
{
    public enum FoundryMode
    {
        ManageLifeCycle,
        ReadOnly
    }

    /// <summary>
    /// Context of the assembly building process.
    /// </summary>
    public class FoundryContext
    {
        private FoundryContext()
        {
            AlreadyBuildedMethods = new Dictionary<Type, MethodInfo>();
        }

        /// <summary>
        /// The <see cref="FieldInfo"/> structure for the current item.
        /// </summary>
        /// <seealso cref="CurrentItemType"/>
        public FieldInfo CurrentItemFieldInfo { get; set; }

        /// <summary>
        /// When building the deserializing logic for a type, 
        /// this field contains the type for the currently processed
        /// field.
        /// </summary>
        public Type CurrentItemType { get; set; }

        /// <summary>
        /// If it's an array or an enum, this field contains the type
        /// contained inside.
        /// </summary>
        public Type CurrentItemUnderlyingType { get; set; }

        /// <summary>
        /// If it's inside a loop, this is the index.
        /// </summary>
        public LocalBuilder Index { get; set; }

        /// <summary>
        /// The instance of the object in course of deserialization.
        /// </summary>
        public LocalBuilder ObjectInstance { get; set; }

        /// <summary>
        /// Type upon wich build the deserialization logic.
        /// </summary>
        public Type ObjectType
        {
            get { return (InputParameters != null) ? InputParameters.FirstOrDefault() : null; }
        }

        /// <summary>
        /// Methods builded in this context.
        /// </summary>
        public Dictionary<Type, MethodInfo> AlreadyBuildedMethods { get; set; }

        /// <summary>
        /// Provide basic abstraction from IL.
        /// </summary>
        public ILAbstraction Manipulator { get; set; }

        /// <summary>
        /// If not null, the assembly will be build based on this sample structure.
        /// </summary>
        public object AdditionalContext { get; set; }

        /// <summary>
        /// Provide an easy way to build a function from scratch.
        /// </summary>
        public IFunctionProvider Provider { get; set; }

        /// <summary>
        /// Generator.
        /// </summary>
        public ILGenerator G { get; set; }

        /// <summary>
        /// Types in input to the method that will be forwarded to the
        /// deserialization handler.
        /// </summary>
        public Type[] InputParameters { get; set; }

        /// <summary>
        /// Known handlers for managing types.
        /// </summary>
        public IChainManager Chain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheleton"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static FoundryContext MakeContext(Type scheleton, object context)
        {
            return new FoundryContext()
            {
                InputParameters = scheleton.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray(),
                AdditionalContext = context
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public SIndexAttribute CurrentAttribute
        {
            get
            {
                if (CurrentItemFieldInfo != null)
                    return CurrentItemFieldInfo.GetCustomAttribute<SIndexAttribute>();
                return null;
            }
        }
    }
}
