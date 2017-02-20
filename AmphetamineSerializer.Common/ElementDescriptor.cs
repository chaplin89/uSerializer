using System;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// 
    /// </summary>
    public struct ElementDescriptor
    {
        /// <summary>
        /// The <see cref="FieldInfo"/> structure for the current item.
        /// </summary>
        /// <seealso cref="CurrentItemType"/>
        public FieldInfo CurrentItemFieldInfo { get; set; }

        /// <summary>
        /// When building the deserializing logic for a type, 
        /// this field contains the type for the currently processed
        /// field.
        /// </summary>
        public Type CurrentItemType { get; set; }

        /// <summary>
        /// If it's an array or an enum, this field contains the type
        /// contained inside.
        /// </summary>
        public Type CurrentItemUnderlyingType { get; set; }

        /// <summary>
        /// Return the index type for current field.
        /// </summary>
        public ASIndexAttribute CurrentAttribute
        {
            get
            {
                if (CurrentItemFieldInfo != null)
                    return CurrentItemFieldInfo.GetCustomAttribute<ASIndexAttribute>();
                return null;
            }
        }
    }
}