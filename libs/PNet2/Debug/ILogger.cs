using System;

namespace PNet
{
    /// <summary>
    /// Interface for logging information
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// informational message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Info(string info);
        /// <summary>
        /// warning message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Warning(string info);
        /// <summary>
        /// error message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        void Error(string info);

        /// <summary>
        /// Exception
        /// </summary>
        /// <param name="info"></param>
        /// <param name="exception"></param>
        /// <param name="args"></param>
        void Exception(Exception exception, string info);
    }
}
