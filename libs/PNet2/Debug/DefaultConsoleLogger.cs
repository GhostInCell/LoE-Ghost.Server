using System;

namespace PNet
{
    /// <summary>
    /// Console recipient for the log
    /// </summary>
    public sealed class DefaultConsoleLogger : ILogger
    {
        /// <summary>
        /// Info
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Info(string info)
        {
            Console.WriteLine(info);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Warning(string info)
        {
            Console.WriteLine(info);
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="info"></param>
        /// <param name="args"></param>
        public void Error(string info)
        {
            Console.WriteLine(info);
        }

        /// <summary>
        /// exception
        /// </summary>
        /// <param name="info"></param>
        /// <param name="exception"></param>
        /// <param name="args"></param>
        public void Exception(Exception exception, string info)
        {
            Console.WriteLine($"{info} ex: {exception}");
        }
    }
}
