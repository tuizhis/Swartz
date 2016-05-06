using System;
using Swartz.Caching;
using Swartz.Exceptions;
using Swartz.FileSystems.AppData;
using Swartz.Logging;

namespace Swartz.Environment
{
    public interface IHostLocalRestart
    {
        /// <summary>
        ///     Monitor changes on the persistent storage.
        /// </summary>
        /// <param name="monitor"></param>
        void Monitor(Action<IVolatileToken> monitor);
    }

    public class DefaultHostLocalRestart : IHostLocalRestart
    {
        private const string FileName = "hrestart.txt";
        private readonly IAppDataFolder _appDataFolder;

        public DefaultHostLocalRestart(IAppDataFolder appDataFolder)
        {
            _appDataFolder = appDataFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Monitor(Action<IVolatileToken> monitor)
        {
            if (!_appDataFolder.FileExists(FileName))
            {
                TouchFile();
            }

            Logger.Debug("Monitoring virtual path \"{0}\"", FileName);
            monitor(_appDataFolder.WhenPathChanges(FileName));
        }

        private void TouchFile()
        {
            try
            {
                _appDataFolder.CreateFile(FileName, "Host Restart");
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }
                Logger.Warning(ex, "Error updating file '{0}'", FileName);
            }
        }
    }
}