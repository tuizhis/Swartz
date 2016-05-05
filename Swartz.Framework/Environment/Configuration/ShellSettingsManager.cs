using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Swartz.FileSystems.AppData;
using Swartz.Logging;

namespace Swartz.Environment.Configuration
{
    public class ShellSettingsManager : Component, IShellSettingsManager
    {
        private const string SettingsFileName = "Settings.txt";
        private readonly IAppDataFolder _appDataFolder;

        public ShellSettingsManager(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            return LoadSettingsInternal();
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(settings.Name))
                throw new ArgumentException(
                    "The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.",
                    nameof(settings));

            Logger.Information("Saving ShellSettings for tenant '{0}'...", settings.Name);
            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), SettingsFileName);
            _appDataFolder.CreateFile(filePath, ShellSettingsSerializer.ComposeSettings(settings));

            Logger.Information("ShellSettings saved successfully; flagging tenant '{0}' for restart.", settings.Name);
        }

        private IEnumerable<ShellSettings> LoadSettingsInternal()
        {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(
                    path => string.Equals(Path.GetFileName(path), SettingsFileName, StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in filePaths)
            {
                yield return ShellSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            }
        }
    }
}