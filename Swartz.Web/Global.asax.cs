using System.Web;
using Autofac;
using Swartz.Environment;
using Swartz.WarmupStarter;

namespace Swartz.Web
{
    public class MvcApplication : HttpApplication
    {
        private static Starter<IWebHost> _starter;

        protected void Application_Start()
        {
            _starter = new Starter<IWebHost>(HostInitialization, HostBeginRequest, HostEndRequest);
            _starter.OnApplicationStart(this);
        }

        protected void Application_BeginRequest()
        {
            _starter.OnBeginRequest(this);
        }

        protected void Application_EndRequest()
        {
            _starter.OnEndRequest(this);
        }

        private static void HostBeginRequest(HttpApplication application, IWebHost host)
        {
            host.BeginRequest();
        }

        private static void HostEndRequest(HttpApplication application, IWebHost host)
        {
            host.EndRequest();
        }

        private static IWebHost HostInitialization(HttpApplication application)
        {
            var host = SwartzStarter.CreateHost(MvcSingletons, application);

            host.Initialize();
            host.BeginRequest();
            host.EndRequest();

            return host;
        }

        private static void MvcSingletons(ContainerBuilder builder)
        {
        }
    }
}