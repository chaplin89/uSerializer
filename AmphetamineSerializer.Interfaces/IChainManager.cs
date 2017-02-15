using AmphetamineSerializer.Interfaces;

namespace AmphetamineSerializer
{
    public interface IChainManager
    {
        IChainElement First { get; }
        IChainManager SetNext(IChainElement next);
        IResponse Process(IRequest request);
    }
}