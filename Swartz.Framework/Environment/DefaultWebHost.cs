using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders;

namespace Swartz.Environment
{
    public class DefaultWebHost : IWebHost
    {
        private readonly IShellSettingsManager _shellSettingsManager;

        void IWebHost.BeginRequest()
        {
            throw new NotImplementedException();
        }

        void IWebHost.EndRequest()
        {
            throw new NotImplementedException();
        }

        ShellContext IWebHost.GetShellContext(ShellSettings shellSettings)
        {
            throw new NotImplementedException();
        }

        void IWebHost.Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
