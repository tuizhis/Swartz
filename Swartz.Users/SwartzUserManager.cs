using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Swartz.Users.Validators;

namespace Swartz.Users
{
    public class SwartzUserManager<TUser, TKey> : UserManager<TUser, TKey> where TUser : class, ISwartzUser<TKey>
        where TKey : IEquatable<TKey>
    {
        public SwartzUserManager(IUserStore<TUser, TKey> store) : base(store)
        {
            UserValidator = new SwartzUserValidator<TUser, TKey>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }

        public virtual Task<TUser> FindByPhoneNumberAsync(string phone)
        {
            var store = GetPhoneNumberStore();
            if (phone == null)
            {
                throw new ArgumentNullException(nameof(phone));
            }

            return store.FindByPhoneNumberAsync(phone);
        }

        internal ISwartzUserPhoneNumberStore<TUser, TKey> GetPhoneNumberStore()
        {
            var phoneNumberStore = Store as ISwartzUserPhoneNumberStore<TUser, TKey>;
            if (phoneNumberStore == null)
            {
                throw new NotSupportedException("Store does not implement ISwartzUserPhoneNumberStore<TUser, TKey>.");
            }

            return phoneNumberStore;
        }
    }

    public class SwartzUserManager<TUser> : SwartzUserManager<TUser, decimal> where TUser : class, ISwartzUser<decimal>
    {
        public SwartzUserManager(IUserStore<TUser, decimal> store) : base(store)
        {
        }
    } 
}