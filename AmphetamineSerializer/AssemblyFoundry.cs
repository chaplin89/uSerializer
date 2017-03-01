using AmphetamineSerializer.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using AmphetamineSerializer.Common;
using Sigil;
using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Common.Element;

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

            ArgumentElement instance = new ArgumentElement(0) {RootType = ctx.ObjectType };

            if (ctx.ManageLifeCycle)
            {
                normalizedType = ctx.ObjectType.GetElementType();
                var ctor = normalizedType.GetConstructor(new Type[] { });

                if (ctor == null)
                    throw new NotSupportedException($"The type {normalizedType.Name} does not have a parameterless constructor.");

                var load = (GenericElement)((g, _) => g.NewObject(normalizedType));

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
                BuildFromFields(ctx, VersionHelper.GetAllFields(instance, normalizedType));

            ctx.G.Return();
            return ctx.Provider.GetMethod();
        }

        private void ManageVersions(FoundryContext ctx, IElement instance, int[] versions, Type normalizedType)
        {
            Label[] labels = new Label[versions.Length];
            var versionField = VersionHelper.GetAllFields(instance, normalizedType).First();

            if (versionField.Field.Name.ToLowerInvariant() != "version")
                throw new InvalidOperationException("The version field should be the first.");

            for (int i = 0; i < labels.Length; i++)
                labels[i] = ctx.G.DefineLabel($"Version_{i}");

            Type requestType = versionField.Field.FieldType;
            if (ctx.ManageLifeCycle)
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

            if (targetMethod != null)
            {
                if (ctx.ManageLifeCycle)
                    versionField.Load(ctx.G, TypeOfContent.Address);
                else
                    versionField.Load(ctx.G, TypeOfContent.Value);

                ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, versionField.Attribute);
            }

            // Enter switch case
            versionField.Load(ctx.G, TypeOfContent.Value);
            ctx.G.LoadConstant(versions[0]);
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

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Fields to manage</param>
        private void BuildFromFields(FoundryContext ctx, IEnumerable<IElement> fields)
        {
            var linkedList = new LinkedList<IElement>(fields);

            while(linkedList.Count > 0)
            {
                ctx.Element = (FieldElement)linkedList.First.Value;

                // todo:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IEnumerable (excluing List ofc)
                SerializationBuildResponse response = null;

                if (ctx.Element.ElementType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                if (ctx.Element.ElementType.IsArray)
                {
                    ManageArray(ctx);
                    continue;
                }

                var request = new SerializationBuildRequest()
                {
                    Element = ctx.Element,
                    AdditionalContext = ctx.AdditionalContext,
                    DelegateType = ctx.Manipulator.MakeDelegateType(ctx.NormalizedType, ctx.InputParameters),
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                // TODO: THERE AREN'T REALLY ANY GOOD REASON FOR MAKING AssemblyFoundry PART OF THE CHAIN.
                //       THIS IS ONLY WASTING SPACE ON THE STACK.
                //       AssemblyFoundry SHOULD SEND A REQUEST AND IF THE RESPONSE IS NULL, IT SHOULD TRY TO HANDLE
                //       THE REQUEST BY ITSELF PUTTING THE REQUEST IN A LIFO QUEUE.
                response = ctx.Chain.Process(request) as SerializationBuildResponse;

                if (response == null)
                    throw new InvalidOperationException($"Unable to find an handler for type {ctx.NormalizedType}");

                // So we have correctly send a request we have its reply.
                // Whoever handled the request had chance to:
                // 1) Modify the context in order to produce some instruction capable of 
                //    handling the request. We don't need to do anything else.
                // 2) Giving back a method that we have to call.
                //    If that is the case, we should rearrange the input and call the method.
                if (response.Response.Status != BuildedFunctionStatus.NoMethodsAvailable)
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

                if (ctx.Element.RootType.IsArray)
                    ctx.Manipulator.AddLoopEpilogue(ctx);

                linkedList.RemoveFirst();
            }
        }

        private void ManageArray(FoundryContext ctx)
        {
            var currentLoopContext = new LoopContext(ctx.G.DeclareLocal(typeof(int)));

            ctx.Element.ElementType = ctx.Element.ElementType.GetElementType();

            if (ctx.ObjectType.IsByRef && ((FieldElement)ctx.Element).Attribute.ArrayFixedSize != -1)
            {
                if (ctx.Element.Index != null)
                    throw new NotSupportedException("Fixed size arrays for multi-dimensional array is not supported.");

                int size = ((FieldElement)ctx.Element).Attribute.ArrayFixedSize;
                currentLoopContext.Size = (ConstantElement<int>)size;
            }

            if (ctx.Element.Index == null)
            {
                ctx.Element.Index = (LocalElement)currentLoopContext.Index;
            }
            else
            {
                var currentIndex = ctx.Element.Index;

                ctx.Element.Index = (GenericElement)((g, _) =>
                {
                    currentIndex.Load(g, TypeOfContent.Value);
                    g.LoadElementAddress(ctx.Element.ElementType);
                    g.LoadLocal(currentLoopContext.Index);
                });
            }
            
            ctx.LoopCtx.Push(currentLoopContext);
            ctx.Manipulator.AddLoopPreamble(ctx);
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
    }
}
