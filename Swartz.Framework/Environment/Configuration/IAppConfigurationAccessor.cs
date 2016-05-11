namespace Swartz.Environment.Configuration
{
    /// <summary>
    ///     Exposes application configuration (can be e.g. AppSettings from Web.config)
    /// </summary>
    public interface IAppConfigurationAccessor : IDependency
    {
        string GetConfiguration(string name);
    }
}