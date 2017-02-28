using System;
using Sigil.NonGeneric;
using Sigil;

namespace AmphetamineSerializer.Common.Element
{
    public class LocalElement : IElementInfo
    {

        public static implicit operator LocalElement(Local local)
        {
            return new LocalElement(local);
        }

        public static implicit operator Local(LocalElement local)
        {
            return local.LocalVariable;
        }

        public LocalElement(Local local)
        {
            LocalVariable = local;
        }

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
