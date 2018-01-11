using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;

namespace AmphetamineSerializer.Backends
{
    public class ByteCountBackend : BuilderBase
    {
        private ConstantElement<uint> staticSize = new ConstantElement<uint>(0);
        private List<IElement> dynamicSizes = new List<IElement>();

        static private readonly Dictionary<Type, ConstantElement<uint>> typeSizeMap = new Dictionary<Type, ConstantElement<uint>>()
        {
            {typeof(uint),    sizeof(uint) },
            {typeof(int),     sizeof(int)  },
            {typeof(ushort),  sizeof(ushort)},
            {typeof(short),   sizeof(short)},
            {typeof(double),  sizeof(double)},
            {typeof(float),   sizeof(float)},
            {typeof(ulong),   sizeof(ulong)},
            {typeof(long),    sizeof(long) },
            {typeof(char),    sizeof(char) },
            {typeof(byte),    sizeof(byte) },
            {typeof(sbyte),   sizeof(sbyte)}
        };

        public ByteCountBackend(Context ctx) : base(ctx)
        {
        }

        protected override ElementBuildResponse InternalMake()
        {
            if (ctx.CurrentElement == null)
                return null;

            LocalElement size = ctx.AdditionalContext as LocalElement;

            if (ctx.AdditionalContext == null)
            {
                size = ctx.VariablePool.GetNewVariable(typeof(uint));
                ctx.AdditionalContext = size;
                size.Store(ctx.G, (ConstantElement<uint>)0, Model.TypeOfContent.Value);
            }

            if (!typeSizeMap.ContainsKey(ctx.CurrentElement.LoadedType))
                return null;

            var sumElement = new GenericElement(typeof(uint))
            {
                LoadAction = (g, type) =>
                {
                    size.Load(ctx.G, Model.TypeOfContent.Value);
                    typeSizeMap[ctx.CurrentElement.LoadedType].Load(ctx.G, Model.TypeOfContent.Value);
                    ctx.G.Add();
                }
            };

            size.Store(ctx.G, sumElement, Model.TypeOfContent.Value);

            return new ElementBuildResponse() { Status = Model.BuildedFunctionStatus.ContextModified };
        }
    }
}
