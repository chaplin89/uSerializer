using Sigil.NonGeneric;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Represent the tuple object instance, field.
    /// </summary>
    public class FieldElementInfo : IElementInfo
    {
        public FieldElementInfo(IElementInfo instance, FieldInfo field)
        {
            Instance = instance;
            Field = field;
        }

        /// <summary>
        /// Object instance.
        /// </summary>
        public IElementInfo Instance { get; set; }

        /// <summary>
        /// If the field is an array, 
        /// this is the index used in load/store.
        /// </summary>
        public IElementInfo Index { get; set; }

        /// <summary>
        /// Field information.
        /// </summary>
        public FieldInfo Field { get; set; }

        public Action<Emit, TypeOfContent> Load
        {
            get
            {
                return (g, content) =>
                {
                    Instance.Load(g, TypeOfContent.Value);

                    if (Field.FieldType.IsArray)
                    {
                        g.LoadField(Field);
                        Index.Load(g, TypeOfContent.Value);

                        if (content == TypeOfContent.Value)
                            g.LoadElement(Field.FieldType.GetElementType());
                        else
                            g.LoadElementAddress(Field.FieldType.GetElementType());
                    }
                    else
                    {
                        if (content == TypeOfContent.Value)
                            g.LoadField(Field);
                        else
                            g.LoadFieldAddress(Field);
                    }
                };
            }
        }

        public Action<Emit, IElementInfo, TypeOfContent> Store
        {
            get
            {
                return (g, value, content) =>
                {
                    Instance.Load(g, TypeOfContent.Value);
                    if (Field.FieldType.IsArray)
                    {
                        g.LoadField(Field);
                        Index.Load(g, TypeOfContent.Value);
                    }

                    value.Load(g, TypeOfContent.Value);

                    if (Field.FieldType.IsArray)
                        g.StoreElement(Field.FieldType.GetElementType());
                    else
                        g.StoreField(Field);
                };
            }
        }
    }
}
