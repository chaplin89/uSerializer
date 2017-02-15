using Mermec.Trackware.DataModel.Samples;
using Mermec.Trackware.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mermec.Trackware.DataModel.Samples
{
    public class TWSampleDeserializer
    {
        private FoundryContext ctx;
        bool isToPersist = true;

        public TWSampleDeserializer(FoundryContext ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Generate the method for deserializing a given sample structure.
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
        public MethodInfo Build()
        {
            ctx.ObjectInstance = ctx.G.DeclareLocal(ctx.ObjectType);

            ctx.G.Emit(OpCodes.Ldarg_0);
            ctx.G.Emit(OpCodes.Ldind_Ref);
            ctx.G.Emit(OpCodes.Stloc, ctx.ObjectInstance);

            BuildFromSampleStructure(ctx);

            ctx.G.Emit(OpCodes.Ret);
            return ctx.Provider.GetMethod(isToPersist);
        }

        /// <summary>
        /// Convert an internal type to a System.Type object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public Type InternalTypeToType(NativeType type)
        {
            switch (type)
            {
                case NativeType.UInt1:
                    return typeof(byte);
                case NativeType.Int1:
                    return typeof(sbyte);
                case NativeType.UInt2:
                    return typeof(ushort);
                case NativeType.Int2:
                    return typeof(short);
                case NativeType.UInt4:
                    return typeof(uint);
                case NativeType.Int4:
                case NativeType._Int4:
                    return typeof(int);
                case NativeType.Single:
                    return typeof(float);
                case NativeType.Double:
                    return typeof(double);
                case NativeType.String:
                    return typeof(string);
                default:
                    Debug.Assert(false);
                    throw new InvalidOperationException("Invalid internal type.");
            }
        }

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="currentContext">Context</param>
        private void BuildFromSampleStructure(FoundryContext ctx)
        {
            TWSampleStructure sampleStructure = (TWSampleStructure)ctx.AdditionalContext;
            Dictionary<Type, LocalBuilder> typeToLocal = new Dictionary<Type, LocalBuilder>();
            LocalBuilder objectTemp = ctx.G.DeclareLocal(typeof(object));
            int size = sampleStructure.DeserializationInfo.Length;

            ctx.Index = ctx.G.DeclareLocal(typeof(int));
            ctx.CurrentItemFieldInfo = typeof(TWSample).GetFields().Single();

            /// sample.Paramters = new object[size]
            ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance);
            ctx.G.Emit(OpCodes.Ldc_I4, size);
            ctx.G.Emit(OpCodes.Newarr, typeof(object));
            ctx.G.Emit(OpCodes.Stfld, ctx.CurrentItemFieldInfo);

            for (int i = 0; i < sampleStructure.DeserializationInfo.Length; i++)
            {
                var currentDeserializationInfo = sampleStructure.DeserializationInfo[i];
                MethodInfo currentMethod = null;
                LoopContext loopCtx = null;

                if (currentDeserializationInfo.ArrayType != NativeArrayType.Scalar)
                {
                    loopCtx = new LoopContext()
                    {
                        StoreAtPosition = i,
                        G = ctx.G,
                        Index = ctx.Index,
                        CurrentItemType = typeof(object[]),
                        CurrentItemUnderlyingType = typeof(object),
                        CurrentItemFieldInfo = ctx.CurrentItemFieldInfo,
                        ObjectInstance = ctx.ObjectInstance,
                        InputParameter = ctx.InputParameters,
                        DeserializationHandlers = ctx.DeserializationHandlers,
                        Size = ctx.G.DeclareLocal(typeof(int))
                    };

                    if (currentDeserializationInfo.ArrayType == NativeArrayType.VariableSize)
                    {
                        var arrayIndexType = sampleStructure.DeserializationInfo[currentDeserializationInfo.ArraySize].ParamInfo.Single().Type;
                        ManageVariableSize(ctx, loopCtx, currentDeserializationInfo.ArraySize, arrayIndexType);
                    }
                    else
                    {
                        ManageFixedSize(ctx, loopCtx, currentDeserializationInfo.ArraySize);
                    }
                }
                for (int h = 0; h < currentDeserializationInfo.ParamInfo.Length; h++)
                {
                    var currentParameter = currentDeserializationInfo.ParamInfo[h];
                    ctx.CurrentItemUnderlyingType = InternalTypeToType(currentParameter.Type);
                    ctx.CurrentItemType = ctx.CurrentItemUnderlyingType;

                    if (!ctx.DeserializationHandlers.ContainsKey(ctx.CurrentItemUnderlyingType))
                        throw new InvalidOperationException($"Deserialization handler not found for type {ctx.CurrentItemUnderlyingType}");

                    currentMethod = ctx.DeserializationHandlers[ctx.CurrentItemUnderlyingType];

                    LocalBuilder deserializeTempObject;

                    if (!typeToLocal.TryGetValue(ctx.CurrentItemUnderlyingType, out deserializeTempObject))
                    {
                        deserializeTempObject = ctx.G.DeclareLocal(ctx.CurrentItemUnderlyingType);
                        typeToLocal.Add(ctx.CurrentItemUnderlyingType, deserializeTempObject);
                    }

                    ctx.G.Emit(OpCodes.Ldloca, deserializeTempObject); // temp --> stack

                    for (int j = 1; j < ctx.InputParameters.Length; j++)
                        ctx.G.Emit(OpCodes.Ldarg, j); // argument i --> stack

                    ctx.G.EmitCall(OpCodes.Call, currentMethod, null); // Decode*(T, byte[], ref int)

                    ctx.G.Emit(OpCodes.Ldloc, deserializeTempObject); // temp --> stack
                    if (ctx.CurrentItemUnderlyingType.IsValueType)
                        ctx.G.Emit(OpCodes.Box, ctx.CurrentItemUnderlyingType); // (box)
                    ctx.G.Emit(OpCodes.Stloc, objectTemp);

                    if (currentDeserializationInfo.ArrayType == NativeArrayType.Scalar)
                    {
                        ctx.Manipulator.EmitStoreArray(ctx.ObjectInstance, i, ctx.CurrentItemFieldInfo, objectTemp, typeof(object));
                    }
                    else
                    {
                        ctx.Manipulator.EmitLoadArray(ctx.ObjectInstance, i, ctx.CurrentItemFieldInfo);
                        ctx.G.Emit(OpCodes.Ldloc, loopCtx.Index);
                        ctx.G.Emit(OpCodes.Ldloc, objectTemp);
                        ctx.G.Emit(OpCodes.Stelem, typeof(object));
                    }
                }

                if (currentDeserializationInfo.ArrayType != NativeArrayType.Scalar)
                    ctx.Manipulator.AddLoopEpilogue(loopCtx);
            }
        }

        private void ManageVariableSize(FoundryContext ctx, LoopContext loopCtx, uint arraySizeIndex, NativeType type)
        {
            /// Size = currentParameter.ArraySize
            ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance); // this (stfld) --> stack
            ctx.G.Emit(OpCodes.Ldfld, ctx.CurrentItemFieldInfo); // this.CurrentItemFieldInfo --> stack
            ctx.G.Emit(OpCodes.Ldc_I4, (int)arraySizeIndex); // i --> stack
            ctx.G.Emit(OpCodes.Ldelem, typeof(object));
            ctx.G.Emit(OpCodes.Unbox_Any, InternalTypeToType(type));
            ctx.G.Emit(OpCodes.Stloc, loopCtx.Size);

            ctx.Manipulator.AddLoopPreamble(ref loopCtx);
        }

        private void ManageFixedSize(FoundryContext ctx, LoopContext loopCtx, uint size)
        {
            ctx.CurrentItemType = ctx.CurrentItemUnderlyingType.MakeArrayType();

            /// Size = currentParameter.ArraySize
            ctx.G.Emit(OpCodes.Ldc_I4, size);
            ctx.G.Emit(OpCodes.Stloc, loopCtx.Size);

            ctx.Manipulator.AddLoopPreamble(ref loopCtx);
        }
    }
}
