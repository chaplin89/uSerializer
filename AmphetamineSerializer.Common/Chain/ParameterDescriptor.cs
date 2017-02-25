using System.Reflection;
using AmphetamineSerializer.Common.Attributes;

namespace AmphetamineSerializer.Chain
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor()
        {
        }

        public int Index { get; set; }
        public ParameterInfo Parameter { get; set; }
        public ParameterRole Role { get; set; }
    }
}