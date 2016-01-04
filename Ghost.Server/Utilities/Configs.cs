using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Ghost.Server.Utilities
{
    public static class Configs
    {
        public const string ConfigFile = "loe_server.cfg";

        private const int DEF_Game_MaxChars = 5;
        private const int DEF_Game_SaveChar = 60;
        private const short DEF_Game_MaxLevel = 50;
        private const int DEF_Game_SpamDelay = 600;
        private const string DEF_Game_UnicornStart = "Canterlot";
        private const string DEF_Game_PegasusStart = "Cloudsdale";
        private const string DEF_Game_EarthPonyStart = "Ponydale";
        private const int DEF_Movement_SyncInterval = 200;
        private const int DEF_Server_MaxMaps = 256;
        private const bool DEF_Server_Master = true;
        private const bool DEF_Server_AllMaps = true;
        private const int DEF_Server_Maps_Port = 14001;
        private const int DEF_Server_MaxPlayers = 50000;
        private const bool DEF_Server_Characters = true;
        private const int DEF_Server_Players_Port = 14000;
        private const string DEF_Server_MapList = "1;2;3";
        private const string DEF_Server_Host = "127.0.0.1";
        private const string DEF_Server_Scripts = "\\Scripts\\";
        private const string DEF_Server_Maps_Hosts = "127.0.0.1";


        private const int DEF_Map_Tick = 66;
        private const int DEF_Map_Port = 14100;
        private const int DEF_Map_MaxPlayers = 100;
        private const int DEF_Map_Reconnect = 2000;
        private const string DEF_Map_Host = "127.0.0.1";

        private const int DEF_Char_Tick = 66;
        private const int DEF_Char_Port = 14010;
        private const int DEF_Char_MaxPlayers = 500;
        private const string DEF_Char_Host = "127.0.0.1";

        private const uint DEF_MySQL_Port = 3306;
        private const string DEF_MySQL_User = "loedata";
        private const string DEF_MySQL_Host = "127.0.0.1";
        private const string DEF_MySQL_Pass = "hMBSeLnHCGv6x9eD";
        private const string DEF_MySQL_Db = "legends_of_equestria";

        public const string MySQL_Db = "mysql_db";
        public const string MySQL_User = "mysql_user";
        public const string MySQL_Pass = "mysql_pass";
        public const string MySQL_Host = "mysql_host";
        public const string MySQL_Port = "mysql_port";
        public const string Char_Port = "char_port";
        public const string Char_Host = "char_host";
        public const string Char_Tick = "char_tick";
        public const string Char_MaxPlayers = "char_maxplayers";
        public const string Map_Port = "map_port";
        public const string Map_Host = "map_host";
        public const string Map_Tick = "map_tick";
        public const string Map_Reconnect = "map_reconnect";
        public const string Map_MaxPlayers = "map_maxplayers";
        public const string Game_SaveChar = "game_savechar";
        public const string Game_MaxChars = "game_maxchars";
        public const string Game_MaxLevel = "game_maxlevel";
        public const string Game_SpamDelay = "game_spamdelay";
        public const string Game_UnicornStart = "game_unicornstart";
        public const string Game_PegasusStart = "game_pegasusstart";
        public const string Game_EarthPonyStart = "game_earthponystart";
        public const string Movement_SyncInterval = "movement_syncinterval";
        public const string Server_Host = "server_host";
        public const string Server_Master = "server_master";
        public const string Server_Scripts = "server_scripts";
        public const string Server_AllMaps = "server_allmaps";
        public const string Server_MaxMaps = "server_maxmaps";
        public const string Server_MapList = "server_maplist";
        public const string Server_Maps_Port = "server_maps_port";
        public const string Server_MaxPlayers = "server_maxplayers";
        public const string Server_Maps_Hosts = "server_maps_hosts";
        public const string Server_Characters = "server_characters";
        public const string Server_Players_Port = "server_players_port";

        private static Dictionary<string, object> _cfgs;
        public static bool IsLoaded { get; private set; }
        static Configs()
        {
            _cfgs = new Dictionary<string, object>();
            if (!(IsLoaded = Load()))
                ServerLogger.LogError($"Couldn't create configuration file: {ConfigFile}");
            else
                ServerLogger.LogInfo($"Loaded configuration file: {ConfigFile}");
            Save();
        }
        public static bool Load()
        {
            try
            {
                if (_cfgs.Count > 0) _cfgs.Clear();
                if (!File.Exists(ConfigFile))
                {
                    ServerLogger.LogWarn($"Configuration file: {ConfigFile} not found. Creating default one ...");
                    return Create();
                }
                foreach (var item in File.ReadAllLines(ConfigFile))
                {
                    var index1 = item.IndexOf('@');
                    var index2 = item.IndexOf('=');
                    if (index1 > 0 && index2 > 0)
                        _cfgs[item.Remove(index1)] = Convert.ChangeType(item.Substring(index2 + 1).Trim(),
                            Type.GetType(item.Substring(index1 + 1, index2 - index1 - 1)));

                }
                return Create(true);
            }
#if !DEBUG
            catch
            {
#else
            catch (Exception exc)
            {

                ServerLogger.LogException(exc);
#endif
                return Create();
            }
        }
        public static void Save()
        {
            try
            {
                if (_cfgs.Count == 0) return;
                using (var cfgFile = File.OpenWrite(ConfigFile))
                using (var writer = new StreamWriter(cfgFile))
                {
                    foreach (var item in _cfgs)
                    {
                        var type = item.Value.GetType();
                        writer.WriteLine($"{item.Key}@{type.FullName}={item.Value.ToString()}");
                    }
                }
            }
#if !DEBUG
            catch
            {
#else
            catch (Exception exc)
            {

                ServerLogger.LogException(exc);
#endif
            }
        }
        private static bool Create(bool check = false)
        {
            try
            {

                foreach (var entry in typeof(Configs).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    if (entry.IsLiteral && !entry.IsInitOnly && entry.Name.StartsWith("DEF_"))
                    {
                        var name = entry.Name.Substring(4).ToLower();
                        if (check && _cfgs.ContainsKey(name)) continue;
                        _cfgs[name] = entry.GetValue(null);
                    }
                }
                return true;
            }
#if !DEBUG
            catch
            {
#else
            catch (Exception exc)
            {

                ServerLogger.LogException(exc);
#endif
                return false;
            }
        }
        public static T Get<T>(string name)
        {
            if (_cfgs.ContainsKey(name) && (_cfgs[name] is T))
                return (T)_cfgs[name];
            throw new KeyNotFoundException();
        }
        public static void Set(string name, int entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, bool entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, byte entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, uint entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, long entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, sbyte entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, ulong entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, short entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, float entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, ushort entry)
        {
            _cfgs[name] = entry;
        }
        public static void Set(string name, string entry)
        {
            _cfgs[name] = entry;
        }
    }
}