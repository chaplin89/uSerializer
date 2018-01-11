using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using Sigil.NonGeneric;
using System.Reflection;

namespace AmphetamineSerializer.Common.Element
{
    public class PropertyElement : MemberElement
    {
        public PropertyElement(IElement instance, PropertyInfo property) : base(instance, property, property.PropertyType)
        {
        }

        public override void Load(Emit g, TypeOfContent content)
        {
            Instance.Load(g, TypeOfContent.Value);
            base.Load(g, content);
        }

        public override void Store(Emit g, IElement value, TypeOfContent content)
        {
            Instance.Load(g, TypeOfContent.Value);
            base.Store(g, value, content);
        }

        protected override void InternalStore(Emit g, TypeOfContent content)
        {
            var property = Member as PropertyInfo;
            g.CallVirtual(property.SetMethod);
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            var property = Member as PropertyInfo;

            g.CallVirtual(property.GetMethod);
            if (content == TypeOfContent.Address)
                throw new System.Exception();
        }
    }
}