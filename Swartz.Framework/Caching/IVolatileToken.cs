namespace Swartz.Caching
{
    public interface IVolatileToken
    {
        bool IsCurrent { get; }
    }
}