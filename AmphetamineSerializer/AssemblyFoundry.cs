using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using AmphetamineSerializer.Model.Attributes;
using Sigil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Make new assemblies.
    /// </summary>
    public class AssemblyFoundry : BuilderBase
    {
        #region ctor

        /// <summary>
        /// Build an AssemblyFoundry object starting from a context.
        /// </summary>
        /// <param name="ctx">Context</param>
        public AssemblyFoundry(FoundryContext ctx) : base(ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");

            bool objectContained = false;

            if (ctx.Provider == null)
                ctx.Provider = new SigilFunctionProvider($"{ctx.ObjectType.Name}_{Guid.NewGuid()}");
            else
                objectContained = ctx.Provider.AlreadyBuildedMethods.ContainsKey(ctx.ObjectType);

            if (objectContained)
                method = ctx.Provider.AlreadyBuildedMethods[ctx.ObjectType];
            else
                ctx.G = ctx.Provider.AddMethod("Handle", ctx.InputParameters, typeof(void));

            if (ctx.Chain == null)
            {
                var manager = new ChainManager()
                                  .SetNext(new CustomSerializerFinder())
                                  .SetNext(new CustomBuilderFinder())
                                  .SetNext(new DefaultHandlerFinder());

                ctx.Chain = manager;
            }
        }
        #endregion

        #region Building related methods

        /// <summary>
        /// Generate the method for the current ObjectType.
        /// </summary>
        /// <returns>Builded method</returns>
        protected override ElementBuildResponse InternalMake()
        {
            Type normalizedType = ctx.ObjectType;
            ArgumentElement instance = new ArgumentElement(0, ctx.ObjectType);

            if (ctx.IsDeserializing)
            {
                normalizedType = ctx.ObjectType.GetElementType();
                var ctor = normalizedType.GetConstructor(new Type[] { });

                if (ctor == null)
                    throw new NotSupportedException($"The type {normalizedType.Name} does not have a parameterless constructor.");

                var load = new GenericElement(((g, _) => g.NewObject(normalizedType)), null);

                instance.Store(ctx.G, load, TypeOfContent.Value);
            }

            var versions = VersionHelper.GetExplicitlyManagedVersions(normalizedType).ToArray();

            if (versions.Length > 1)
                ManageVersions(ctx, instance, versions, normalizedType);
            else
            {
                BuildFromFields(ctx, VersionHelper.GetAllFields(instance, normalizedType));
                ctx.G.Return();
            }

            return ctx.Provider.GetMethod();
        }

        private void ManageVersions(FoundryContext ctx, IElement instance, object[] versions, Type normalizedType)
        {
            Label[] labels = new Label[versions.Length];
            var versionField = VersionHelper.GetAllFields(instance, normalizedType).First();

            if (versionField.Field.Name.ToUpperInvariant() != "version")
                throw new InvalidOperationException("The version field should be the first.");

            for (int i = 0; i < labels.Length; i++)
                labels[i] = ctx.G.DefineLabel($"Version_{i}");

            Type requestType = versionField.LoadedType;
            if (ctx.IsDeserializing)
                requestType = requestType.MakeByRefType();

            var request = new ElementBuildRequest()
            {
                Element = versionField,
                AdditionalContext = ctx.AdditionalContext,
                InputTypes = GetInputTypes(ctx, requestType),
                Provider = ctx.Provider,
                G = ctx.G
            };

            var response = ctx.Chain.Process(request) as ElementBuildResponse;
            var targetMethod = response;

            if (targetMethod.Status != BuildedFunctionStatus.ContextModified)
            {
                if (ctx.IsDeserializing)
                    versionField.Load(ctx.G, TypeOfContent.Address);
                else
                    versionField.Load(ctx.G, TypeOfContent.Value);

                ForwardParameters(ctx, targetMethod);
            }

            if (versionField.Field.FieldType == typeof(int))
            {
                versionField.Load(ctx.G, TypeOfContent.Value);
                ctx.G.LoadConstant((int)versions[0]);
                ctx.G.Subtract();
                ctx.G.Switch(labels);

                for (int i = 0; i < versions.Length; i++)
                {
                    var fields = VersionHelper.GetVersionSnapshot(instance, normalizedType, versions[i])
                                              .Where(x => x.Field.Name.ToUpperInvariant() != "version");
                    ctx.G.MarkLabel(labels[i]);
                    BuildFromFields(ctx, fields);
                    ctx.G.Return();
                }
            }
            else if(versionField.Field.FieldType.IsAssignableFrom(typeof(IEquatable<>)))
            {
                // TODO: Manage non numeric versions
            }
        }

        private Type[] GetInputTypes(FoundryContext ctx, Type overrideRootType)
        {
            if (overrideRootType != null)
            {
                Type[] finalTypes = new Type[ctx.InputParameters.Length];
                Array.Copy(ctx.InputParameters, finalTypes, ctx.InputParameters.Length);
                finalTypes[0] = overrideRootType;
                return finalTypes;
            }

            return ctx.InputParameters;
        }

        /// <summary>
        /// Increment a local variable with a given step
        /// </summary>
        /// <param name="index">Variable to increment</param>
        /// <param name="step">Step</param>
        private void IncrementLocalVariable(Local index, int step = 1)
        {
            // C# Translation:
            //     index+=step;
            ctx.G.LoadLocal(index); // index --> stack
            if (step == 1)
                ctx.G.LoadConstant(1); // 1 --> stack
            else
                ctx.G.LoadConstant(step); // step --> stack
            ctx.G.Add(); // index + step --> stack
            ctx.G.StoreLocal(index); // stack --> index
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        private void ForwardParameters(FoundryContext ctx, ElementBuildResponse currentMethod)
        {
            if (currentMethod != null && currentMethod.Status == BuildedFunctionStatus.TypeFinalized)
            {
                ParameterInfo[] parameters = currentMethod.Method.GetParameters();
                bool[] foundParameter = new bool[parameters.Length - 1];

                for (int i = 1; i < parameters.Length; ++i)
                {
                    for (ushort j = 1; j < ctx.InputParameters.Length; j++)
                    {
                        if (ctx.InputParameters[j] == parameters[i].ParameterType)
                        {
                            if (foundParameter[i - 1])
                                throw new AmbiguousMatchException("Input arguments match more than one argument in the handler signature.");
                            foundParameter[i - 1] = true;

                            ctx.G.LoadArgument(j); // argument i --> stack
                            break;
                        }
                    }
                    if (!foundParameter[i - 1])
                    {
                        throw new InvalidOperationException("Unable to load all the parameters for the handler.");
                    }
                }

                if (currentMethod.Method.IsStatic)
                    ctx.G.Call(currentMethod.Method);                 // void func(ref obj,byte[], ref int)
                else
                    ctx.G.CallVirtual(currentMethod.Method);
            }
            else
            {
                for (ushort j = 1; j < ctx.InputParameters.Length; j++)
                    ctx.G.LoadArgument(j); // argument i --> stack
            }
        }

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Fields to manage</param>
        private void BuildFromFields(FoundryContext ctx, IEnumerable<IElement> fields)
        {
            var linkedList = new LinkedList<IElement>(fields);

            while (linkedList.Count > 0)
            {
                ctx.Element = linkedList.First.Value;
                ElementBuildResponse response = null;

                // todo:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IEnumerable (excluding List ofc)
                if (ctx.Element.LoadedType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                if (ctx.Element.IsIndexable)
                {
                    var index = AddLoopPreamble(ctx).Index;

                    linkedList.RemoveFirst();
                    linkedList.AddFirst(ctx.Element.EnterArray(index));
                    continue;
                }

                var request = new ElementBuildRequest()
                {
                    Element = ctx.Element,
                    AdditionalContext = ctx.AdditionalContext,
                    InputTypes = GetInputTypes(ctx, ctx.NormalizedType),
                    Provider = ctx.Provider,
                    G = ctx.G,
                };

                response = ctx.Chain.Process(request) as ElementBuildResponse;

                if (response == null)
                    throw new NotSupportedException();

                // So we have correctly send a request we have its reply.
                // Whoever handled the request had chance to:
                // 1) Modify the context in order to produce some instruction capable of 
                //    handling the request. We don't need to do anything else.
                // 2) Giving back a method that we have to call.
                //    If that is the case, we should rearrange the input and call the method.
                if (response.Status != BuildedFunctionStatus.ContextModified)
                {
                    HandleType(ctx);

                    bool callEmiter =
                        response.Status == BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized ||
                        response.Status == BuildedFunctionStatus.FunctionNotFinalized;

                    if (callEmiter)
                        ctx.G.Call(response.Emiter, null);
                    else if (method != null)
                        ctx.G.Call(response.Method, null);
                }

                if (ctx.LoopCtx.Count > 0)
                    AddLoopEpilogue(ctx);

                linkedList.RemoveFirst();
            }
        }
        #endregion

        #region Type management

        /// <summary>
        /// Manage 
        /// </summary>
        /// <param name="ctx">Context</param>
        private void HandleType(FoundryContext ctx)
        {
            if (ctx.ObjectType.IsByRef)
                ctx.Element.Load(ctx.G, TypeOfContent.Address);
            else
                ctx.Element.Load(ctx.G, TypeOfContent.Value);

            ForwardParameters(ctx, null);
        }
        #endregion

        #region Loop
        /// <summary>
        /// TODO: this should not be here -> FIX CA1822.
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
        private LoopContext AddLoopPreamble(FoundryContext ctx)
        {
            var currentLoopContext = new LoopContext(ctx.VariablePool.GetNewVariable(typeof(uint)));
            ctx.LoopCtx.Push(currentLoopContext);

            currentLoopContext.Body = ctx.G.DefineLabel($"Body_{ctx.Element.GetHashCode()}");
            currentLoopContext.CheckOutOfBound = ctx.G.DefineLabel($"OutOfBound_{ctx.Element.GetHashCode()}");
            
            Type indexType = ctx.Element.Attribute?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            if (ctx.IsDeserializing)
            {
                if (ctx.Element.Attribute?.ArrayFixedSize != -1)
                {
                    if (ctx.Element.Index != null)
                        throw new NotSupportedException("Fixed size arrays for multi-dimensional array is not supported.");

                    int size = ctx.Element.Attribute.ArrayFixedSize;
                    currentLoopContext.Size = (ConstantElement<int>)size;
                }
            }
            else
            {
                currentLoopContext.Size = ctx.Element.Lenght;
            }

            // Write in stream
            if (!ctx.IsDeserializing)
            {
                // Write the size of the array
                var request = new ElementBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    InputTypes = GetInputTypes(ctx, indexType),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as ElementBuildResponse;

                if (response.Status != BuildedFunctionStatus.ContextModified)
                {
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Value);

                    if (response.Status == BuildedFunctionStatus.TypeFinalized)
                        ForwardParameters(ctx, response);
                    else
                    {
                        ForwardParameters(ctx, null);
                        ctx.G.Call(response.Emiter);
                    }
                }
            }

            // Case #1: Noone created the Size variable; create a new one and expect to find its value
            //          in the stream.
            // Case #2: The Size variable was already initialized by someone else; Use it.
            else if (currentLoopContext.Size == null)
            {
                currentLoopContext.Size = ctx.VariablePool.GetNewVariable(indexType);

                var request = new ElementBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    InputTypes = GetInputTypes(ctx, indexType.MakeByRefType()),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as ElementBuildResponse;

                if (response.Status != BuildedFunctionStatus.ContextModified)
                {
                    // this.DecodeUInt(ref size, buffer, ref position);
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Address);
                    ForwardParameters(ctx, response);
                }
            }

            if (ctx.IsDeserializing)
            {
                // ObjectInstance.CurrentItemFieldInfo = new CurrentItemUnderlyingType[Size];
                var newArray = new GenericElement(((g, _) =>
                {
                    currentLoopContext.Size.Load(g, TypeOfContent.Value);
                    ctx.G.NewArray(ctx.Element.LoadedType.GetElementType());
                }), null);

                ctx.Element.Store(ctx.G, newArray, TypeOfContent.Value);
            }

            // int indexLocal = 0;
            // goto CheckOutOfBound;
            ctx.G.LoadConstant(0);
            ctx.G.StoreLocal(currentLoopContext.Index);
            ctx.G.Branch(currentLoopContext.CheckOutOfBound); // Local variables initialized, jump

            // Loop start
            ctx.G.MarkLabel(currentLoopContext.Body);

            return currentLoopContext;
        }

        private Type MakeDelegateType(Type objectType, Type[] inputTypes)
        {
            List<Type> arguments = new List<Type>(inputTypes.Length + 1);
            arguments.Add(objectType);

            for (int i = 1; i < inputTypes.Length; i++)
                arguments.Add(inputTypes[i]);
            arguments.Add(typeof(void));

            return Expression.GetDelegateType(arguments.ToArray());
        }

        /// <summary>
        /// TODO: this should not be here -> FIX CA1822.
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
        private void AddLoopEpilogue(FoundryContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");

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
                    ctx.G.LoadLength(ctx.Element.LoadedType);
                }
                else
                {
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Value);
                }

                ctx.G.BranchIfLess(currentLoopContext.Body);
                ctx.VariablePool.ReleaseVariable(currentLoopContext.Index);

                if (currentLoopContext.Size is LocalElement)
                    ctx.VariablePool.ReleaseVariable(currentLoopContext.Size as LocalElement);
            }
        }
        #endregion
    }
}
