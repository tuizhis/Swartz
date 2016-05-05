using System.Configuration;

namespace Swartz.Environment.Configuration
{
    public class AppConfigurationAccessor : IAppConfigurationAccessor
    {
        public string GetConfiguration(string name)
        {
            var appSettingsValue = ConfigurationManager.AppSettings[name] ?? ConfigurationManager.AppSettings[name];
            if (appSettingsValue != null)
            {
                return appSettingsValue;
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[name] ??
                                           ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings != null)
            {
                return connectionStringSettings.ConnectionString;
            }

            return string.Empty;
        }
    }
}