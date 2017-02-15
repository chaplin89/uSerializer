using System;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Basic option for managing the serialization of strings.
    /// </summary>
    [Flags]
    public enum StringType : int
    {
        /// <summary>
        /// Encoding is ASCII
        /// </summary>
        Ascii = 2,
        /// <summary>
        /// String is null-terminated.
        /// </summary>
        NullTerminated = 4,
        /// <summary>
        /// Encoding is Unicode.
        /// </summary>
        Unicode = 8,
        /// <summary>
        /// Unicode/Lenght prefixed.
        /// </summary>
        Default = 0
    }
}
