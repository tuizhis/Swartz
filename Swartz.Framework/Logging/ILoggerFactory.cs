using System;

namespace Swartz.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}