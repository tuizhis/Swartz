namespace Swartz.Environment.Configuration
{
    public interface IShellSettingsManager
    {
        ShellSettings LoadSettings();

        void SaveSettings(ShellSettings settings);
    }
}