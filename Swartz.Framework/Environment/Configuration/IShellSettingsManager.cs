using System.Collections.Generic;

namespace Swartz.Environment.Configuration
{
    public interface IShellSettingsManager
    {
        IEnumerable<ShellSettings> LoadSettings();

        void SaveSettings(ShellSettings settings);
    }
}