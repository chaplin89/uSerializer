using System.Reflection;

namespace AmphetamineSerializer.Interfaces
{
    public interface IBuilder
    {
        MethodInfo Method { get; }
    }
}