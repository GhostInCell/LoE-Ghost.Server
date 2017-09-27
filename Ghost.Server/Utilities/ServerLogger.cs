﻿using Ghost.Server.Utilities.Interfaces;
using System;
using System.Diagnostics;

namespace Ghost.Server.Utilities
{
    public static class ServerLogger
    {
        private const string m_info = "Info   ";
        private const string m_debug = "Debug  ";
        private const string m_error = "Error  ";
        private const string m_warning = "Warning";
        private const string m_verbose = "Verbose";
        
        public static void Log(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]: {msg}");
        }
        public static void LogInfo(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_info}: {msg}");
        }
        public static void LogError(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_error}: {msg}");
        }
        [Conditional("DEBUG")]
        public static void LogDebug(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_debug}: {msg}");
        }
        public static void LogWarning(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_warning}: {msg}");
        }
        [Conditional("VERBOSE")]
        public static void LogVerbose(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{m_verbose}: {msg}");
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
        public static void LogChat(string user, string pony, ChatType type, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][{type}][{user}:{pony}]: {msg}");
        }
        public static void LogLocalChat(IPlayer player, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}][{player.Server.ID}][{player.User.Name}]: {msg}");
        }
    }
}