namespace Swartz.Services
{
    public interface IClientHostAddressAccessor : IDependency
    {
        string GetClientAddress();
    }
}