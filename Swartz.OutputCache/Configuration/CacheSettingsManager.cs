using System;
using System.IO;
using System.Linq;
using Swartz.Environment.Configuration;
using Swartz.FileSystems.AppData;

namespace Swartz.OutputCache.Configuration
{
    public class CacheSettingsManager : Component, ICacheSettingsManager
    {
        private const string SettingsFileName = "CacheSettings.txt";
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;

        public CacheSettingsManager(IAppDataFolder appDataFolder, ShellSettings shellSettings)
        {
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
        }

        CacheSettings ICacheSettingsManager.LoadSettings()
        {
            var filePath =
                _appDataFolder.ListDirectories("OutputCache")
                    .SelectMany(path => _appDataFolder.ListFiles(path))
                    .Where(
                        path =>
                            string.Equals(Path.GetFileName(path), SettingsFileName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => new FileInfo(x).LastWriteTimeUtc)
                    .FirstOrDefault();

            if (filePath != null)
            {
                return CacheSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            }

            return null;
        }

        void ICacheSettingsManager.SaveSettings(CacheSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var filePath = Path.Combine(Path.Combine("OutputCache", _shellSettings.Name), SettingsFileName);
            _appDataFolder.CreateFile(filePath, CacheSettingsSerializer.ComposeSettings(settings));
        }
    }
}