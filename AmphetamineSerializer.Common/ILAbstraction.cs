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
using AmphetamineSerializer.Common.Element;

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
    /// TODO: REMOVE THIS MESS!!
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

            Type indexType = ((FieldElement)ctx.Element).Attribute?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            // Write in stream
            if (!ctx.ManageLifeCycle)
            {
                Type requestType = indexType;

                var currentElement = ctx.Element;

                currentLoopContext.Size = (GenericElement)((g, content) =>
                {
                    //TODO: ADDRESS IF SIZE WAS NOT NULL
                    currentElement.Load(g, TypeOfContent.Value);
                    g.LoadLength(currentElement.ElementType);
                });

                // Write the size of the array
                var request = new SerializationBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    DelegateType = MakeDelegateType(requestType, ctx.InputParameters),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                ctx.Element = currentElement;

                if (response.Response.Status != BuildedFunctionStatus.NoMethodsAvailable)
                {
                    currentLoopContext.Size = (LocalElement)ctx.G.DeclareLocal(typeof(uint));
                    var lenght = (GenericElement)((g, element) =>
                    {
                        ctx.Element.Load(g, TypeOfContent.Value);
                        ctx.G.LoadLength(ctx.Element.ElementType);
                    });

                    currentLoopContext.Size.Store(ctx.G, lenght, TypeOfContent.Value);
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Value);

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
                currentLoopContext.Size = (LocalElement)ctx.G.DeclareLocal(indexType);

                var request = new SerializationBuildRequest()
                {
                    Element = currentLoopContext.Size,
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
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Address);
                    ForwardParameters(ctx.InputParameters, response.Response);
                }
            }

            if (ctx.ManageLifeCycle)
            {
                // ObjectInstance.CurrentItemFieldInfo = new CurrentItemUnderlyingType[Size];
                var newArray = (GenericElement)((g, _) =>
                {
                    currentLoopContext.Size.Load(g, TypeOfContent.Value);
                    ctx.G.NewArray(ctx.Element.ElementType);
                });

                ctx.Element.Store(ctx.G, newArray, TypeOfContent.Value);
            }

            // int indexLocal = 0;
            // goto CheckOutOfBound;
            ctx.G.LoadConstant(0);
            ctx.G.StoreLocal(currentLoopContext.Index);
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

            while (ctx.LoopCtx.Count > 0)
            {
                var currentLoopContext = ctx.LoopCtx.Pop();

                IncrementLocalVariable(currentLoopContext.Index);

                ctx.G.MarkLabel(currentLoopContext.CheckOutOfBound);
                ctx.G.LoadLocal(currentLoopContext.Index);

                // If the Size is not provided, load the lenght of the array.
                if (currentLoopContext.Size == null)
                {
                    ctx.Element.Load(ctx.G, TypeOfContent.Value);
                    ctx.G.LoadLength(ctx.Element.ElementType);
                }
                else
                {
                    currentLoopContext.Size.Load(g, TypeOfContent.Value);
                }

                ctx.G.BranchIfLess(currentLoopContext.Body);
            }
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
