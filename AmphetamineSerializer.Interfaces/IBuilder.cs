namespace AmphetamineSerializer.Interfaces
{
    public interface IBuilder
    {
        IResponse PreMake();
        IResponse Make();
        IResponse PostMake();
    }
}