using System;
using System.Globalization;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Util;

namespace Swartz.Logging
{
    public class SwartzLog4NetLogger : ILogger
    {
        private static readonly Type DeclaringType = typeof(SwartzLog4NetLogger);

        public SwartzLog4NetLogger(log4net.Core.ILogger logger)
        {
            Logger = logger;
        }

        internal SwartzLog4NetLogger() { }

        internal SwartzLog4NetLogger(ILog log) : this(log.Logger)
        {
        }

        protected internal log4net.Core.ILogger Logger { get; set; }

        public bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return Logger.IsEnabledFor(Level.Debug);
                case LogLevel.Information:
                    return Logger.IsEnabledFor(Level.Info);
                case LogLevel.Warning:
                    return Logger.IsEnabledFor(Level.Warn);
                case LogLevel.Error:
                    return Logger.IsEnabledFor(Level.Error);
                case LogLevel.Fatal:
                    return Logger.IsEnabledFor(Level.Fatal);
            }
            return false;
        }

        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
            AddExtendedThreadInfo();
            if (args == null)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Logger.Log(DeclaringType, Level.Debug, format, exception);
                        break;
                    case LogLevel.Information:
                        Logger.Log(DeclaringType, Level.Info, format, exception);
                        break;
                    case LogLevel.Warning:
                        Logger.Log(DeclaringType, Level.Warn, format, exception);
                        break;
                    case LogLevel.Error:
                        Logger.Log(DeclaringType, Level.Error, format, exception);
                        break;
                    case LogLevel.Fatal:
                        Logger.Log(DeclaringType, Level.Fatal, format, exception);
                        break;
                }
            }
            else
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Logger.Log(DeclaringType, Level.Debug,
                            new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Information:
                        Logger.Log(DeclaringType, Level.Info,
                            new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Warning:
                        Logger.Log(DeclaringType, Level.Warn,
                            new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Error:
                        Logger.Log(DeclaringType, Level.Error,
                            new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Fatal:
                        Logger.Log(DeclaringType, Level.Fatal,
                            new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                }
            }
        }

        public override string ToString()
        {
            return Logger.ToString();
        }

        protected internal void AddExtendedThreadInfo()
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null)
                {
                    ThreadContext.Properties["Url"] = ctx.Request.Url.ToString();
                }
                else
                {
                    ThreadContext.Properties.Remove("Url");
                }
            }
            catch (Exception)
            {
                // can happen on cloud service for an unknown reason
            }
        }
    }
}