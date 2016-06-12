using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Swartz.Events;
using Swartz.Logging;

namespace Swartz.Exceptions
{
    public class DefaultExceptionPolicy : IExceptionPolicy
    {
        public DefaultExceptionPolicy()
        {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool HandleException(object sender, Exception exception)
        {
            if (IsFatal(exception))
            {
                return false;
            }

            if (sender is IEventBus && exception is SwartzFatalException)
            {
                return false;
            }

            Logger.Error(exception, "An unexpected exception was caught");
            return true;
        }

        private static bool IsFatal(Exception exception)
        {
            return exception is SwartzSecurityException || exception is StackOverflowException ||
                   exception is AccessViolationException || exception is AppDomainUnloadedException ||
                   exception is ThreadAbortException || exception is SecurityException || exception is SEHException;
        }
    }
}