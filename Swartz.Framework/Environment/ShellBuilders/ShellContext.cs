using System;
using Autofac;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders.Models;

namespace Swartz.Environment.ShellBuilders
{
    public class ShellContext : IDisposable
    {
        private bool _disposed;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public ILifetimeScope LifetimeScope { get; set; }
        public IWebShell Shell { get; set; }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    LifetimeScope.Dispose();
                }

                Settings = null;
                Blueprint = null;
                Shell = null;

                _disposed = true;
            }
        }

        ~ShellContext()
        {
            Dispose(false);
        }
    }
}