using AmphetamineSerializer.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Common;
using Sigil;

namespace AmphetamineSerializer
{
    /// <summary>
    /// Make new assemblies.
    /// </summary>
    public class AssemblyFoundry : IBuilder
    {
        #region ctor
        /// <summary>
        /// Build an AssemblyFoundry object.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="persist"></param>
        public AssemblyFoundry(FoundryContext ctx)
        {
            this.ctx = ctx;

            if (ctx.Provider != null)
            {
                if (ctx.Provider.AlreadyBuildedMethods.ContainsKey(ctx.ObjectType))
                {
                    method = ctx.Provider.AlreadyBuildedMethods[ctx.ObjectType];
                }
                else
                {
                    List<Type> input = new List<Type>() { ctx.ObjectType };
                    for (int i = 1; i < ctx.InputParameters.Length; i++)
                    {
                        input.Add(ctx.InputParameters[i]);
                    }
                    ctx.G = ctx.Provider.AddMethod("Handle", input.ToArray(), null);
                }
            }
            else
            {
                ctx.Provider = new SigilFunctionProvider($"{ctx.ObjectType.Name}_{Guid.NewGuid()}");
                ctx.G = ctx.Provider.AddMethod("Handle", ctx.InputParameters, null);
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

        #region Private state

        /// <summary>
        /// Context for the current building process.
        /// </summary>
        FoundryContext ctx;

        /// <summary>
        /// Internally cached method.
        /// </summary>
        BuildedFunction method;
        #endregion

        #region Public properties
        /// <summary>
        /// Expose the builded method.
        /// </summary>
        public BuildedFunction Method
        {
            get
            {
                if (method == null)
                    method = Make();

                return method;
            }
        }

        #endregion

        #region Building related methods

        /// <summary>
        /// Generate the method for the current ObjectType.
        /// </summary>
        /// <returns>Builded method</returns>
        private BuildedFunction Make()
        {
            Type normalizedType;
            int[] versions;
            if (ctx.ManageLifeCycle)
            {
                normalizedType = ctx.ObjectType.GetElementType();
                ctx.G.LoadArgument(0);
                var ctor = normalizedType.GetConstructor(new Type[] { });
                ctx.ObjectInstance = ctx.G.DeclareLocal(normalizedType);
                ctx.G.NewObject(ctx.ObjectType.GetElementType());
                ctx.G.StoreLocal(ctx.ObjectInstance);
                ctx.G.LoadLocal(ctx.ObjectInstance);
                ctx.G.StoreIndirect(normalizedType);
            }
            else
            {
                normalizedType = ctx.ObjectType;
                ctx.ObjectInstance = ctx.G.DeclareLocal(ctx.ObjectType);
                ctx.G.LoadArgument(0);
                ctx.G.StoreLocal(ctx.ObjectInstance);
            }

            versions = VersionHelper.GetExplicitlyManagedVersions(normalizedType).ToArray();
            Label[] labels = null;
            if (versions.Length > 1)
            {
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
                    AdditionalContext = ctx,
                    DelegateType = ctx.Manipulator.MakeDelegateType(requestType, ctx.InputParameters)
                };
                var response = ctx.Chain.Process(request) as SerializationBuildResponse;
                var targetMethod = response.Method;

                if (ctx.ObjectType.IsByRef)
                {
                    Local local = ctx.G.DeclareLocal(field.FieldType);
                    ctx.G.LoadLocalAddress(local);
                    ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.CurrentAttribute);
                    ctx.G.LoadLocal(local);
                    ctx.Manipulator.EmitStoreObject(ctx.ObjectInstance, field, local);
                }
                else
                {
                    ctx.Manipulator.EmitAccessObject(ctx.ObjectInstance, field);
                    ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.CurrentAttribute);
                    ctx.Manipulator.EmitAccessObject(ctx.ObjectInstance, field);
                }

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
            else
            {
                BuildFromFields(ctx, VersionHelper.GetAllFields(normalizedType));
            }

            ctx.G.Return();
            return ctx.Provider.GetMethod();
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

                ctx.CurrentItemFieldInfo = item;
                ctx.CurrentItemType = item.FieldType;
                ctx.CurrentItemUnderlyingType = item.FieldType;

                if (ctx.CurrentItemUnderlyingType.IsInterface || ctx.CurrentItemUnderlyingType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                if (ctx.CurrentItemType.IsArray)
                    ManageArray(ctx);

                var request = new SerializationBuildRequest()
                {
                    AdditionalContext = ctx,
                    DelegateType = ctx.Manipulator.MakeDelegateType(ctx.NormalizedType, ctx.InputParameters)
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
                if (response.Method.Status != BuildedFunctionStatus.NoMethodsAvailable)
                {
                    HandleType(ctx);

                    bool callEmiter =
                        response.Method.Status == BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized ||
                        response.Method.Status == BuildedFunctionStatus.FunctionNotFinalized;

                    if (callEmiter)
                        ctx.G.Call(response.Method.Emiter, null);
                    else
                        ctx.G.Call(response.Method.Method, null);
                }

                if (ctx.CurrentItemType.IsArray)
                    ctx.Manipulator.AddLoopEpilogue(ctx);
            }
        }

        private void ManageArray(FoundryContext ctx)
        {
            ctx.CurrentItemUnderlyingType = ctx.CurrentItemFieldInfo.FieldType.GetElementType();

            var currentLoopContext = new LoopContext()
            {
                Index = ctx.G.DeclareLocal(typeof(int))
            };

            ctx.LoopCtx.Push(currentLoopContext);


            if (ctx.ObjectType.IsByRef && ctx.CurrentAttribute.ArrayFixedSize != -1)
            {
                currentLoopContext.Size = ctx.G.DeclareLocal(typeof(int));
                ctx.G.LoadConstant(ctx.CurrentAttribute.ArrayFixedSize);
                ctx.G.StoreLocal(currentLoopContext.Size);
            }

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
            if (!ctx.CurrentItemType.IsArray)
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);                              // this --> stack
                if (ctx.ObjectType.IsByRef)
                    ctx.G.LoadFieldAddress(ctx.CurrentItemFieldInfo);
                else
                    ctx.G.LoadField(ctx.CurrentItemFieldInfo);
            }
            else
            {
                ctx.G.LoadLocal(ctx.ObjectInstance);                         // this       --> stack
                ctx.G.LoadField(ctx.CurrentItemFieldInfo);                   // field      --> stack
                ctx.G.LoadLocal(ctx.LoopCtx.Peek().Index);                                  // indexLocal --> stack
                if (ctx.ObjectType.IsByRef)
                    ctx.G.LoadElementAddress(ctx.CurrentItemUnderlyingType); // stack      --> arraylocal[indexLocal]
                else
                    ctx.G.LoadElement(ctx.CurrentItemUnderlyingType);
            }

            ctx.Manipulator.ForwardParameters(ctx.InputParameters, null, ctx.CurrentAttribute);
        }
        #endregion
    }
}
