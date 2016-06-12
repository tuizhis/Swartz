using System.ComponentModel.DataAnnotations;

namespace Swartz.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}