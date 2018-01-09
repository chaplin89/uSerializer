using AmphetamineSerializer.Nodes;
using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using Sigil;
using System;
using System.Collections.Generic;
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
                CreateAndAssignNewInstance(instance, null);
            }

            var versions = VersionHelper.GetExplicitlyManagedVersions(normalizedType).ToArray();

            if (versions.Length > 1)
                ManageVersions(instance, versions, normalizedType);
            else
            {
                BuildFromFields(VersionHelper.GetAllFields(instance, normalizedType));
                ctx.G.Return();
            }

            return ctx.Provider.GetMethod();
        }

        /// <summary>
        /// Creat an element of type "instance" and store it in the "instance" element.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="size"></param>
        private void CreateAndAssignNewInstance(IElement instance, IElement size)
        {
            Type elementType = instance.LoadedType.GetElementType();
            GenericElement newInstance = null;

            if (size == null)
            {
                newInstance = new GenericElement(((g, _) => g.NewObject(elementType)), null);
            }
            else
            {
                newInstance = new GenericElement(((g, _) =>
                {
                    size.Load(g, TypeOfContent.Value);
                    g.NewArray(elementType);
                }), null);
            }

            instance.Store(ctx.G, newInstance, TypeOfContent.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="versions"></param>
        /// <param name="normalizedType"></param>
        private void ManageVersions(IElement instance, object[] versions, Type normalizedType)
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
                InputTypes = GetInputTypes(versionType),
                Provider = ctx.Provider,
                G = ctx.G
            };

            Request(request, versionField);

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
                    BuildFromFields(fields);
                    ctx.G.Return();
                }
            }
            else if (versionField.Field.FieldType.IsAssignableFrom(typeof(IEquatable<>)))
            {
                // TODO: Manage non numeric versions
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="overrideRootType"></param>
        /// <returns></returns>
        private Type[] GetInputTypes(Type overrideRootType)
        {
            Type[] finalTypes = new Type[ctx.InputParameters.Length];
            Array.Copy(ctx.InputParameters, finalTypes, ctx.InputParameters.Length);
            finalTypes[0] = overrideRootType;
            return finalTypes;
        }

        /// <summary>
        /// Send the request to the chain and undertakes the appropriate action.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="response"></param>
        private void Request(IRequest request, IElement element)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (element == null)
                throw new ArgumentNullException("element");

            var response = ctx.Chain.Process(request) as ElementBuildResponse;

            if (response == null)
                throw new InvalidOperationException("Unable to process the request.");

            if (response.Status == BuildedFunctionStatus.ContextModified)
                return;

            if (ctx.IsDeserializing)
                element.Load(ctx.G, TypeOfContent.Address);
            else
                element.Load(ctx.G, TypeOfContent.Value);

            for (ushort j = 1; j < ctx.InputParameters.Length; j++)
                ctx.G.LoadArgument(j);

            if (response.Status == BuildedFunctionStatus.FunctionFinalizedTypeNotFinalized)
            {
                ctx.G.Call(response.Emiter);
                return;
            }
            else if (response.Status == BuildedFunctionStatus.TypeFinalized)
            {
                if (!response.Method.IsStatic)
                    throw new InvalidOperationException("Expected a static function but received a non-static one.");

                ctx.G.Call(response.Method);
                return;
            }

            throw new InvalidOperationException("Can't forward parameters.");
        }

        /// <summary>
        /// Recursively emit the instruction for deserialize everything,
        /// including array, primitive and non primitive types.
        /// </summary>
        /// <param name="fields">Fields to manage</param>
        private void BuildFromFields(IEnumerable<IElement> fields)
        {
            if (ctx == null)
                throw new InvalidOperationException("Context is null.");

            var linkedList = new Stack<IElement>(fields.Reverse());

            while (linkedList.Count > 0)
            {
                ctx.CurrentElement = linkedList.Pop();

                while (ctx.CurrentElement.IsIndexable)
                {
                    // Generate the loop preamble and skip to element @ "Index" of the inner type.
                    IElement sizeElement = null;
                    if (ctx.CurrentElement.Attribute?.SizeIndex != -1)
                    {
                        if (ctx.CurrentElement.Attribute.SizeIndex > ctx.CurrentElement.Attribute.Index)
                            throw new InvalidOperationException("Size should come before the array.");
                        sizeElement = fields.Where(_ => _.Attribute.Index == ctx.CurrentElement.Attribute.SizeIndex).Single();
                    }

                    var loopContext = GenerateLoopPreamble(sizeElement);
                    var innerElement = ctx.CurrentElement.EnterArray(loopContext.Index);

                    ctx.CurrentElement = innerElement;
                    ctx.LoopCtx.Push(loopContext);
                }

                // TODO:
                // 1. List (needs a special handling because of its similarities with Array)
                // 2. Anything else implementing IEnumerable (excluding List ofc)
                if (ctx.CurrentElement.LoadedType.IsAbstract)
                    throw new InvalidOperationException("Incomplete types are not allowed.");

                var request = new ElementBuildRequest()
                {
                    Element = ctx.CurrentElement,
                    AdditionalContext = ctx.AdditionalContext,
                    InputTypes = GetInputTypes(GetCurrentNormalizedType()),
                    Provider = ctx.Provider,
                    G = ctx.G,
                };

                Request(request, ctx.CurrentElement);

                while (ctx.LoopCtx.Count > 0)
                    GenerateLoopEpilogue(ctx.LoopCtx.Pop());
            }
        }

        /// <summary>
        /// Return a "normalized" version of the type of the current element.
        /// </summary>
        /// <returns>
        /// If the type is an enum, this return the underlying type.
        /// If the first input parameter is by ref, this return the type by ref.
        /// </returns>
        private Type GetCurrentNormalizedType()
        {
            Type normalizedType = ctx.CurrentElement.LoadedType;

            if (normalizedType.IsEnum)
                normalizedType = normalizedType.GetEnumUnderlyingType();

            if (ctx.IsDeserializing)
                normalizedType = normalizedType.MakeByRefType();

            return normalizedType;
        }
        #endregion

        #region Loop handling
        /// <summary>
        /// Generate a loop preamble:
        ///     1. Initialize the index
        ///     2. Jump for checking if current index is out of bound
        ///     3. Mark the begin of the loop's body
        /// </summary>
        /// <returns>The generated loop context.</returns>
        private LoopContext GenerateLoopPreamble(IElement size)
        {
            var loopContext = new LoopContext(ctx.VariablePool.GetNewVariable(typeof(uint)))
            {
                Size = ctx.CurrentElement.Lenght,
                Body = ctx.G.DefineLabel($"Body_{ctx.CurrentElement.GetHashCode()}"),
                CheckOutOfBound = ctx.G.DefineLabel($"OutOfBound_{ctx.CurrentElement.GetHashCode()}"),
            };

            Type indexType = ctx.CurrentElement.Attribute?.SizeType;

            if (indexType == null)
                indexType = typeof(uint);

            bool staticSize = ctx.IsDeserializing && ctx.CurrentElement.Attribute?.ArrayFixedSize != -1;
            bool isSizeInAnotherField = ctx.IsDeserializing && ctx.CurrentElement.Attribute?.SizeIndex != -1;

            if (staticSize)
            {
                if (ctx.CurrentElement.Index != null)
                    throw new NotSupportedException("Fixed size arrays for jagged array is not supported.");

                loopContext.Size = (ConstantElement<int>)ctx.CurrentElement.Attribute.ArrayFixedSize;
            }
            else if (ctx.IsDeserializing)
            {
                if (size != null)
                    loopContext.Size = size;
                else
                    loopContext.Size = ctx.VariablePool.GetNewVariable(indexType);

                indexType = indexType.MakeByRefType();

                var request = new ElementBuildRequest()
                {
                    Element = loopContext.Size,
                    InputTypes = GetInputTypes(indexType),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                Request(request, loopContext.Size);
                CreateAndAssignNewInstance(ctx.CurrentElement, loopContext.Size);
            }
            else if (size == null)
            {
                var request = new ElementBuildRequest()
                {
                    Element = loopContext.Size,
                    InputTypes = GetInputTypes(indexType),
                    AdditionalContext = ctx.AdditionalContext,
                    Provider = ctx.Provider,
                    G = ctx.G
                };

                Request(request, loopContext.Size);
            }

            ctx.G.LoadConstant(0);
            ctx.G.StoreLocal(loopContext.Index);
            ctx.G.Branch(loopContext.CheckOutOfBound);

            ctx.G.MarkLabel(loopContext.Body);

            return loopContext;
        }

        /// <summary>
        /// Generate all the loop epilogues for the current context:
        ///     1. Increment index
        ///     2. Out of bound check, eventually jump to the body
        /// </summary>
        private void GenerateLoopEpilogue(LoopContext loopContext)
        {
            ctx.G.LoadLocal(loopContext.Index);
            ctx.G.LoadConstant(1);
            ctx.G.Add();
            ctx.G.StoreLocal(loopContext.Index);

            ctx.G.MarkLabel(loopContext.CheckOutOfBound);
            ctx.G.LoadLocal(loopContext.Index);
            loopContext.Size.Load(ctx.G, TypeOfContent.Value);

            ctx.G.BranchIfLess(loopContext.Body);
            ctx.VariablePool.ReleaseVariable(loopContext.Index);

            if (loopContext.Size is LocalElement)
                ctx.VariablePool.ReleaseVariable(loopContext.Size as LocalElement);
        }
        #endregion
    }
}