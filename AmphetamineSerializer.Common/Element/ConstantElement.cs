using System;
using Sigil.NonGeneric;

namespace AmphetamineSerializer.Common.Element
{
    class ConstantElement<ConstantType> : IElementInfo
    {
        public ConstantType Constant { get; set; }
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (content == TypeOfContent.Address)
                        throw new NotSupportedException("Requested the address of a constant value.");
                    var method = typeof(ConstantType)
                                  .GetMethod("LoadConstant", new Type[] { Constant.GetType() });

                    if (method == null)
                        throw new InvalidOperationException("Unrecognized type");
                    method.Invoke(g, new object[] { Constant });
                };
            }
        }

        public Action<Emit, IElementInfo, TypeOfContent> Store
        {
            get
            {
                throw new NotSupportedException("Trying to set a constant value.");
            }
        }
    }
}
