using System;
using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public interface ISwartzUserStore<TUser, TRole, TUserRole, in TKey> : IUserRoleStore<TUser, TKey>,
        IUserPasswordStore<TUser, TKey>, IUserEmailStore<TUser, TKey>, IQueryableUserStore<TUser, TKey>,
        IUserPhoneNumberStore<TUser, TKey>, IUserSecurityStampStore<TUser, TKey>, IDependency 
        where TUser : class, ISwartzUser<TKey>
    {
    }
}