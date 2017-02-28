using AmphetamineSerializer.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmphetamineSerializer.Common;
using Sigil;
using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Common.Element;
using Sigil.NonGeneric;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Make new assemblies.
    /// </summary>
    public class AssemblyFoundry : BuilderBase
    {
        #region ctor
        /// <summary>
        /// Build an AssemblyFoundry object.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="persist"></param>
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

            ctx.Element = new FieldElementInfo();

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

        #region Public Methods
        /// <summary>
        /// Expose the builded method.
        /// </summary>
        public override BuildedFunction Make()
        {
            if (method == null)
                method = InternalMake();

            return method;
        }

        #endregion

        #region Building related methods

        /// <summary>
        /// Generate the method for the current ObjectType.
        /// </summary>
        /// <returns>Builded method</returns>
        private BuildedFunction InternalMake()
        {
            Type normalizedType;
            int[] versions;
            ctx.Element = new FieldElementInfo();

            if (ctx.ManageLifeCycle)
            {
                normalizedType = ctx.ObjectType.GetElementType();
                ctx.G.LoadArgument(0);
                var ctor = normalizedType.GetConstructor(new Type[] { });

                if (ctor == null)
                    throw new NotSupportedException($"The type {normalizedType.Name} does not have a parameterless constructor.");

                LocalElement instance = ctx.G.DeclareLocal(normalizedType);
                ctx.Element.Instance = instance;

                GenericElementInfo load = (GenericElementInfo)((g, content)=>
                {
                    if (content == TypeOfContent.Address)
                        throw new InvalidOperationException("Unable to return the content by address.");

                    g.NewObject(ctx.ObjectType.GetElementType());
                });

                instance.Store(ctx.G, load, TypeOfContent.Value);
                instance.Load(ctx.G, TypeOfContent.Value);
                ctx.G.StoreIndirect(normalizedType);
            }
            else
            {
                normalizedType = ctx.ObjectType;
                ctx.Element.Instance = (LocalElement)ctx.G.DeclareLocal(ctx.ObjectType);
                ctx.G.LoadArgument(0);
                ctx.G.StoreLocal((LocalElement)ctx.Element.Instance);
            }

            versions = VersionHelper.GetExplicitlyManagedVersions(normalizedType).ToArray();

            if (versions.Length > 1)
                ManageVersions(ctx, versions, normalizedType);
            else
                BuildFromFields(ctx, VersionHelper.GetAllFields(normalizedType));

            ctx.G.Return();
            return ctx.Provider.GetMethod();
        }

        private void ManageVersions(FoundryContext ctx, int[] versions, Type normalizedType)
        {
            Label[] labels = null;
            var field = VersionHelper.GetAllFields(normalizedType).Where(x => x.Name.ToLower() == "version").Single();
            if (VersionHelper.GetAllFields(normalizedType).First() != field)
                throw new InvalidOperationException("The version field should be the first.");

            labels = new Label[versions.Length];

            for (int i = 0; i < labels.Length; i++)
                labels[i] = ctx.G.DefineLabel($"Version{i}");

            Type requestType = field.FieldType;
            if (ctx.ManageLifeCycle)
                requestType = requestType.MakeByRefType();

            var request = new SerializationBuildRequest()
            {
                Element = ctx.Element,
                AdditionalContext = ctx.AdditionalContext,
                DelegateType = ctx.Manipulator.MakeDelegateType(requestType, ctx.InputParameters),
                Provider = ctx.Provider,
                G = ctx.G
            };

            var response = ctx.Chain.Process(request) as SerializationBuildResponse;
            var targetMethod = response.Response;

            if (ctx.ObjectType.IsByRef)
            {
                ctx.G.LoadLocal((LocalElement)ctx.Element.Instance); // this --> stack
                ctx.G.LoadFieldAddress(field); // &this.CurrentItem --> stack
                ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.Element.CurrentAttribute);

            }
            else
            {
                ctx.G.LoadLocal((LocalElement)ctx.Element.Instance); // this --> stack
                ctx.G.LoadField(field); // this.CurrentItem --> stack
                ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.Element.CurrentAttribute);
            }

            ctx.G.LoadLocal((LocalElement)ctx.Element.Instance); // this --> stack
            ctx.G.LoadField(field); // this.CurrentItem --> stack

            // Deserialize versions
            ctx.G.LoadConstant(versions[0]);
            ctx.G.Subtract();
            ctx.G.Switch(labels);

            for (int i = 0; i < versions.Length; i++)
            {
                var fields = VersionHelper.GetVersionSnapshot(normalizedType, versions[i]).Where(x => x.Name.ToLower() != "version");
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
        private void BuildFromFields(FoundryContext ctx, IEnumerable<FieldInfo> fields)
        {
            foreach (var item in fields)
            {
                // todo:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IList (excluing List ofc)
                SerializationBuildResponse response = null;

                ctx.Element.Field = item;

                // TODO: IsInterface is needed if IsAbstract is true?
                if (ctx.Element.Field.FieldType.IsInterface || ctx.Element.Field.FieldType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                if (ctx.Element.Field.FieldType.IsArray)
                    ManageArray(ctx);

                var request = new SerializationBuildRequest()
                {
                    Element = ctx.Element,
                    AdditionalContext = ctx.AdditionalContext,
                    DelegateType = ctx.Manipulator.MakeDelegateType(ctx.NormalizedType, ctx.InputParameters),
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                using (var status = new StatusSaver(ctx))
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

                if (ctx.Element.Field.FieldType.IsArray)
                    ctx.Manipulator.AddLoopEpilogue(ctx);
            }
        }

        private void ManageArray(FoundryContext ctx)
        {
            var currentLoopContext = new LoopContext()
            {
                Index = ctx.G.DeclareLocal(typeof(int))
            };

            if (ctx.ObjectType.IsByRef && ctx.Element.CurrentAttribute.ArrayFixedSize != -1)
            {
                currentLoopContext.Size = ctx.G.DeclareLocal(typeof(int));
                ctx.G.LoadConstant(ctx.Element.CurrentAttribute.ArrayFixedSize);
                ctx.G.StoreLocal(currentLoopContext.Size);
            }

            // TODO: REPLACE THIS WITH INDEX!!
            // ctx.Element.LoopCtx = currentLoopContext;

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
            // TODO: SET INDEX?!
            if (ctx.ObjectType.IsByRef)
                ctx.Element.Load(ctx.G, TypeOfContent.Address);
            else
                ctx.Element.Load(ctx.G, TypeOfContent.Value);

            ctx.Manipulator.ForwardParameters(ctx.InputParameters, null, ctx.Element.CurrentAttribute);
        }
        #endregion
    }
}
