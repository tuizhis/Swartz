using System;
using System.Configuration;
using System.IO;
using log4net;
using log4net.Config;

namespace Swartz.Logging
{
    public class SwartzLog4NetFactory : ILoggerFactory
    {
        public static bool IsFileWatched;

        public SwartzLog4NetFactory() : this(ConfigurationManager.AppSettings["log4net.Config"])
        {
        }

        public SwartzLog4NetFactory(string configFilename)
        {
            if (!IsFileWatched && !string.IsNullOrWhiteSpace(configFilename))
            {
                XmlConfigurator.ConfigureAndWatch(GetConfigFile(configFilename));
                IsFileWatched = true;
            }
        }

        protected static FileInfo GetConfigFile(string fileName)
        {
            if (Path.IsPathRooted(fileName))
            {
                return new FileInfo(fileName);
            }
            return new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
        }

        public ILogger CreateLogger(Type type)
        {
            return new SwartzLog4NetLogger(LogManager.GetLogger(type));
        }
    }
}