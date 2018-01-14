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
        private ConstantElement<uint> staticSizeCounter;

        public ByteCountBackend(Context ctx) : base(ctx)
        {
        }

        protected override IResponse InternalMake()
        {
            if (ctx.CurrentElement == null)
                return null;
            
            if (!typeSizeMap.ContainsKey(ctx.CurrentElement.LoadedType))
                return null;

            object counter = null;
            bool counterExists = ctx.AdditionalContext.TryGetValue("StaticSizeCounter", out counter);

            if (counterExists)
            {
                staticSizeCounter = (ConstantElement<uint>)counter;
            }
            else
            {
                staticSizeCounter = new ConstantElement<uint>(0);
                ctx.AdditionalContext.Add("StaticSizeCounter", staticSizeCounter);
            }
            staticSizeCounter.Constant += typeSizeMap[ctx.CurrentElement.LoadedType].Constant;

            return new ContextModifiedBuildResponse();
        }

        public override IResponse PreMake()
        {
            return null;
        }

        public override IResponse PostMake()
        {
            return null;
        }
    }
}
