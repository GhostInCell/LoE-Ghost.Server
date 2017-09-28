using System;

namespace PNet
{
    /// <summary>
    /// Logger, but logs to nowhere
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>
        /// informational message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Info(string info)
        {
            
        }

        /// <summary>
        /// warning message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Warning(string info)
        {
            
        }

        /// <summary>
        /// error message
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Error(string info)
        {
            
        }

        /// <summary>
        /// exception
        /// </summary>
        /// <param name="info"></param>
        /// <param name="exception"></param>
        /// <param name="args"></param>
        public void Exception(Exception exception, string info)
        {
            
        }
    }
}
