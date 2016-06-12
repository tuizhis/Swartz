using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Swartz.Users.Validators
{
    public class SwartzPasswordValidator : PasswordValidator
    {
        public bool RequirePassword { get; set; }

        public override Task<IdentityResult> ValidateAsync(string item)
        {
            if (!RequirePassword && string.IsNullOrEmpty(item))
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return base.ValidateAsync(item);
        }
    }
}