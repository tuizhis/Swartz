using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Owin;
using Swartz.Owin;
using Swartz.Users;

namespace Swartz.Web
{
    public class OwinMiddleware : IOwinMiddlewareProvider
    {
        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares()
        {
            var middleware = new OwinMiddlewareRegistration
            {
                Configure = app =>
                {
                    app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                        LoginPath = new PathString("/Account/LogOn"),
                        TicketDataFormat = new TicketDataFormat(new MachineKeyProtector())
                    });
                }
            };

            return new[] {middleware};
        }
    }
}