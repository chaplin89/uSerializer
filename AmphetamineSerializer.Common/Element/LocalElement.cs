using System;
using Sigil.NonGeneric;
using Sigil;

namespace AmphetamineSerializer.Common.Element
{
    public class LocalElement : IElementInfo
    {
        Local LocalVariable { get; set; }
        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    if (content == TypeOfContent.Value)
                        g.LoadLocal(LocalVariable);
                    else
                        g.LoadLocalAddress(LocalVariable);
                };
            }
        }

        public Action<Emit, IElementInfo, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    value.Load(g, content);
                    g.StoreLocal(LocalVariable);
                };
            }
        }
    }
}
