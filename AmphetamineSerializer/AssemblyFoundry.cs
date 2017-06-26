using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Element;
using Sigil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

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
            if (ctx.Provider != null && ctx.Provider.AlreadyBuildedMethods.ContainsKey(ctx.ObjectType))
            {
                method = ctx.Provider.AlreadyBuildedMethods[ctx.ObjectType];
            }
            else
            {
                if (ctx.Provider == null)
                    ctx.Provider = new SigilFunctionProvider($"{ctx.ObjectType.Name}_{Guid.NewGuid()}");
                ctx.G = ctx.Provider.AddMethod("Handle", ctx.InputParameters, typeof(void));
            }

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
        protected override BuildedFunction InternalMake()
        {
            Type normalizedType;

            ArgumentElement instance = new ArgumentElement(0, ctx.ObjectType);

            if (ctx.WriteIntoObject)
            {
                normalizedType = ctx.ObjectType.GetElementType();
                var ctor = normalizedType.GetConstructor(new Type[] { });

                if (ctor == null)
                    throw new NotSupportedException($"The type {normalizedType.Name} does not have a parameterless constructor.");

                var load = new GenericElement(((g, _) => g.NewObject(normalizedType)), null);

                instance.Store(ctx.G, load, TypeOfContent.Value);
            }
            else
            {
                normalizedType = ctx.ObjectType;
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

            if (versionField.Field.Name.ToLowerInvariant() != "version")
                throw new InvalidOperationException("The version field should be the first.");

            for (int i = 0; i < labels.Length; i++)
                labels[i] = ctx.G.DefineLabel($"Version_{i}");

            Type requestType = versionField.LoadedType;
            if (ctx.WriteIntoObject)
                requestType = requestType.MakeByRefType();

            var request = new SerializationBuildRequest()
            {
                Element = versionField,
                AdditionalContext = ctx.AdditionalContext,
                DelegateType = ctx.Manipulator.MakeDelegateType(requestType, ctx.InputParameters),
                Provider = ctx.Provider,
                G = ctx.G
            };

            var response = ctx.Chain.Process(request) as SerializationBuildResponse;
            var targetMethod = response.Response;

            if (targetMethod.Status != BuildedFunctionStatus.ContextModified)
            {
                if (ctx.WriteIntoObject)
                    versionField.Load(ctx.G, TypeOfContent.Address);
                else
                    versionField.Load(ctx.G, TypeOfContent.Value);

                ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, versionField.Attribute);
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
                                              .Where(x => x.Field.Name.ToLowerInvariant() != "version");
                    ctx.G.MarkLabel(labels[i]);
                    BuildFromFields(ctx, fields);
                    ctx.G.Return();
                }
            }
            else if(versionField.Field.FieldType.IsAssignableFrom(typeof(IEquatable<>)))
            {
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
                ctx.Element = (FieldElement)linkedList.First.Value;
                SerializationBuildResponse response = null;

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

                var request = new SerializationBuildRequest()
                {
                    Element = ctx.Element,
                    AdditionalContext = ctx.AdditionalContext,
                    DelegateType = ctx.Manipulator.MakeDelegateType(ctx.NormalizedType, ctx.InputParameters),
                    Provider = ctx.Provider,
                    G = ctx.G,
                    RequestType = TypeOfRequest.Everything
                };

                response = ctx.Chain.Process(request) as SerializationBuildResponse;

                if (response == null)
                    throw new NotSupportedException();

                // So we have correctly send a request we have its reply.
                // Whoever handled the request had chance to:
                // 1) Modify the context in order to produce some instruction capable of 
                //    handling the request. We don't need to do anything else.
                // 2) Giving back a method that we have to call.
                //    If that is the case, we should rearrange the input and call the method.
                if (response.Response.Status != BuildedFunctionStatus.ContextModified)
                {
                    HandleType(ctx);

                    bool callEmiter =
                        response.Response.Status == BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized ||
                        response.Response.Status == BuildedFunctionStatus.FunctionNotFinalized;

                    if (callEmiter)
                        ctx.G.Call(response.Response.Emiter, null);
                    else if (method != null)
                        ctx.G.Call(response.Response.Method, null);
                    else
                        ctx.G.Call(response.Response.Delegate.Method, null);
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

            ctx.Manipulator.ForwardParameters(ctx.InputParameters, null, ((FieldElement)ctx.Element).Attribute);
        }
        #endregion

        #region Loop
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
        public LoopContext AddLoopPreamble(FoundryContext ctx)
        {
            Contract.Ensures(ctx != null);

            var currentLoopContext = new LoopContext(ctx.VariablePool.GetNewVariable(typeof(uint)));
            ctx.LoopCtx.Push(currentLoopContext);

            currentLoopContext.Body = ctx.G.DefineLabel($"Body_{ctx.Element.GetHashCode()}");
            currentLoopContext.CheckOutOfBound = ctx.G.DefineLabel($"OutOfBound_{ctx.Element.GetHashCode()}");

            var fieldElement = ctx.Element as FieldElement;

            Type indexType = ((FieldElement)ctx.Element).Attribute?.SizeType;
            if (indexType == null)
                indexType = typeof(uint);

            if (ctx.WriteIntoObject)
            {
                if (fieldElement.Attribute.ArrayFixedSize != -1)
                {
                    if (ctx.Element.Index != null)
                        throw new NotSupportedException("Fixed size arrays for multi-dimensional array is not supported.");

                    int size = fieldElement.Attribute.ArrayFixedSize;
                    currentLoopContext.Size = (ConstantElement<int>)size;
                }
            }
            else
            {
                currentLoopContext.Size = ctx.Element.Lenght;
            }

            // Write in stream
            if (!ctx.WriteIntoObject)
            {
                // Write the size of the array
                var request = new SerializationBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    DelegateType = ctx.Manipulator.MakeDelegateType(indexType, ctx.InputParameters),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                if (response.Response.Status != BuildedFunctionStatus.ContextModified)
                {
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Value);

                    if (response.Response.Status == BuildedFunctionStatus.TypeFinalized)
                        ctx.Manipulator.ForwardParameters(ctx.InputParameters, response.Response);
                    else
                    {
                        ctx.Manipulator.ForwardParameters(ctx.InputParameters, null);
                        ctx.G.Call(response.Response.Emiter);
                    }
                }
            }

            // Case #1: Noone created the Size variable; create a new one and expect to find its value
            //          in the stream.
            // Case #2: The Size variable was already initialized by someone else; Use it.
            else if (currentLoopContext.Size == null)
            {
                currentLoopContext.Size = ctx.VariablePool.GetNewVariable(indexType);

                var request = new SerializationBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    DelegateType = ctx.Manipulator.MakeDelegateType(indexType.MakeByRefType(), ctx.InputParameters),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                var response = ctx.Chain.Process(request) as SerializationBuildResponse;

                if (response.Response.Status != BuildedFunctionStatus.ContextModified)
                {
                    // this.DecodeUInt(ref size, buffer, ref position);
                    currentLoopContext.Size.Load(ctx.G, TypeOfContent.Address);
                    ctx.Manipulator.ForwardParameters(ctx.InputParameters, response.Response);
                }
            }

            if (ctx.WriteIntoObject)
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

                ctx.Manipulator.IncrementLocalVariable(currentLoopContext.Index);

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
