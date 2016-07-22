using Ghost.Server.Utilities.Interfaces;
using System;

namespace Ghost.Server.Utilities
{
    public static class ServerLogger
    {
        private const string m_info = "Info";
        private const string m_warn = "Warn";
        private const string m_error = "Error";
        public static void Log(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]: {msg}");
        }
        public static void LogInfo(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_info}: {msg}");
        }
        public static void LogWarn(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_warn}: {msg}");
        }
        public static void LogError(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_error}: {msg}");
        }
        public static void LogServer(IServer server, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{server.Name}[{server.ID}]: {msg}");
        }
        public static void LogServer(IPlayer player, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{player.Server.Name}[{player.Server.ID}]: {msg}");
        }
        public static void LogException(Exception exp, string msg = null)
        {
            var timeStamp = DateTime.Now.ToString("HH:mm:ss");
            if (msg != null) Console.WriteLine($"[{timeStamp}]{m_error}: {msg}");
            Console.WriteLine($"[{timeStamp}]{m_error}: Exception {exp.GetType().Name}");
            Console.WriteLine(exp.Message);
            Console.WriteLine(exp.StackTrace);
        }
        public static void LogChat(string user, ChatType type, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][{user}][{type}]: {msg}");
        }
        public static void LogLocalChat(IPlayer player, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][{player.User.Name}][{player.Server.ID}]: {msg}");
        }
    }
}