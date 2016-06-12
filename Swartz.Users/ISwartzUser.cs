using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public interface ISwartzUser<out TKey> : IUser<TKey>
    {
    }

    public interface ISwartzUser : ISwartzUser<decimal>
    {
    }
}