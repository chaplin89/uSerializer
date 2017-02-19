using AmphetamineSerializer.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Sigil.NonGeneric;
using Sigil;
using System.Linq;

namespace AmphetamineSerializer.Common
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
    public class LoopContext : IDisposable
    {
        /// <summary>
        /// This is the index.
        /// </summary>
        public Local Index { get; set; }

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
        public Local Size { get; set; }

        /// <summary>
        /// If it's not null, CurrentItemFieldInfo is assumed to be
        /// an array, and the loopManager won't try to set the
        /// </summary>
        public int? StoreAtPosition { get; set; }
        
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
                Mode = ctx.ObjectType.IsByRef ? FoundryMode.ManageLifeCycle : FoundryMode.ReadOnly,
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Provide basic abstraction from IL.
    /// </summary>
    public class ILAbstraction
    {
        private Emit g;

        /// <summary>
        /// Construct an ILAbstraction object.
        /// </summary>
        /// <param name="g">Generator</param>
        public ILAbstraction(Emit g)
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
        public void EmitLoadArray(Local objectInstance, object index, object array, Type elementType = null, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            Contract.Ensures((array is Local && ((Local)array).LocalType.IsArray) ||
                             (array is FieldInfo && ((FieldInfo)array).FieldType.IsArray));
            Contract.Ensures(index is Local || index is int);

            Type typeToLoad = DeduceTypeAndLoad(array, objectInstance);

            if (elementType != null)
                typeToLoad = elementType;

            if (index is Local)
                g.LoadLocal((Local)index); // index --> stack
            else
                g.LoadConstant((int)index); // index --> stack

            if (whatToLoad == TypeOfContent.Value)
                g.LoadElement(typeToLoad); // arraylocal[indexLocal] --> stack
            else
                g.LoadElementAddress(typeToLoad); // arraylocal[indexLocal] --> stack
        }

        /// <summary>
        /// Return the content of objectInstance.obj if obj is a FieldInfo.
        /// Return the content of objectInstance if it's a local variable.
        /// </summary>
        /// <param name="obj">Can be a LocalBuilder or a FieldInfo depending on the needs.</param>
        /// <param name="objectInstance">Instance of the object that is being deserialized.</param>
        /// <returns></returns>
        private Type DeduceTypeAndLoad(object obj, Local objectInstance)
        {
            Type typeToLoad = null;
            if (obj is Local)
                typeToLoad = ((Local)obj).LocalType.GetElementType();
            else if (obj is FieldInfo)
                typeToLoad = ((FieldInfo)obj).FieldType.GetElementType();
            else
                throw new ArgumentException("obj type not recognized.");

            if (obj is Local)
            {
                g.LoadLocal((Local)obj); // field --> stack
            }
            else
            {
                if (objectInstance != null)
                    g.LoadLocal(objectInstance); // this --> stack
                g.LoadField((FieldInfo)obj); // field --> stack
            }
            return typeToLoad;
        }

        /// <summary>
        /// Increment a local variable with a given step
        /// </summary>
        /// <param name="index">Variable to increment</param>
        /// <param name="step">Step</param>
        public void IncrementLocalVariable(Local index, int step = 1)
        {
            // C# Translation:
            //     index+=step;
            g.LoadLocal(index); // index --> stack
            if (step == 1)
                g.LoadConstant(1); // 1 --> stack
            else
                g.LoadConstant(step); // step --> stack
            g.Add(); // index + step --> stack
            g.StoreLocal(index); // stack --> index
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
        public void EmitStoreArray(Local objectInstance, Local index, FieldInfo array, Local value, Type undelyingType, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.LoadLocal(objectInstance);
            g.LoadField(array);
            g.LoadLocal(index);

            if (whatToStore == TypeOfContent.Value)
                g.LoadLocal(value);
            else
                g.LoadLocalAddress(value);

            g.StoreElement(undelyingType);
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
        public void EmitStoreArray(Local objectInstance, int index, FieldInfo array, Local value, Type undelyingType, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.LoadLocal(objectInstance);
            g.LoadField(array);
            g.LoadConstant(index);

            if (whatToStore == TypeOfContent.Value)
                g.LoadLocal(value);
            else
                g.LoadLocalAddress(value);

            g.StoreElement(undelyingType);
        }

        /// <summary>
        /// Load in the stack objectInstance.field or &amp;objectInstance.field 
        /// </summary>
        /// <param name="objectInstance">Instance</param>
        /// <param name="field">Field</param>
        /// <param name="whatToLoad">Tell if the caller is interested in the address or in the content</param>
        public void EmitAccessObject(Local objectInstance, FieldInfo field, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            g.LoadLocal(objectInstance);      // this                                            --> stack
            if (whatToLoad == TypeOfContent.Address)
                g.LoadFieldAddress(field);          // &this.CurrentItem                               --> stack
            else
                g.LoadField(field);           // this.CurrentItem                                --> stack
        }

        /// <summary>
        /// Load in the stack objectInstance.field or &amp;objectInstance.field 
        /// </summary>
        /// <param name="objectInstance">Instance</param>
        /// <param name="field">Field</param>
        /// <param name="whatToLoad">Tell if the caller is interested in the address or in the content</param>
        public void EmitStoreObject(Local objectInstance, FieldInfo field, Local content, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            g.LoadLocal(objectInstance); // this --> stack

            if (whatToLoad == TypeOfContent.Address)
                g.LoadLocalAddress(content);// this --> stack
            else
                g.LoadLocal(content); // this --> stack
            g.StoreField(field);          // &this.CurrentItem --> stack
        }

        /// <summary>
        /// Generate a loop preamble:
        /// 1. Initialize the index
        /// 2. Jump for checking if current index is out of bound
        /// 3. Mark the begin of the loop's body
        /// </summary>
        /// <param name="ctx">Context of the loop</param>
        /// <remarks>The loop context can be provided by the caller or it can be null.
        /// If it's null, the function will generate a new loop context that you need to pass to the 
        /// <see cref="AddLoopEpilogue(LoopContext)"/> function.
        /// C# Translation:
        ///     Index = 0;
        ///     (Initialize the array);
        /// </remarks>
        public void AddLoopPreamble(ref FoundryContext ctx)
        {
            Contract.Ensures(ctx != null);

            ctx.LoopCtx.Body = ctx.G.DefineLabel();
            ctx.LoopCtx.CheckOutOfBound = ctx.G.DefineLabel();

            Type indexType = ctx.CurrentItemFieldInfo.GetCustomAttribute<ASIndexAttribute>(false)?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            if (ctx.LoopCtx.Mode == FoundryMode.ReadOnly)
            {
                Type requestType = indexType;

                var request = new SerializationBuildRequest()
                {
                    DelegateType = MakeDelegateType(requestType, ctx.InputParameters),
                    AdditionalContext = ctx
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                if (response.Method.Status != BuildedFunctionStatus.NoMethodsAvailable)
                {
                    ctx.LoopCtx.Size = ctx.G.DeclareLocal(typeof(uint));
                    ctx.G.LoadLocal(ctx.ObjectInstance); // this (stfld) --> stack
                    ctx.G.LoadField(ctx.CurrentItemFieldInfo); // this.CurrentItemFieldInfo --> stack
                    ctx.G.LoadLength(ctx.CurrentItemFieldInfo.FieldType.GetElementType());
                    ctx.G.StoreLocal(ctx.LoopCtx.Size);
                    ctx.G.LoadLocal(ctx.LoopCtx.Size);

                    if (response.Method.Status == BuildedFunctionStatus.TypeFinalized)
                        ForwardParameters(ctx.InputParameters, response.Method);
                    else
                    {
                        ForwardParameters(ctx.InputParameters, null);
                        ctx.G.Call(response.Method.Emiter);
                    }
                }
            }

            // Case #1: Noone created the Size variable; create a new one and expect to find its value
            //          in the stream.
            // Case #2: The Size variable was already initialized by someone else; Use it.
            else if (ctx.LoopCtx.Size == null)
            {
                var request = new SerializationBuildRequest()
                {
                    DelegateType = MakeDelegateType(indexType.MakeByRefType(), ctx.InputParameters),
                    AdditionalContext = ctx
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                ctx.LoopCtx.Size = ctx.G.DeclareLocal(indexType);

                // this.DecodeUInt(ref size, buffer, ref position);
                ctx.G.LoadLocalAddress(ctx.LoopCtx.Size);
                ForwardParameters(ctx.InputParameters, response.Method);
            }

            if (ctx.LoopCtx.Mode == FoundryMode.ManageLifeCycle)
            {
                if (!ctx.LoopCtx.StoreAtPosition.HasValue)
                {
                    // ObjectInstance.CurrentItemFieldInfo = new CurrentItemUnderlyingType[Size];
                    ctx.G.LoadLocal(ctx.ObjectInstance); // this (stfld) --> stack
                    ctx.G.LoadLocal(ctx.LoopCtx.Size); // size --> stack
                    ctx.G.NewArray(ctx.CurrentItemUnderlyingType); // new Array[size] --> stack
                    ctx.G.StoreField(ctx.CurrentItemFieldInfo); // stack --> item
                }
                else
                {
                    // ObjectInstance.CurrentItemFieldInfo[StoreAtPosition] = new CurrentItemUnderlyingType[Size]
                    ctx.G.LoadLocal(ctx.ObjectInstance); // this (stfld) --> stack
                    ctx.G.LoadField(ctx.CurrentItemFieldInfo); // this.CurrentItemFieldInfo --> stack
                    ctx.G.LoadConstant(ctx.LoopCtx.StoreAtPosition.Value); // StoreAtPosition --> stack
                    ctx.G.LoadLocal(ctx.LoopCtx.Size); // size --> stack
                    ctx.G.NewArray(ctx.CurrentItemUnderlyingType); // new Array[size] --> stack
                    ctx.G.StoreElement(ctx.CurrentItemType);
                }
            }
            // int indexLocal = 0;
            // goto CheckOutOfBound;
            ctx.G.LoadConstant(0); // 0 --> stack
            ctx.G.StoreLocal(ctx.LoopCtx.Index); // stack --> indexLocal
            ctx.G.Branch(ctx.LoopCtx.CheckOutOfBound); // Local variables initialized, jump

            // Loop start
            ctx.G.MarkLabel(ctx.LoopCtx.Body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h"></param>
        /// <param name="currentArray"></param>
        /// <param name="objectTemp"></param>
        /// <param name="type"></param>
        /// <param name="whatToStore"></param>
        public void EmitStoreArray(int h, Local currentArray, Local objectTemp, Type type, TypeOfContent whatToStore = TypeOfContent.Value)
        {
            g.LoadLocal(currentArray);
            g.LoadConstant(h);

            if (whatToStore == TypeOfContent.Value)
                g.LoadLocal(objectTemp);
            else
                g.LoadLocalAddress(objectTemp);

            g.StoreElement(type);
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
        public void AddLoopEpilogue(FoundryContext ctx)
        {
            Contract.Ensures(ctx != null);
            Contract.Ensures(ctx.LoopCtx.Index != null);

            IncrementLocalVariable(ctx.LoopCtx.Index);

            ctx.G.MarkLabel(ctx.LoopCtx.CheckOutOfBound);
            ctx.G.LoadLocal(ctx.LoopCtx.Index); // Index --> stack

            // If the Size is not provided, load the lenght of the array.
            if (ctx.LoopCtx.Size == null)
            {
                ctx.G.LoadLocal(ctx.ObjectInstance); // this --> stack
                ctx.G.LoadField(ctx.CurrentItemFieldInfo); // Array --> stack
                ctx.G.LoadLength<int>(); // Array.Lenght --> stack
            }
            else
            {
                ctx.G.LoadLocal(ctx.LoopCtx.Size);
            }

            ctx.G.BranchIfLess(ctx.LoopCtx.Body);
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
        public void ForwardParameters(Type[] inputParameter, BuildedFunction currentMethod, ASIndexAttribute attribute = null)
        {
            if (currentMethod != null && currentMethod.Status == BuildedFunctionStatus.TypeFinalized)
            {
                ParameterInfo[] parameters = currentMethod.Method.GetParameters();
                bool[] foundParameter = new bool[parameters.Length - 1];

                for (int i = 1; i < parameters.Length; ++i)
                {
                    for (ushort j = 1; j < inputParameter.Length; j++)
                    {
                        if (inputParameter[j] == parameters[i].ParameterType)
                        {
                            if (foundParameter[i - 1])
                                throw new AmbiguousMatchException("Input arguments match more than one argument in the handler signature.");
                            foundParameter[i - 1] = true;

                            g.LoadArgument(j); // argument i --> stack
                            break;
                        }
                    }
                    if (!foundParameter[i - 1])
                    {
                        throw new InvalidOperationException("Unable to load all the parameters for the handler.");
                    }
                }

                if (currentMethod.Method.IsStatic)
                    g.Call(currentMethod.Method);                 // void func(ref obj,byte[], ref int)
                else
                    g.CallVirtual(currentMethod.Method);
            }
            else
            {
                for (ushort j = 1; j < inputParameter.Length; j++)
                    g.LoadArgument(j); // argument i --> stack
            }
        }
    }
}
