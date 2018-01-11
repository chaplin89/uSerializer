using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model;
using AmphetamineSerializer.Model.Attributes;
using Sigil.NonGeneric;
using System.Reflection;

namespace AmphetamineSerializer.Common.Element
{
    /// <summary>
    /// Manage a class field.
    /// </summary>
    public class FieldElement : MemberElement
    {
        /// <summary>
        /// Build this object and initialize instance and field.
        /// </summary>
        /// <param name="instance">Instance of the object that contain the field</param>
        /// <param name="field">The field</param>
        public FieldElement(IElement instance, FieldInfo field) : base(instance, field, field.FieldType)
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
            var field = Member as FieldInfo;
            g.StoreField(field);
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            var field = Member as FieldInfo;
            if (content == TypeOfContent.Value)
                g.LoadField(field);
            else
                g.LoadFieldAddress(field);
        }
    }
}
