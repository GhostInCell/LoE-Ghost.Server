using Ghost.Server.Core.Servers;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Concurrent;

namespace Ghost.Server.Mgrs
{
    public static class ServersMgr
    {
        private static MasterServer s_master;
        private readonly static ConcurrentDictionary<Guid, IServer> s_servers;

        public static bool Dedicated
        {
            get { return s_master == null; }
        }

        public static MasterServer Master
        {
            get { return s_master; }
        }

        static ServersMgr()
        {
            s_servers = new ConcurrentDictionary<Guid, IServer>();
        }

        public static void Add(IServer server)
        {
            if (s_servers.TryAdd(server.Guid, server))
            {
                if (server is MasterServer master)
                    s_master = master;
            }
            else ServerLogger.LogError($"Duplicate entry guid {server.Guid} name {server.Name}");
        }

        public static bool Contains(Guid guid) => s_servers.ContainsKey(guid);

        public static void Remove(IServer server)
        {
            if (s_servers.TryRemove(server.Guid, out var result))
            {
                if (server is MasterServer)
                    s_master = null;
            }
            else ServerLogger.LogWarning($"Trying remove unregistered server guid {server.Guid} name {server.Name}");
        }

        public static IServer GetServer(Guid guid)
        {
            if (s_servers.TryGetValue(guid, out var server))
                return server;
            return null;
        }

        public static IPlayer GetPlayer(Guid guid, ushort id)
        {
            if (s_servers.TryGetValue(guid, out var result))
            {
                switch (result)
                {
                    case MapServer server: return server[id];
                    case CharServer server: return server[id];
                }
            }
            return null;
        }
    }
}