using AmphetamineSerializer.Interfaces;
using System;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    public class SerializationBuildResponse : IResponse
    {
        public BuildedFunction Method;

        public object Instance { get; set; }
        public string ProcessedBy { get; set; }
    }
}
