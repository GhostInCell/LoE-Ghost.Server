using PNet;
using System;

namespace PNetR
{
    /// <summary>
    /// Debug
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Reference to the actual log receiver
        /// </summary>
        public static ILogger Logger = new NullLogger();

        /// <summary>
        /// Info message
        /// </summary>
        /// <param name="value"></param>
        public static void Log(string value)
        {
            Logger.Info(value);
        }
        /// <summary>
        /// Error message
        /// </summary>
        /// <param name="value"></param>
        public static void LogError(string value)
        {
            Logger.Error(value);
        }
        /// <summary>
        /// Warning message
        /// </summary>
        /// <param name="value"></param>
        public static void LogWarning(string value)
        {
            Logger.Warning(value);
        }

        /// <summary>
        /// exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="value"></param>
        public static void LogException(Exception exception, string value)
        {
            Logger.Exception(exception, value);
        }

        /// <summary>
        /// exception
        /// </summary>
        /// <param name="exception"></param>
        public static void LogException(Exception exception)
        {
            Logger.Exception(exception, "");
        }
    }
}
