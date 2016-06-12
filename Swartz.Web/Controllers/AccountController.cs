using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Swartz.Environment.Configuration;
using Swartz.Mvc.AntiForgery;
using Swartz.Mvc.Extensions;
using Swartz.Users;
using Swartz.Users.Services;
using Swartz.Web.Models;
using Swartz.Web.ViewModels;

namespace Swartz.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController(ShellSettings settings)
        {
            UserManager =
                new SwartzUserManager<User>(
                    new SwartzUserStore<User>(new DatabaseContext(settings.GetDataConnectionString())));
        }

        public SwartzUserManager<User> UserManager { get; }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryTokenSwartz]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return this.RedirectLocal(returnUrl);
                }

                ModelState.AddModelError("FORM", "Invalid username or password");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryTokenSwartz]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User {UserName = model.UserName, Email = model.Email};
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SignInAsync(user, false);
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return View(model);
        }

        private async Task SignInAsync(User user, bool isPersistent)
        {
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            // 添加自定义声明
            //var homeclaim = new Claim(ClaimTypes.Country, user.HomeTown);
            //identity.AddClaim(homeclaim);
            AuthenticationManager.SignIn(new AuthenticationProperties {IsPersistent = isPersistent}, identity);
        }
    }
}