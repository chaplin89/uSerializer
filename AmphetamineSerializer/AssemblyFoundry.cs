using AmphetamineSerializer.Chain;
using AmphetamineSerializer.FunctionProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Common;

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
                    ctx.Manipulator = new ILAbstraction(ctx.G);
                }
            }
            else
            {
                ctx.Provider = new PersistentFunctionProvider(ctx.ObjectType.Name + Guid.NewGuid());
                ctx.G = ctx.Provider.AddMethod("Handle", ctx.InputParameters, null);
                ctx.Manipulator = new ILAbstraction(ctx.G);
                isToPersist = true;
            }

            if (ctx.Chain == null)
            {
                var manager = new ChainManager()
                                  .SetNext(new CustomSerializerFinder())
                                  .SetNext(new CustomBuilderFinder())
                                  .SetNext(new DefaultHandlerFinder())
                                  .SetNext(new DefaultBuilder());

                ctx.Chain = manager;
            }
        }
        #endregion

        #region Private state
        /// <summary>
        /// If true, the class will save the generated assembly.
        /// </summary>
        bool isToPersist;

        /// <summary>
        /// Context for the current building process.
        /// </summary>
        FoundryContext ctx;

        /// <summary>
        /// Internally cached method.
        /// </summary>
        MethodInfo method;
        #endregion

        #region Public properties
        /// <summary>
        /// Expose the builded method.
        /// </summary>
        public MethodInfo Method
        {
            get
            {
                MethodInfo existentMethod = null;

                ctx.AlreadyBuildedMethods.TryGetValue(ctx.ObjectType, out existentMethod);

                if (existentMethod == null)
                    method = Make();

                if (existentMethod == null)
                    ctx.AlreadyBuildedMethods.Add(ctx.ObjectType, method);
                else
                    ctx.AlreadyBuildedMethods[ctx.ObjectType] = method;

                return method;
            }
        }

        public FoundryContext Context
        {
            get
            {
                return ctx;
            }

            set
            {
                ctx = value;
            }
        }

        #endregion

        #region Building related methods

        /// <summary>
        /// Generate the method for the current ObjectType.
        /// </summary>
        /// <returns>Builded method</returns>
        private MethodInfo Make()
        {
            Type normalizedType;
            int[] versions;
            if (ctx.InputParameters[0].IsByRef)
            {
                ctx.G.Emit(OpCodes.Ldarg_0);
                var ctor = ctx.ObjectType.GetElementType().GetConstructor(new Type[] { });
                ctx.ObjectInstance = ctx.G.DeclareLocal(ctx.ObjectType.GetElementType());
                ctx.G.Emit(OpCodes.Newobj, ctor);
                ctx.G.Emit(OpCodes.Stloc, ctx.ObjectInstance);
                ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance);
                ctx.G.Emit(OpCodes.Stind_Ref);
                normalizedType = ctx.ObjectType.GetElementType();
            }
            else
            {
                ctx.ObjectInstance = ctx.G.DeclareLocal(ctx.ObjectType);
                ctx.G.Emit(OpCodes.Ldarg_0);
                ctx.G.Emit(OpCodes.Stloc, ctx.ObjectInstance);
                normalizedType = ctx.ObjectType;
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
                    labels[i] = ctx.G.DefineLabel();

                Type requestType = field.FieldType;
                if (ctx.ObjectType.IsByRef)
                    requestType = requestType.MakeByRefType();

                var request = new SerializationBuildRequest()
                {
                    AdditionalContext = ctx.AdditionalContext,
                    DelegateType = ctx.Manipulator.MakeDelegateType(requestType, ctx.InputParameters)
                };
                var response = ctx.Chain.Process(request) as SerializationBuildResponse;
                var targetMethod = response.Method;

                if (ctx.ObjectType.IsByRef)
                {
                    LocalBuilder local = ctx.G.DeclareLocal(field.FieldType);
                    ctx.G.Emit(OpCodes.Ldloca, local);
                    ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.CurrentAttribute);
                    ctx.G.Emit(OpCodes.Ldloc, local);
                    ctx.Manipulator.EmitStoreObject(ctx.ObjectInstance, field, local);
                }
                else
                {
                    ctx.Manipulator.EmitAccessObject(ctx.ObjectInstance, field);
                    ctx.Manipulator.ForwardParameters(ctx.InputParameters, targetMethod, ctx.CurrentAttribute);
                    ctx.Manipulator.EmitAccessObject(ctx.ObjectInstance, field);
                }

                // Deserialize versions
                ctx.G.Emit(OpCodes.Ldc_I4, versions[0]);
                ctx.G.Emit(OpCodes.Sub);
                ctx.G.Emit(OpCodes.Switch, labels);

                for (int i = 0; i < versions.Length; i++)
                {
                    var fields = VersionHelper.GetVersionSnapshot(normalizedType, versions[i]).Where(x => x.Name.ToLower() != "version");
                    ctx.G.MarkLabel(labels[i]);
                    BuildFromFields(ctx, fields);
                    ctx.G.Emit(OpCodes.Ret);
                }
            }
            else
            {
                BuildFromFields(ctx, VersionHelper.GetAllFields(normalizedType));
            }

            ctx.G.Emit(OpCodes.Ret);
            return ctx.Provider.GetMethod(isToPersist);
        }

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Fields to manage</param>
        private void BuildFromFields(FoundryContext ctx, IEnumerable<FieldInfo> fields)
        {
            ctx.Index = ctx.G.DeclareLocal(typeof(int));

            foreach (var item in fields)
            {
                // todo:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IList (excluing List ofc)
                LoopContext loopCtx = null;

                ctx.CurrentItemFieldInfo = item;
                ctx.CurrentItemType = item.FieldType;
                ctx.CurrentItemUnderlyingType = item.FieldType;

                if (ctx.CurrentItemUnderlyingType.IsInterface || ctx.CurrentItemUnderlyingType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                if (ctx.CurrentItemType.IsArray)
                    loopCtx = ManageArray(ctx);

                Type currentType = ctx.CurrentItemUnderlyingType;

                if (ctx.CurrentItemUnderlyingType.IsEnum)
                    currentType = currentType.GetEnumUnderlyingType();

                Type requestType = currentType;
                if (ctx.ObjectType.IsByRef)
                    requestType = requestType.MakeByRefType();

                var request = new SerializationBuildRequest()
                {
                    AdditionalContext = ctx,
                    DelegateType = ctx.Manipulator.MakeDelegateType(requestType, ctx.InputParameters)
                };

                SerializationBuildResponse response;
                using (var status = new StatusSaver(ctx))
                    response = ctx.Chain.Process(request) as SerializationBuildResponse;

                HandleType(ctx, response.Method);

                if (ctx.CurrentItemType.IsArray)
                    ctx.Manipulator.AddLoopEpilogue(loopCtx);
            }
        }

        private LoopContext ManageArray(FoundryContext ctx)
        {
            LoopContext loopCtx;
            ctx.CurrentItemUnderlyingType = ctx.CurrentItemFieldInfo.FieldType.GetElementType();

            LocalBuilder currentSize = null;

            if (ctx.ObjectType.IsByRef && ctx.CurrentAttribute.ArrayFixedSize != -1)
            {
                currentSize = ctx.G.DeclareLocal(typeof(int));
                ctx.G.Emit(OpCodes.Ldc_I4, ctx.CurrentAttribute.ArrayFixedSize);
                ctx.G.Emit(OpCodes.Stloc, currentSize);
            }

            loopCtx = LoopContext.FromFoundryContext(ctx);
            loopCtx.Size = currentSize;

            ctx.Manipulator.AddLoopPreamble(ref loopCtx);
            return loopCtx;
        }
        #endregion

        #region Type management

        /// <summary>
        /// Manage a non-trivial type.
        /// A non-trivial type is a type for which a deserializing function is not know.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <remarks>
        /// In order to produce istructions for deserialize a non-trivial type, 
        /// the function will modify the context and recursively create other foundries.
        /// </remarks>
        private void HandleType(FoundryContext ctx, MethodInfo method)
        {
            // We are moving in an inner level of the graph;
            // Save the state here and restore when we are done.
            if (!ctx.CurrentItemType.IsArray)
            {
                ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance);                              // this --> stack
                if (ctx.ObjectType.IsByRef)
                    ctx.G.Emit(OpCodes.Ldflda, ctx.CurrentItemFieldInfo);
                else
                    ctx.G.Emit(OpCodes.Ldfld, ctx.CurrentItemFieldInfo);
            }
            else
            {
                ctx.G.Emit(OpCodes.Ldloc, ctx.ObjectInstance);                                          // this       --> stack
                ctx.G.Emit(OpCodes.Ldfld, ctx.CurrentItemFieldInfo);                                    // field      --> stack
                ctx.G.Emit(OpCodes.Ldloc, ctx.Index);                                                   // indexLocal --> stack
                if (ctx.ObjectType.IsByRef)
                    ctx.G.Emit(OpCodes.Ldelema, ctx.CurrentItemUnderlyingType);                         // stack      --> arraylocal[indexLocal]
                else
                    ctx.G.Emit(OpCodes.Ldelem, ctx.CurrentItemUnderlyingType);
            }
            ctx.Manipulator.ForwardParameters(ctx.InputParameters, null, ctx.CurrentAttribute);
            ctx.G.EmitCall(OpCodes.Call, method, null);
        }
        #endregion
    }
}
