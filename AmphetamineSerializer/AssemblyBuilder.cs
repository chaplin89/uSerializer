using AmphetamineSerializer.Nodes;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
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
    public class AssemblyBuilder : BuilderBase
    {
        #region ctor
        /// <summary>
        /// Build an AssemblyBuilder object starting from a context.
        /// </summary>
        /// <param name="ctx">Context</param>
        public AssemblyBuilder(Context ctx) : base(ctx)
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
                ctx.Chain = new ChainManager().SetNext(new DefaultHandlerFinder());
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
                normalizedType = normalizedType.GetElementType();
                CreateAndAssignNewInstance(normalizedType, instance);
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

        private void CreateAndAssignNewInstance(Type typeToCreate, IElement instance)
        {
            if (typeToCreate.GetConstructor(Type.EmptyTypes) == null)
                throw new NotSupportedException($"The type {typeToCreate.Name} does not have a parameterless constructor.");

            var newInstance = new GenericElement(((g, _) => g.NewObject(typeToCreate)), null);

            instance.Store(ctx.G, newInstance, TypeOfContent.Value);
        }

        private void ManageVersions(Context ctx, IElement instance, object[] versions, Type normalizedType)
        {
            Label[] labels = new Label[versions.Length];
            var versionField = VersionHelper.GetAllFields(instance, normalizedType).First();

            if (versionField.Field.Name.ToUpperInvariant() != "version")
                throw new InvalidOperationException("The version field should be the first.");

            for (int i = 0; i < labels.Length; i++)
                labels[i] = ctx.G.DefineLabel($"Version_{i}");

            Type versionType = versionField.LoadedType;
            if (ctx.IsDeserializing)
                versionType = versionType.MakeByRefType();

            var request = new ElementBuildRequest()
            {
                Element = versionField,
                AdditionalContext = ctx.AdditionalContext,
                InputTypes = GetInputTypes(ctx, versionType),
                Provider = ctx.Provider,
                G = ctx.G
            };

            var response = ctx.Chain.Process(request) as ElementBuildResponse;
            ManageReponse(ctx, response, versionField);

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
            else if (versionField.Field.FieldType.IsAssignableFrom(typeof(IEquatable<>)))
            {
                // TODO: Manage non numeric versions
            }
        }

        private Type[] GetInputTypes(Context ctx, Type overrideRootType)
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
        /// Increment a local variable
        /// </summary>
        /// <param name="index">Variable to increment</param>
        private void IncrementLocalVariable(Local index)
        {
            // C# Translation:
            //     index+=step;
            ctx.G.LoadLocal(index);
            ctx.G.LoadConstant(1);
            ctx.G.Add();
            ctx.G.StoreLocal(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        private void ManageReponse(Context ctx, ElementBuildResponse response, IElement firstElement)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (response.Status == BuildedFunctionStatus.ContextModified)
                return;

            if (ctx.IsDeserializing)
                firstElement.Load(ctx.G, TypeOfContent.Address);
            else
                firstElement.Load(ctx.G, TypeOfContent.Value);

            for (ushort j = 1; j < ctx.InputParameters.Length; j++)
                ctx.G.LoadArgument(j);

            if (response.Status == BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized)
            {
                ctx.G.Call(response.Emiter);
                return;
            }
            else if (response.Status == BuildedFunctionStatus.TypeFinalized)
            {
                if (response.Method.IsStatic)
                    ctx.G.Call(response.Method);
                else
                    ctx.G.CallVirtual(response.Method);
                return;
            }

            throw new InvalidOperationException("Can't forward parameters.");
        }

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Fields to manage</param>
        private void BuildFromFields(Context ctx, IEnumerable<IElement> fields)
        {
            var linkedList = new LinkedList<IElement>(fields);

            while (linkedList.Count > 0)
            {
                ctx.CurrentElement = linkedList.First.Value;
                ElementBuildResponse response = null;

                // TODO:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IEnumerable (excluding List ofc)
                if (ctx.CurrentElement.LoadedType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                // Indexable; just generate the loop preamble and skip to element @ "Index" of the inner type.
                if (ctx.CurrentElement.IsIndexable)
                {
                    var index = AddLoopPreamble(ctx).Index;

                    linkedList.RemoveFirst();
                    linkedList.AddFirst(ctx.CurrentElement.EnterArray(index));
                    continue;
                }

                // Not indexable; proceed with the request.
                var request = new ElementBuildRequest()
                {
                    Element = ctx.CurrentElement,
                    AdditionalContext = ctx.AdditionalContext,
                    InputTypes = GetInputTypes(ctx, GetNormalizedType(ctx)),
                    Provider = ctx.Provider,
                    G = ctx.G,
                };

                response = ctx.Chain.Process(request) as ElementBuildResponse;
                ManageReponse(ctx, response, ctx.CurrentElement);

                if (ctx.LoopCtx.Count > 0)
                    AddLoopEpilogue(ctx);

                linkedList.RemoveFirst();
            }
        }
        #endregion

        #region Type management

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentElement"></param>
        /// <returns></returns>
        private Type GetNormalizedType(Context ctx)
        {
            Type normalizedType = ctx.CurrentElement.LoadedType;

            if (normalizedType.IsEnum)
                normalizedType = normalizedType.GetEnumUnderlyingType();

            if (ctx.IsDeserializing)
                normalizedType = normalizedType.MakeByRefType();

            return normalizedType;
        }
        #endregion

        #region Loop
        /// <summary>
        /// TODO: this should not be here -> FIX CA1822.
        /// Generate a loop preamble:
        ///     1. Initialize the index
        ///     2. Jump for checking if current index is out of bound
        ///     3. Mark the begin of the loop's body
        /// </summary>
        /// <param name="ctx">Context.</param>
        /// <returns>The generated loop context.</returns>
        private LoopContext AddLoopPreamble(Context ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");

            var currentLoopContext = new LoopContext(ctx.VariablePool.GetNewVariable(typeof(uint)));
            currentLoopContext.Size = ctx.CurrentElement.Lenght;

            ctx.LoopCtx.Push(currentLoopContext);

            currentLoopContext.Body = ctx.G.DefineLabel($"Body_{ctx.CurrentElement.GetHashCode()}");
            currentLoopContext.CheckOutOfBound = ctx.G.DefineLabel($"OutOfBound_{ctx.CurrentElement.GetHashCode()}");

            Type indexType = ctx.CurrentElement.Attribute?.SizeType;

            if (indexType == null)
                indexType = typeof(uint);
            ElementBuildResponse response = null;

            bool staticSize = ctx.IsDeserializing && ctx.CurrentElement.Attribute?.ArrayFixedSize != -1;

            if (staticSize)
            {
                if (ctx.CurrentElement.Index != null)
                    throw new NotSupportedException("Fixed size arrays for jagged array is not supported.");

                int size = ctx.CurrentElement.Attribute.ArrayFixedSize;
                currentLoopContext.Size = (ConstantElement<int>)size;
            }
            else
            {
                if (ctx.IsDeserializing)
                {
                    currentLoopContext.Size = ctx.VariablePool.GetNewVariable(indexType);
                    indexType = indexType.MakeByRefType();
                }

                var request = new ElementBuildRequest()
                {
                    Element = currentLoopContext.Size,
                    InputTypes = GetInputTypes(ctx, indexType),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                response = ctx.Chain.Process(request) as ElementBuildResponse;
                ManageReponse(ctx, response, currentLoopContext.Size);

                if (ctx.IsDeserializing)
                {
                    var newArray = new GenericElement(((g, _) =>
                    {
                        currentLoopContext.Size.Load(g, TypeOfContent.Value);
                        g.NewArray(ctx.CurrentElement.LoadedType.GetElementType());
                    }), null);

                    ctx.CurrentElement.Store(ctx.G, newArray, TypeOfContent.Value);
                }
            }

            ctx.G.LoadConstant(0);
            ctx.G.StoreLocal(currentLoopContext.Index);
            ctx.G.Branch(currentLoopContext.CheckOutOfBound);

            ctx.G.MarkLabel(currentLoopContext.Body);

            return currentLoopContext;
        }

        /// <summary>
        /// TODO: this should not be here -> FIX CA1822.
        /// Generate all the loop epilogues for the current context:
        ///     1. Increment index
        ///     2. Out of bound check, eventually jump to the body
        /// </summary>
        /// <param name="ctx">Context</param>
        private void AddLoopEpilogue(Context ctx)
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
                currentLoopContext.Size.Load(ctx.G, TypeOfContent.Value);

                ctx.G.BranchIfLess(currentLoopContext.Body);
                ctx.VariablePool.ReleaseVariable(currentLoopContext.Index);

                if (currentLoopContext.Size is LocalElement)
                    ctx.VariablePool.ReleaseVariable(currentLoopContext.Size as LocalElement);
            }
        }
        #endregion
    }
}