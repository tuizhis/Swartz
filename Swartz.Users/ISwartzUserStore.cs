using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public interface ISwartzUserStore<TUser, in TKey> : IUserRoleStore<TUser, TKey>, IUserPasswordStore<TUser, TKey>,
        IUserEmailStore<TUser, TKey>, IQueryableUserStore<TUser, TKey>, IUserPhoneNumberStore<TUser, TKey>,
        IUserSecurityStampStore<TUser, TKey> where TUser : class, ISwartzUser<TKey>
    {
    }

    public interface ISwartzUserStore<TUser> : ISwartzUserStore<TUser, decimal>
        where TUser : class, ISwartzUser<decimal>
    {
    }
}