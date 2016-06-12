using System;
using Microsoft.AspNet.Identity;

namespace Swartz.Users
{
    public class SwartzRoleManager<TRole, TKey> : RoleManager<TRole, TKey> where TRole : class, ISwartzRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public SwartzRoleManager(IRoleStore<TRole, TKey> store) : base(store)
        {
        }
    }

    public class SwartzRoleManager<TRole> : SwartzRoleManager<TRole, decimal> where TRole : class, ISwartzRole<decimal>
    {
        public SwartzRoleManager(IRoleStore<TRole, decimal> store) : base(store)
        {
        }
    }
}