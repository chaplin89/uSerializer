using System.Reflection;
using AmphetamineSerializer.Common.Attributes;
using System;

namespace AmphetamineSerializer.Chain
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor()
        {
        }

        public int Index { get; set; }
        public Type Parameter { get; set; }
        public ParameterRole Role { get; set; }
    }
}