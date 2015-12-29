using Ghost.Server.Core.Servers;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Mgrs
{
    public static class ServersMgr
    {
        private static MasterServer _master;
        private static HashSet<Guid> _servers;
        private static Dictionary<Guid, MapServer> _mServers;
        private static Dictionary<Guid, CharServer> _cServers;
        public static bool Dedicated
        {
            get { return _master == null; }
        }
        public static MasterServer Master
        {
            get { return _master; }
        }
        static ServersMgr()
        {
            _servers = new HashSet<Guid>();
            _mServers = new Dictionary<Guid, MapServer>();
            _cServers = new Dictionary<Guid, CharServer>();
        }
        public static void Add(IRoom server)
        {
            lock (_servers)
            {
                if (_servers.Contains(server.Guid))
                    ServerLogger.LogError($"Duplicate entry guid {server.Guid} name {server.Name}");
                else if (server is MapServer)
                    _mServers.Add(server.Guid, server as MapServer);
                else if (server is CharServer)
                    _cServers.Add(server.Guid, server as CharServer);
                _servers.Add(server.Guid);
            }
        }
        public static void Add(IServer server)
        {
            lock (_servers)
            {
                if (_servers.Contains(server.Guid))
                    ServerLogger.LogError($"Duplicate entry guid {server.Guid} name {server.Name}");
                if (server is MasterServer)
                    _master = server as MasterServer;
                else if (server is MapServer)
                    _mServers.Add(server.Guid, server as MapServer);
                else if (server is CharServer)
                    _cServers.Add(server.Guid, server as CharServer);
                _servers.Add(server.Guid);
            }
        }
        public static bool Contains(Guid guid)
        {
            lock (_servers) return _servers.Contains(guid);
        }
        public static void Remove(IRoom server)
        {
            lock (_servers)
            {
                if (!_servers.Contains(server.Guid))
                    ServerLogger.LogWarn($"Trying remove unregistered server guid {server.Guid} name {server.Name}");
                else
                {
                    if (server is MapServer)
                        _mServers.Remove(server.Guid);
                    else if (server is CharServer)
                        _cServers.Remove(server.Guid);
                    _servers.Remove(server.Guid);
                }
            }
        }
        public static void Remove(IServer server)
        {
            lock (_servers)
            {
                if (!_servers.Contains(server.Guid))
                    ServerLogger.LogWarn($"Trying remove unregistered server guid {server.Guid} name {server.Name}");
                else
                {
                    if (server is MasterServer)
                        _master = null;
                    else if (server is MapServer)
                        _mServers.Remove(server.Guid);
                    else if (server is CharServer)
                        _cServers.Remove(server.Guid);
                    _servers.Remove(server.Guid);
                }
            }
        }
        public static IServer GetServer(Guid guid)
        {
            lock (_servers)
            {
                MapServer ms; CharServer cs;
                if (!_servers.Contains(guid)) return null;
                if (_master?.Guid == guid) return _master;
                else if (_mServers.TryGetValue(guid, out ms)) return ms;
                else if (_cServers.TryGetValue(guid, out cs)) return cs;
                else return null;
            }
        }
        public static IPlayer GetPlayer(Guid roomID, ushort id)
        {
            lock (_servers)
            {
                MapServer ms; CharServer cs;
                if (!_servers.Contains(roomID)) return null;
                else if (_cServers.TryGetValue(roomID, out cs)) return cs[id];
                else if (_mServers.TryGetValue(roomID, out ms)) return ms[id];
                else return null;
            }
        }
    }
}