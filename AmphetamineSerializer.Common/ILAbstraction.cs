using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Sigil.NonGeneric;
using Sigil;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Common.Attributes;

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

        public void Load(FoundryContext ctx)
        {
            switch (ctx.Element.ElementType)
            {
                case ElementType.Field:
                    LoadFromField(ctx);
                    break;
                case ElementType.Local:
                    LoadFromLocal(ctx);
                    break;
                case ElementType.Custom:
                    LoadCustom(ctx);
                    break;
                default:
                    break;
            }
        }

        private void LoadCustom(FoundryContext ctx)
        {
            ctx.Element.CustomElement.LoadAction(ctx);
        }

        private void LoadFromLocal(FoundryContext ctx)
        {
            ctx.G.LoadLocal(ctx.Element.LocalVariable);
        }

        private void LoadFromField(FoundryContext ctx)
        {
            if (ctx.Element.ItemType.IsArray)
            {
                ctx.G.LoadLocal(ctx.Element.FieldElement.Instance);
                ctx.G.LoadField(ctx.Element.FieldElement.Field);
                ctx.G.LoadLocal(ctx.Element.LoopCtx.Index);
                ctx.G.LoadElement(ctx.Element.UnderlyingType);
            }
            else
            {
                ctx.G.LoadLocal(ctx.Element.FieldElement.Instance);
                ctx.G.LoadField(ctx.Element.FieldElement.Field);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="loadValueToStore"></param>
        public void Store(FoundryContext ctx, Action<FoundryContext> loadValueToStore)
        {
            switch (ctx.Element.ElementType)
            {
                case ElementType.Field:
                    StoreInField(ctx, loadValueToStore);
                    break;
                case ElementType.Local:
                    StoreInLocal(ctx, loadValueToStore);
                    break;
                case ElementType.Custom:
                    StoreOther(ctx, loadValueToStore);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="loadValueToStore"></param>
        private void StoreOther(FoundryContext ctx, Action<FoundryContext> loadValueToStore)
        {
            loadValueToStore(ctx);
            ctx.Element.CustomElement.StoreAction(ctx);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="loadValueToStore"></param>
        private void StoreInLocal(FoundryContext ctx, Action<FoundryContext> loadValueToStore)
        {
            loadValueToStore(ctx);
            ctx.G.StoreLocal(ctx.Element.LocalVariable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="loadValueToStore"></param>
        private void StoreInField(FoundryContext ctx, Action<FoundryContext> loadValueToStore)
        {
            if (ctx.Element.ItemType.IsArray)
            {
                ctx.G.LoadLocal(ctx.Element.FieldElement.Instance);
                ctx.G.LoadField(ctx.Element.FieldElement.Field);
                ctx.G.LoadLocal(ctx.Element.LoopCtx.Index);
            }
            else
            {
                ctx.G.LoadLocal(ctx.Element.FieldElement.Instance);
            }

            loadValueToStore(ctx);

            if (ctx.Element.ItemType.IsArray)
                ctx.G.StoreElement(ctx.Element.UnderlyingType);
            else
                ctx.G.StoreField(ctx.Element.FieldElement.Field);
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
        /// Load in the stack objectInstance.field or &amp;objectInstance.field 
        /// </summary>
        /// <param name="objectInstance">Instance</param>
        /// <param name="field">Field</param>
        /// <param name="whatToLoad">Tell if the caller is interested in the address or in the content</param>
        public void EmitAccessObject(Local objectInstance, FieldInfo field, TypeOfContent whatToLoad = TypeOfContent.Value)
        {
            g.LoadLocal(objectInstance); // this --> stack
            if (whatToLoad == TypeOfContent.Address)
                g.LoadFieldAddress(field); // &this.CurrentItem --> stack
            else
                g.LoadField(field); // this.CurrentItem --> stack
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
            g.StoreField(field); // &this.CurrentItem --> stack
        }

        /// <summary>
        /// Generate a loop preamble:
        /// 1. Initialize the index
        /// 2. Jump for checking if current index is out of bound
        /// 3. Mark the begin of the loop's body
        /// </summary>
        /// <param name="ctx">Context of the loop</param>
        /// <remarks>
        /// C# Translation:
        ///     Index = 0;
        ///     (Initialize the array);
        /// </remarks>
        public void AddLoopPreamble(FoundryContext ctx)
        {
            Contract.Ensures(ctx != null);

            var currentLoopContext = ctx.LoopCtx.Peek();
            currentLoopContext.Body = ctx.G.DefineLabel();
            currentLoopContext.CheckOutOfBound = ctx.G.DefineLabel();

            Type indexType = ctx.Element.CurrentAttribute?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            // Write in stream
            if (!ctx.ManageLifeCycle)
            {
                Type requestType = indexType;

                var currentElement = ctx.Element;

                ctx.Element = new ElementDescriptor()
                {
                    CustomElement = new CustomElementInfo()
                    {
                        LoadAction = (context) =>
                        {
                            context.G.LoadLocal(currentElement.FieldElement.Instance); // this (stfld) --> stack
                            context.G.LoadField(currentElement.FieldElement.Field); // this.CurrentItemFieldInfo --> stack
                            context.G.LoadLength(currentElement.FieldElement.Field.FieldType.GetElementType());
                        }
                    },
                    ItemType = indexType,
                    UnderlyingType = indexType
                };

                // Write the size of the array
                var request = new SerializationBuildRequest()
                {
                    Element = ctx.Element,
                    DelegateType = MakeDelegateType(requestType, ctx.InputParameters),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                ctx.Element = currentElement;

                if (response.Response.Status != BuildedFunctionStatus.NoMethodsAvailable)
                {
                    currentLoopContext.Size = ctx.G.DeclareLocal(typeof(uint));
                    ctx.G.LoadLocal(ctx.Element.FieldElement.Instance); // this (stfld) --> stack
                    ctx.G.LoadField(ctx.Element.FieldElement.Field); // this.CurrentItemFieldInfo --> stack
                    ctx.G.LoadLength(ctx.Element.FieldElement.Field.FieldType.GetElementType());
                    ctx.G.StoreLocal(currentLoopContext.Size);
                    ctx.G.LoadLocal(currentLoopContext.Size);

                    if (response.Response.Status == BuildedFunctionStatus.TypeFinalized)
                        ForwardParameters(ctx.InputParameters, response.Response);
                    else
                    {
                        ForwardParameters(ctx.InputParameters, null);
                        ctx.G.Call(response.Response.Emiter);
                    }
                }
            }

            // Case #1: Noone created the Size variable; create a new one and expect to find its value
            //          in the stream.
            // Case #2: The Size variable was already initialized by someone else; Use it.
            else if (currentLoopContext.Size == null)
            {
                var currentElement = ctx.Element;
                currentLoopContext.Size = ctx.G.DeclareLocal(indexType);

                ctx.Element = new ElementDescriptor()
                {
                    LocalVariable = currentLoopContext.Size,
                    ItemType = indexType,
                    UnderlyingType = indexType
                };

                var request = new SerializationBuildRequest()
                {
                    Element = ctx.Element,
                    DelegateType = MakeDelegateType(indexType.MakeByRefType(), ctx.InputParameters),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;
                ctx.Element = currentElement;

                if (response.Response.Status != BuildedFunctionStatus.NoMethodsAvailable)
                {
                    // this.DecodeUInt(ref size, buffer, ref position);
                    ctx.G.LoadLocalAddress(currentLoopContext.Size);
                    ForwardParameters(ctx.InputParameters, response.Response);
                }
            }

            if (ctx.ManageLifeCycle)
            {
                if (!currentLoopContext.StoreAtPosition.HasValue)
                {
                    // ObjectInstance.CurrentItemFieldInfo = new CurrentItemUnderlyingType[Size];
                    ctx.G.LoadLocal(ctx.Element.FieldElement.Instance); // this (stfld) --> stack
                    ctx.G.LoadLocal(currentLoopContext.Size); // size --> stack
                    ctx.G.NewArray(ctx.Element.UnderlyingType); // new Array[size] --> stack
                    ctx.G.StoreField(ctx.Element.FieldElement.Field); // stack --> item
                }
                else
                {
                    // ObjectInstance.CurrentItemFieldInfo[StoreAtPosition] = new CurrentItemUnderlyingType[Size]
                    ctx.G.LoadLocal(ctx.Element.FieldElement.Instance); // this (stfld) --> stack
                    ctx.G.LoadField(ctx.Element.FieldElement.Field); // this.CurrentItemFieldInfo --> stack
                    ctx.G.LoadConstant(currentLoopContext.StoreAtPosition.Value); // StoreAtPosition --> stack
                    ctx.G.LoadLocal(currentLoopContext.Size); // size --> stack
                    ctx.G.NewArray(ctx.Element.UnderlyingType); // new Array[size] --> stack
                    ctx.G.StoreElement(ctx.Element.ItemType);
                }
            }
            // int indexLocal = 0;
            // goto CheckOutOfBound;
            ctx.G.LoadConstant(0); // 0 --> stack
            ctx.G.StoreLocal(currentLoopContext.Index); // stack --> indexLocal
            ctx.G.Branch(currentLoopContext.CheckOutOfBound); // Local variables initialized, jump

            // Loop start
            ctx.G.MarkLabel(currentLoopContext.Body);
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
            Contract.Ensures(ctx.LoopCtx.Count > 0);

            var currentLoopContext = ctx.LoopCtx.Pop();

            IncrementLocalVariable(currentLoopContext.Index);

            ctx.G.MarkLabel(currentLoopContext.CheckOutOfBound);
            ctx.G.LoadLocal(currentLoopContext.Index); // Index --> stack

            // If the Size is not provided, load the lenght of the array.
            if (currentLoopContext.Size == null)
            {
                ctx.G.LoadLocal(ctx.Element.FieldElement.Instance); // this --> stack
                ctx.G.LoadField(ctx.Element.FieldElement.Field); // Array --> stack
                ctx.G.LoadLength(ctx.Element.UnderlyingType); // Array.Lenght --> stack
            }
            else
            {
                ctx.G.LoadLocal(currentLoopContext.Size);
            }

            ctx.G.BranchIfLess(currentLoopContext.Body);
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
