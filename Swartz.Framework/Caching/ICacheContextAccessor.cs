namespace Swartz.Caching
{
    public interface ICacheContextAccessor
    {
        IAcquireContext Current { get; set; }
    }
}