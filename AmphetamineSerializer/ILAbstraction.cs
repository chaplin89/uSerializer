using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Type of content to be loaded
    /// </summary>
    public enum TypeOfContent
    {
        /// <summary>
        /// The value will be pushed
        /// </summary>
        Value,
        /// <summary>
        /// The address will be pushed
        /// </summary>
        Address
    }

    /// <summary>
    /// Manage a context for a loop
    /// </summary>
    public class LoopContext
    {
        /// <summary>
        /// Label that point to the end of the loop,
        /// where the out of bound condition is checked.
        /// </summary>
        public Label CheckOutOfBound { get; set; }

        /// <summary>
        /// Label that point at the body of the loop.
        /// </summary>
        public Label Body { get; set; }

        /// <summary>
        /// Size of the array.
        /// </summary>
        public LocalBuilder Size { get; set; }

        /// <summary>
        /// If it's not null, CurrentItemFieldInfo is assumed to be
        /// an array, and the loopManager won't try to set the
        /// </summary>
        public int? StoreAtPosition { get; set; }

        /// <summary>
        /// Generator.
        /// </summary>
        public ILGenerator G { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IChainManager Chain { get; set; }

        /// <summary>
        /// Index used in the loop.
        /// </summary>
        public LocalBuilder Index { get; set; }

        /// <summary>
        /// Type of the array that will be created.
        /// Can be CurrentItemType.GetElementType() == CurrentUnderlyingType
        /// or not (i.e. you are using an object[] to contain float elements).
        /// </summary>
        public Type CurrentItemType { get; set; }

        /// <summary>
        /// Type contained in the array that will be created.
        /// </summary>
        public Type CurrentItemUnderlyingType { get; set; }

        /// <summary>
        /// FieldInfo for 
        /// </summary>
        public FieldInfo CurrentItemFieldInfo { get; set; }

        /// <summary>
        /// Instance of the object being deserialized.
        /// </summary>
        public LocalBuilder ObjectInstance { get; set; }

        /// <summary>
        /// Types of the arguments that will be forwarded to the handlers.
        /// </summary>
        public Type[] InputParameter { get; set; }
        public FoundryMode Mode { get; private set; }

        /// <summary>
        /// Generate the LoopContext form a Foundry context.
        /// </summary>
        /// <param name="ctx">Foundy context</param>
        /// <returns>Generated LoopContext</returns>
        public static LoopContext FromFoundryContext(FoundryContext ctx)
        {
            return new LoopContext()
            {
                G = ctx.G,
                Index = ctx.Index,
                CurrentItemType = ctx.CurrentItemType,
                CurrentItemUnderlyingType = ctx.CurrentItemUnderlyingType,
                CurrentItemFieldInfo = ctx.CurrentItemFieldInfo,
                ObjectInstance = ctx.ObjectInstance,
                InputParameter = ctx.InputParameters,
                Mode = ctx.ObjectType.IsByRef ? FoundryMode.ManageLifeCycle : FoundryMode.ReadOnly,
                Chain = ctx.Chain
            };
        }
    }

    /// <summary>
    /// Provide basic abstraction from IL.
    /// </summary>
    public class ILAbstraction
    {
        private ILGenerator g;

        /// <summary>
        /// Construct an ILAbstraction object.
        /// </summary>
        /// <param name="g">Generator</param>
        public ILAbstraction(ILGenerator g)
        {
            this.g = g;
        }

        /// <summary>
        /// Emit the instructions for accessing a position inside an array.
        /// Rough C# Translation: 
        /// 
        /// Value: (elementType)objectInstance.array[index]
        /// Address: ref (elementType)objectInstance.array[index]
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="index"></param>
        /// <param name="array"></param>
        /// <param name="elementType"></param>
        /// <param name="whatToLoad">Can be value or address.</param>
        /// <remarks>You can specify an <paramref name="elementType"/> if for example you </remarks>
        public void EmitLoadArray(LocalBuilder objectInstance, object index, object array, Type elementType = null, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            Contract.Ensures((array is LocalBuilder && ((LocalBuilder)array).LocalType.IsArray) ||
                             (array is FieldInfo && ((FieldInfo)array).FieldType.IsArray));
            Contract.Ensures(index is LocalBuilder || index is int);

            Type typeToLoad = DeduceTypeAndLoad(array, objectInstance);

            if (elementType != null)
                typeToLoad = elementType;

            if (index is LocalBuilder)
                g.Emit(OpCodes.Ldloc, (LocalBuilder)index); // index --> stack
            else
                g.Emit(OpCodes.Ldc_I4, (int)index); // index --> stack

            if (whatToLoad == TypeOfContent.Value)
                g.Emit(OpCodes.Ldelem, typeToLoad); // arraylocal[indexLocal] --> stack
            else
                g.Emit(OpCodes.Ldelema, typeToLoad); // arraylocal[indexLocal] --> stack
        }

        /// <summary>
        /// Return the content of objectInstance.obj if obj is a FieldInfo.
        /// Return the content of objectInstance if it's a local variable.
        /// </summary>
        /// <param name="obj">Can be a LocalBuilder or a FieldInfo depending on the needs.</param>
        /// <param name="objectInstance">Instance of the object that is being deserialized.</param>
        /// <returns></returns>
        private Type DeduceTypeAndLoad(object obj, LocalBuilder objectInstance)
        {
            Type typeToLoad = null;
            if (obj is LocalBuilder)
                typeToLoad = ((LocalBuilder)obj).LocalType.GetElementType();
            else if (obj is FieldInfo)
                typeToLoad = ((FieldInfo)obj).FieldType.GetElementType();
            else
                throw new ArgumentException("obj type not recognized.");

            if (obj is LocalBuilder)
            {
                g.Emit(OpCodes.Ldloc, (LocalBuilder)obj); // field --> stack
            }
            else
            {
                if (objectInstance != null)
                    g.Emit(OpCodes.Ldloc, objectInstance); // this --> stack
                g.Emit(OpCodes.Ldfld, (FieldInfo)obj); // field --> stack
            }
            return typeToLoad;
        }

        /// <summary>
        /// Increment a local variable with a given step
        /// </summary>
        /// <param name="index">Variable to increment</param>
        /// <param name="step">Step</param>
        public void IncrementLocalVariable(LocalBuilder index, int step = 1)
        {
            // C# Translation:
            //     index+=step;
            g.Emit(OpCodes.Ldloc, index); // index --> stack
            if (step == 1)
                g.Emit(OpCodes.Ldc_I4_1); // 1 --> stack
            else
                g.Emit(OpCodes.Ldc_I4, step); // step --> stack
            g.Emit(OpCodes.Add); // index + step --> stack
            g.Emit(OpCodes.Stloc, index); // stack --> index
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="index"></param>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="undelyingType"></param>
        /// <param name="whatToStore"></param>
        public void EmitStoreArray(LocalBuilder objectInstance, LocalBuilder index, FieldInfo array, LocalBuilder value, Type undelyingType, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.Emit(OpCodes.Ldloc, objectInstance);
            g.Emit(OpCodes.Ldfld, array);
            g.Emit(OpCodes.Ldloc, index);

            if (whatToStore == TypeOfContent.Value)
                g.Emit(OpCodes.Ldloc, value);
            else
                g.Emit(OpCodes.Ldind_Ref, value);

            g.Emit(OpCodes.Stelem, undelyingType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectInstance"></param>
        /// <param name="index"></param>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="undelyingType"></param>
        /// <param name="whatToStore"></param>
        public void EmitStoreArray(LocalBuilder objectInstance, int index, FieldInfo array, LocalBuilder value, Type undelyingType, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.Emit(OpCodes.Ldloc, objectInstance);
            g.Emit(OpCodes.Ldfld, array);
            g.Emit(OpCodes.Ldc_I4, index);

            if (whatToStore == TypeOfContent.Value)
                g.Emit(OpCodes.Ldloc, value);
            else
                g.Emit(OpCodes.Ldind_Ref, value);

            g.Emit(OpCodes.Stelem, undelyingType);
        }

        /// <summary>
        /// Load in the stack objectInstance.field or &amp;objectInstance.field 
        /// </summary>
        /// <param name="objectInstance">Instance</param>
        /// <param name="field">Field</param>
        /// <param name="whatToLoad">Tell if the caller is interested in the address or in the content</param>
        public void EmitAccessObject(LocalBuilder objectInstance, FieldInfo field, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            g.Emit(OpCodes.Ldloc, objectInstance);      // this                                            --> stack
            if (whatToLoad == TypeOfContent.Address)
                g.Emit(OpCodes.Ldflda, field);          // &this.CurrentItem                               --> stack
            else
                g.Emit(OpCodes.Ldfld, field);           // this.CurrentItem                                --> stack
        }

        /// <summary>
        /// Load in the stack objectInstance.field or &amp;objectInstance.field 
        /// </summary>
        /// <param name="objectInstance">Instance</param>
        /// <param name="field">Field</param>
        /// <param name="whatToLoad">Tell if the caller is interested in the address or in the content</param>
        public void EmitStoreObject(LocalBuilder objectInstance, FieldInfo field, LocalBuilder content, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            g.Emit(OpCodes.Ldloc, objectInstance); // this --> stack

            if (whatToLoad == TypeOfContent.Address)
                g.Emit(OpCodes.Ldloca, content);// this --> stack
            else
                g.Emit(OpCodes.Ldloc, content); // this --> stack
            g.Emit(OpCodes.Stfld, field);          // &this.CurrentItem --> stack
        }

        /// <summary>
        /// Generate a loop preamble:
        /// 1. Initialize the index
        /// 2. Jump for checking if current index is out of bound
        /// 3. Mark the begin of the loop's body
        /// </summary>
        /// <param name="loopCtx">Context of the loop</param>
        /// <remarks>The loop context can be provided by the caller or it can be null.
        /// If it's null, the function will generate a new loop context that you need to pass to the 
        /// <see cref="AddLoopEpilogue(LoopContext)"/> function.
        /// C# Translation:
        ///     Index = 0;
        ///     (Initialize the array);
        /// </remarks>
        public void AddLoopPreamble(ref LoopContext loopCtx)
        {
            Contract.Ensures(loopCtx != null);

            loopCtx.Body = loopCtx.G.DefineLabel();
            loopCtx.CheckOutOfBound = loopCtx.G.DefineLabel();

            Type indexType = loopCtx.CurrentItemFieldInfo.GetCustomAttribute<SIndexAttribute>(false)?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            if (loopCtx.Mode == FoundryMode.ReadOnly)
            {
                Type requestType = indexType;

                var request = new SerializationBuildRequest()
                {
                    DelegateType = MakeDelegateType(requestType, loopCtx.InputParameter)
                };
                var response = loopCtx.Chain.Process(request) as SerializationBuildResponse;
                loopCtx.Size = loopCtx.G.DeclareLocal(typeof(uint));
                loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.ObjectInstance); // this (stfld) --> stack
                loopCtx.G.Emit(OpCodes.Ldfld, loopCtx.CurrentItemFieldInfo); // this.CurrentItemFieldInfo --> stack
                loopCtx.G.Emit(OpCodes.Ldlen);
                loopCtx.G.Emit(OpCodes.Stloc, loopCtx.Size);

                // this.DecodeUInt(ref size, buffer, ref position);
                loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.Size);

                ForwardParameters(loopCtx.InputParameter, response.Method);
            }

            // Case #1: Noone created the Size variable; create a new one and expect to find its value
            //          in the stream.
            // Case #2: The Size variable was already initialized by someone else; Use it.
            else if (loopCtx.Size == null)
            {
                var request = new SerializationBuildRequest()
                {
                    DelegateType = MakeDelegateType(indexType.MakeByRefType(), loopCtx.InputParameter)
                };
                var response = loopCtx.Chain.Process(request) as SerializationBuildResponse;

                loopCtx.Size = loopCtx.G.DeclareLocal(indexType);

                // this.DecodeUInt(ref size, buffer, ref position);
                loopCtx.G.Emit(OpCodes.Ldloca, loopCtx.Size);
                ForwardParameters(loopCtx.InputParameter, response.Method);
            }

            if (loopCtx.Mode == FoundryMode.ManageLifeCycle)
            {
                if (!loopCtx.StoreAtPosition.HasValue)
                {
                    // ObjectInstance.CurrentItemFieldInfo = new CurrentItemUnderlyingType[Size];
                    loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.ObjectInstance); // this (stfld) --> stack
                    loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.Size); // size --> stack
                    loopCtx.G.Emit(OpCodes.Newarr, loopCtx.CurrentItemUnderlyingType); // new Array[size] --> stack
                    loopCtx.G.Emit(OpCodes.Stfld, loopCtx.CurrentItemFieldInfo); // stack --> item
                }
                else
                {
                    // ObjectInstance.CurrentItemFieldInfo[StoreAtPosition] = new CurrentItemUnderlyingType[Size]
                    loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.ObjectInstance); // this (stfld) --> stack
                    loopCtx.G.Emit(OpCodes.Ldfld, loopCtx.CurrentItemFieldInfo); // this.CurrentItemFieldInfo --> stack
                    loopCtx.G.Emit(OpCodes.Ldc_I4, loopCtx.StoreAtPosition.Value); // StoreAtPosition --> stack
                    loopCtx.G.Emit(OpCodes.Ldloc, loopCtx.Size); // size --> stack
                    loopCtx.G.Emit(OpCodes.Newarr, loopCtx.CurrentItemUnderlyingType); // new Array[size] --> stack
                    loopCtx.G.Emit(OpCodes.Stelem, loopCtx.CurrentItemType);
                }
            }
            // int indexLocal = 0;
            // goto CheckOutOfBound;
            loopCtx.G.Emit(OpCodes.Ldc_I4_0); // 0 --> stack
            loopCtx.G.Emit(OpCodes.Stloc, loopCtx.Index); // stack --> indexLocal
            loopCtx.G.Emit(OpCodes.Br, loopCtx.CheckOutOfBound); // Local variables initialized, jump

            // Loop start
            loopCtx.G.MarkLabel(loopCtx.Body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h"></param>
        /// <param name="currentArray"></param>
        /// <param name="objectTemp"></param>
        /// <param name="type"></param>
        /// <param name="whatToStore"></param>
        public void EmitStoreArray(int h, LocalBuilder currentArray, LocalBuilder objectTemp, Type type, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.Emit(OpCodes.Ldloc, currentArray);
            g.Emit(OpCodes.Ldc_I4, h);

            if (whatToStore == TypeOfContent.Value)
                g.Emit(OpCodes.Ldloc, objectTemp);
            else
                g.Emit(OpCodes.Ldind_Ref, objectTemp);

            g.Emit(OpCodes.Stelem, type);
        }

        /// <summary>
        /// Generate a loop epilogue:
        /// 1. Increment index
        /// 2. Out of bound check, eventually jump to the body
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <remarks>The loop context must be a valid context and must be the same passed
        /// (or generated) by the <see cref="AddLoopPreamble(ref LoopContext)"/> function.
        /// C# Translation:
        ///     while (Index++ &lt; Size) {
        ///         (here follow the Body label)
        ///     }
        /// </remarks>
        public void AddLoopEpilogue(LoopContext ctx)
        {
            Contract.Ensures(ctx != null);
            Contract.Ensures(ctx.Index != null);

            IncrementLocalVariable(ctx.Index);

            ctx.G.MarkLabel(ctx.CheckOutOfBound);
            ctx.G.Emit(OpCodes.Ldloc, ctx.Index); // Index --> stack

            // If the Size is not provided, load the lenght of the array.
            if (ctx.Size == null)
            {
                ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance); // this --> stack
                ctx.G.Emit(OpCodes.Ldfld, ctx.CurrentItemFieldInfo); // Array --> stack
                ctx.G.Emit(OpCodes.Ldlen); // Array.Lenght --> stack
                ctx.G.Emit(OpCodes.Conv_I4); // stack --> to int 32
            }
            else
            {
                ctx.G.Emit(OpCodes.Ldloc, ctx.Size);
            }

            ctx.G.Emit(OpCodes.Blt, ctx.Body);
        }

        public Type MakeDelegateType(Type objectType, Type[] inputTypes)
        {
            List<Type> arguments = new List<Type>(inputTypes.Length + 1);
            arguments.Add(objectType);

            for (int i = 1; i < inputTypes.Length; i++)
                arguments.Add(inputTypes[i]);
            arguments.Add(typeof(void));

            return Expression.GetDelegateType(arguments.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public void ForwardParameters(Type[] inputParameter, MethodInfo currentMethod, SIndexAttribute attribute = null)
        {
            ulong currentOptions = 0;
            if (currentMethod != null)
            {
                var parameters = currentMethod.GetParameters();
                bool[] foundParameter = new bool[parameters.Length - 1];
                bool additionalParameterFound = false;

                for (int i = 1; i < parameters.Length; ++i)
                {
                    for (int j = 1; j < inputParameter.Length; j++)
                    {
                        if (inputParameter[j] == parameters[i].ParameterType)
                        {
                            if (foundParameter[i - 1])
                                throw new AmbiguousMatchException("Input arguments match more than one argument in the handler signature.");
                            foundParameter[i - 1] = true;

                            g.Emit(OpCodes.Ldarg, j); // argument i --> stack
                            break;
                        }
                    }
                    if (!foundParameter[i - 1])
                    {
                        if (parameters[i].ParameterType == typeof(ulong) || parameters[i].ParameterType == typeof(long))
                        {
                            if (additionalParameterFound)
                                throw new AmbiguousMatchException("More than one parameter accepting additional options were found in the handler signature.");
                            if (attribute != null)
                                currentOptions = attribute.AdditionalOptions;
                            g.Emit(OpCodes.Ldc_I8, (long)currentOptions);
                            additionalParameterFound = true;
                        }
                        else
                            throw new InvalidOperationException("Unable to load all the parameters for the handler.");
                    }
                }
                g.Emit(OpCodes.Nop);
                g.Emit(OpCodes.Nop);
                g.EmitCall(OpCodes.Call, currentMethod, null);                 // void func(ref obj,byte[], ref int)
            }
            else
            {
                for (int j = 1; j < inputParameter.Length; j++)
                    g.Emit(OpCodes.Ldarg, j); // argument i --> stack
            }
        }
    }
}
