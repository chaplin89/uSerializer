using System;

namespace AmphetamineSerializer.Common
{
    static class TypeExtension
    {
        public static Type Normalize(this Type type)
        {
            Type returnValue = type;
            if (type.HasElementType)
                returnValue = type.GetElementType();
            if (type.IsEnum)
                returnValue = type.UnderlyingSystemType;
            return returnValue;
        }
    }
}
