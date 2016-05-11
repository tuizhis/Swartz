using System.Collections.Generic;
using log4net.Appender;
using log4net.Util;

namespace Swartz.Logging
{
    public class SwartzFileAppender : RollingFileAppender
    {
        /// <summary>
        ///     The number of suffix attempts that will be made on each OpenFile method call.
        /// </summary>
        private const int Retries = 50;

        /// <summary>
        ///     Maximum number of suffixes recorded before a cleanup happens to recycle memory.
        /// </summary>
        private const int MaxSuffixes = 100;

        /// <summary>
        ///     Dictionary of already known suffixes (based on previous attempts) for a given filename.
        /// </summary>
        private static readonly Dictionary<string, int> Suffixes = new Dictionary<string, int>();

        /// <summary>
        ///     Opens the log file adding an incremental suffix to the filename if required due to an openning failure (usually,
        ///     locking).
        /// </summary>
        /// <param name="fileName">The filename as specified in the configuration file.</param>
        /// <param name="append">Boolean flag indicating weather the log file should be appended if it already exists.</param>
        protected override void OpenFile(string fileName, bool append)
        {
            lock (this)
            {
                var fileOpened = false;
                var completeFilename = GetNextOutputFileName(fileName);
                var currentFilename = fileName;

                if (Suffixes.Count > MaxSuffixes)
                {
                    Suffixes.Clear();
                }

                if (!Suffixes.ContainsKey(completeFilename))
                {
                    Suffixes[completeFilename] = 0;
                }

                var newSuffix = Suffixes[completeFilename];

                for (var i = 1; !fileOpened && i <= Retries; i++)
                {
                    try
                    {
                        if (newSuffix > 0)
                        {
                            currentFilename = $"{fileName}-{newSuffix}";
                        }

                        BaseOpenFile(currentFilename, append);

                        fileOpened = true;
                    }
                    catch
                    {
                        newSuffix = Suffixes[completeFilename] + 1;

                        LogLog.Error(typeof(SwartzFileAppender),
                            $"SwartzFileAppender: Failed to open [{fileName}]. Attempting [{fileName}-{newSuffix}] instead.");
                    }
                }

                Suffixes[completeFilename] = newSuffix;
            }
        }

        /// <summary>
        ///     Calls the base class OpenFile method. Allows this method to be mocked.
        /// </summary>
        /// <param name="fileName">The filename as specified in the configuration file.</param>
        /// <param name="append">Boolean flag indicating weather the log file should be appended if it already exists.</param>
        protected virtual void BaseOpenFile(string fileName, bool append)
        {
            base.OpenFile(fileName, append);
        }
    }
}