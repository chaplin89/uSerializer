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
    public class FieldElement : BaseElement
    {
        /// <summary>
        /// Build this object and initialize instance and field.
        /// </summary>
        /// <param name="instance">Instance of the object that contain the field</param>
        /// <param name="field">The field</param>
        public FieldElement(IElement instance, FieldInfo field)
        {
            Instance = instance;
            Field = field;
            loadedType = Field.FieldType;
        }

        /// <summary>
        /// Object instance.
        /// </summary>
        public IElement Instance { get; private set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; private set; }

        /// <summary>
        /// Access the ASIndexAttribute of the field.
        /// </summary>
        public override ASIndexAttribute Attribute
        {
            get
            {
                if (Field == null)
                    return null;
                return Field.GetCustomAttribute<ASIndexAttribute>();
            }
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
            g.StoreField(Field);
        }

        protected override void InternalLoad(Emit g, TypeOfContent content)
        {
            if (content == TypeOfContent.Value)
                g.LoadField(Field);
            else
                g.LoadFieldAddress(Field);
        }
    }
}
