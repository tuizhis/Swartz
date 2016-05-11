using System.Diagnostics;

namespace Swartz.Environment
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        public string GetEnvironmentIdentifier()
        {
            return $"{System.Environment.MachineName}-{Process.GetCurrentProcess().Id}";
        }
    }
}