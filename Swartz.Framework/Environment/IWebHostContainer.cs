namespace Swartz.Environment
{
    public interface IWebHostContainer
    {
        T Resolve<T>();
    }
}