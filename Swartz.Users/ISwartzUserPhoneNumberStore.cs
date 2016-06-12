using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public interface ISwartzUserPhoneNumberStore<TUser, in TKey> : IUserPhoneNumberStore<TUser, TKey>
        where TUser : class, ISwartzUser<TKey>
    {
        Task<TUser> FindByPhoneNumberAsync(string phone);
    }

    public interface ISwartzUserPhoneNumberStore<TUser> : ISwartzUserPhoneNumberStore<TUser, decimal>
        where TUser : class, ISwartzUser<decimal>
    {
    }
}