using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Swartz.Users.Validators
{
    public class SwartzUserValidator<TUser, TKey> : UserValidator<TUser, TKey> where TUser : class, ISwartzUser<TKey>
        where TKey : IEquatable<TKey>
    {
        public SwartzUserValidator(UserManager<TUser, TKey> manager) : base(manager)
        {
        }

        private SwartzUserManager<TUser, TKey> Manager { get; set; }

        public bool RequireUniquePhoneNumber { get; set; }

        public override async Task<IdentityResult> ValidateAsync(TUser item)
        {
            var errors = new List<string>();
            if (RequireUniquePhoneNumber)
            {
                await ValidatePhoneNumber(item, errors);
            }

            if (errors.Count > 0)
            {
                return IdentityResult.Failed(errors.ToArray());
            }

            return await base.ValidateAsync(item);
        }

        private async Task ValidatePhoneNumber(TUser user, List<string> errors)
        {
            var phone = await Manager.GetPhoneNumberAsync(user.Id);
            if (string.IsNullOrWhiteSpace(phone))
            {
                errors.Add("Phone Number cannot be null or empty.");
            }
            else
            {
                var owner = await Manager.FindByPhoneNumberAsync(phone);
                if (owner == null || EqualityComparer<TKey>.Default.Equals(owner.Id, user.Id))
                {
                    return;
                }
                errors.Add($"{phone} is already taken");
            }
        }
    }
}