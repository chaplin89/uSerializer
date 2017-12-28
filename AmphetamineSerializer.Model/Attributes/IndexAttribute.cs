using System;

namespace AmphetamineSerializer.Model.Attributes
{
    /// <summary>
    /// Instruct the deserializator about how to deserialize a given field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ASIndexAttribute : Attribute
    {
        /// <summary>
        /// Order used for deserializing the field.
        /// </summary>
        /// <remarks>
        /// This is the only required information that have to be provided in order
        /// to correctly build the logic for deserialization.
        /// Indexes has not to be sequential because the index is just an hint for the deserializator.
        /// In other word, it won't care if are specified the index 1 and the index 3 but not the index 2.
        /// If you specify two field with the same Index the behaviour is undefined (no warning are issued).
        /// </remarks>
        public int Index { get; }

        /// <summary>
        /// If the field is an array and is prefixed by a lenght, this property specify
        /// the type of the prefixed lenght (i.e.: int, short, etc.).
        /// </summary>
        /// <remarks>
        /// This property should be set only if the field is an array.
        /// If the field is not an array this will be ignored (no warning are issued).
        /// </remarks>
        public Type SizeType { get; set; }

        /// <summary>
        /// If set, the field will be deserialized only if the version is &gte; this property.
        /// </summary>
        public int VersionBegin { get; set; }

        /// <summary>
        /// If set, the field will be deserialized only if the version is &lte; this property.
        /// </summary>
        public int VersionEnd { get; set; }

        /// <summary>
        /// If set, the field will be deserialized only if the version is == this property.
        /// </summary>
        public object Version { get; set; }

        /// <summary>
        /// If the type is an array, this property specify the size of the array.
        /// If this property is not set, the deserializator generally will assume that the array
        /// is lenght prefixed.
        /// </summary>
        public int ArrayFixedSize { get; set; }

        /// <summary>
        /// If the type is an array, this property specify what is the field that contains the
        /// size of this array. Of course, the size should come before the array itself.
        /// </summary>
        public int SizeIndex { get; set; }

        /// <summary>
        /// Initialize an attribute with an index.
        /// </summary>
        /// <param name="index">Index</param>
        public ASIndexAttribute(int index)
        {
            Index = index;

            VersionBegin = -1;
            VersionEnd = -1;
            ArrayFixedSize = -1;
        }
    }
}
