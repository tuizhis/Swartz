using System;
using System.Security.Cryptography;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders;
using Swartz.Exceptions;
using Swartz.Logging;
using Swartz.Utility.Extensions;

namespace Swartz.Environment
{
    public class DefaultWebHost : IWebHost
    {
        private static readonly object SyncLock = new object();
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellSettingsManager _shellSettingsManager;

        private ShellContext _shellContext;

        public DefaultWebHost(IShellSettingsManager shellSettingsManager, IShellContextFactory shellContextFactory,
            IApplicationEnvironment applicationEnvironment)
        {
            _shellContextFactory = shellContextFactory;
            _shellSettingsManager = shellSettingsManager;
            _applicationEnvironment = applicationEnvironment;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShellContext Current => BuildCurrent();

        void IWebHost.BeginRequest()
        {
        }

        void IWebHost.EndRequest()
        {
        }

        void IWebHost.Initialize()
        {
            BuildCurrent();
        }

        private ShellContext BuildCurrent()
        {
            if (_shellContext == null)
            {
                lock (SyncLock)
                {
                    if (_shellContext == null)
                    {
                        CreateAndActivateShells();
                    }
                }
            }

            return _shellContext;
        }

        private void CreateAndActivateShells()
        {
            var settings = _shellSettingsManager.LoadSettings();
            if (settings == null)
            {
                settings = new ShellSettings
                {
                    Name = _applicationEnvironment.GetEnvironmentIdentifier().ToSafeDirectoryName(),
                    DataProvider = "MySql",
                    EncryptionAlgorithm = "AES",
                    HashAlgorithm = "HMACSHA256",
                    Host = "",
                    User = "",
                    Database = "",
                    Port = "3306"
                };
                settings.EncryptionKey = SymmetricAlgorithm.Create(settings.EncryptionAlgorithm).Key.ToHexString();
                settings.HashKey = HMAC.Create(settings.HashAlgorithm).Key.ToHexString();
                _shellSettingsManager.SaveSettings(settings);
            }

            try
            {
                var context = CreateShellContext(settings);
                ActivateShell(context);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal()) throw;
                Logger.Error(ex, "A tenant could not be started: " + settings.Name);
            }
        }

        private ShellContext CreateShellContext(ShellSettings settings)
        {
            Logger.Debug("Creating shell context for tenant {0}.", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        private void ActivateShell(ShellContext context)
        {
            context.Shell.Activate();
            _shellContext = context;
        }
    }
}