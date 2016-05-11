namespace Swartz.OutputCache.Configuration
{
    public interface ICacheSettingsManager : ISingletonDependency
    {
        CacheSettings LoadSettings();

        void SaveSettings(CacheSettings settings);
    }
}