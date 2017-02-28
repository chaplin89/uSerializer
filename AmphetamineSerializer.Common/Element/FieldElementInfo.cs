using Sigil.NonGeneric;
using System;
using System.Reflection;
using AmphetamineSerializer.Common.Attributes;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Abstract load/store of a class field.
    /// </summary>
    public class FieldElementInfo : IElementInfo
    {

        /// <summary>
        /// Build this object and initialize instance and field.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="field"></param>
        public FieldElementInfo(IElementInfo instance, FieldInfo field)
        {
            Instance = instance;
            Field = field;
        }

        /// <summary>
        /// Build this object with null instance/field.
        /// </summary>
        public FieldElementInfo()
        {
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

        /// <summary>
        /// Access the ASIndexAttribute of Field.
        /// </summary>
        public ASIndexAttribute CurrentAttribute { get; }
        
        /// <summary>
        /// Abstract the load of a class field.
        /// </summary>
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

        /// <summary>
        /// Abstract the store of a class field.
        /// </summary>
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
