using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ghost.Server.Core.Servers
{
    public class MasterServer : IServer
    {
        private bool _running;
        private PNetS.Server _server;
        private static readonly Guid _guid;
        private static readonly string _idStr;
        private static readonly string _guidStr;
        static MasterServer()
        {
            _guid = Guid.NewGuid();
            _guidStr = _guid.ToString();
            _idStr = _guidStr.Remove(8);
        }
        private Timer m_bans_timer;
        private NetworkConfiguration _cfg;
        private ConcurrentDictionary<int, MasterPlayer> m_users;
        private ConcurrentDictionary<ushort, MasterPlayer> m_players;

        public string ID
        {
            get
            {
                return _idStr;
            }
        }
        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }
        public string Name
        {
            get
            {
                return Constants.Master;
            }
        }
        public string Status
        {
            get
            {
                return $"{Name}: Status: {(_running ? "Running" : "Stopped")}; Port: {_server?.Configuration.PlayerListenPort}; Dispatcher: {_server?.Configuration.RoomListenPort}; " +
                    $"Rooms: {_server?.RoomCount}/{_server?.Configuration.MaximumRooms}; Players: {m_players.Count}/{_server?.Configuration.MaximumPlayers}";
            }
        }
        public PNetS.Server Server
        {
            get
            {
                return _server;
            }
        }
        public bool IsRunning
        {
            get
            {
                return _running;
            }
        }

        public MasterPlayer this[ushort index]
        {
            get { m_players.TryGetValue(index, out var entry); return entry; }
        }

        public MasterServer()
        {
            m_bans_timer = new Timer(DeleteBansTimer, null, Timeout.Infinite, Timeout.Infinite);
            m_users = new ConcurrentDictionary<int, MasterPlayer>();
            m_players = new ConcurrentDictionary<ushort, MasterPlayer>();
            ServersMgr.Add(this);
#if DEBUG
            if (!(Debug.Logger is DefaultConsoleLogger))
                Debug.Logger = new DefaultConsoleLogger();
#endif
            m_bans_timer.Change(0, Timeout.Infinite);
        }

        public void Stop()
        {
            if (_server == null || !_running) return;
            _running = false;
            _server.RoomAdded -= MasterServer_RoomAdded;
            _server.RoomRemoved -= MasterServer_RoomRemoved;
            _server.PlayerAdded -= MasterServer_PlayerAdded;
            _server.VerifyPlayer -= MasterServer_VerifyPlayer;
            _server.PlayerRemoved -= MasterServer_PlayerRemoved;
            _server.ConstructNetData -= MasterServer_ConstructNetData;
            foreach (var item in m_players.Values)
                item.Destroy();
            m_players.Clear();
            m_users.Clear();
            _server.Shutdown();
            _cfg = null;
            _server = null;
            Thread.Sleep(100);
            ServerLogger.LogServer(this, $"Stopped");
        }
        public void Start()
        {
            if (_server != null) return;
            _server = new PNetS.Server(); ReloadCFG();
            _server.RoomAdded += MasterServer_RoomAdded;
            _server.RoomRemoved += MasterServer_RoomRemoved;
            _server.PlayerAdded += MasterServer_PlayerAdded;
            _server.VerifyPlayer += MasterServer_VerifyPlayer;
            _server.PlayerRemoved += MasterServer_PlayerRemoved;
            _server.ConstructNetData += MasterServer_ConstructNetData;
            _server.Initialize(_cfg);
            ServerLogger.LogServer(this, $"Started");
            _running = true;
        }
        public void Restart()
        {
            if (_server == null) return;
            if (_running) Stop();
            Start();
        }
        public bool IsOnline(int id)
        {
            return m_users.ContainsKey(id);
        }
        public IEnumerable<MasterPlayer> GetPlayers()
        {
            return m_players.Select(x => x.Value);
        }
        public IEnumerable<MasterPlayer> FindPlayers(string request)
        {
            request = request.ToLowerInvariant();
            return GetPlayers()
                .Where(x => x.User != null && x.Char != null)
                .WhereIf(request != "all", x => x.User.Name.ToLowerInvariant().Contains(request) || x.Char.Pony.Name.ToLowerInvariant().Contains(request));
        }

        public bool TryGetById(ushort id, out MasterPlayer player)
        {
            return m_players.TryGetValue(id, out player);
        }
        public bool TryGetByUserId(int id, out MasterPlayer player)
        {
            return m_users.TryGetValue(id, out player);
        }
        public bool TryGetByName(string name, out MasterPlayer player)
        {
            player = GetPlayers().FirstOrDefault(x => x.User.Name == name);
            return player != null;
        }
        public bool TryGetByPonyName(string name, out MasterPlayer player)
        {
            player = GetPlayers().FirstOrDefault(x => x.Char?.Pony.Name == name);
            return player != null;
        }
        private void ReloadCFG()
        {
            _cfg = new NetworkConfiguration(maximumRooms: Configs.Get<int>(Configs.Server_MaxMaps),
                playerListenPort: Configs.Get<int>(Configs.Server_Players_Port),
                roomListenPort: Configs.Get<int>(Configs.Server_Maps_Port),
                roomHosts: Configs.Get<string>(Configs.Server_Maps_Hosts),
                maximumPlayers: Configs.Get<int>(Configs.Server_MaxPlayers));
        }

        private async void DeleteBansTimer(object state)
        {
            await ServerDB.DeleateAllOutdatedBansAsync();
            m_bans_timer.Change(15 * 60 * 1000, Timeout.Infinite);
        }

        #region RPC Handlers
        private void RPC_255(NetMessage message)
        {
            var id = message.ReadUInt16();
            var reason = message.ReadString();
            if (TryGetById(id, out var player))
                player.Player.Disconnect(reason);
        }
        #endregion
        #region Event Handlers
        private void MasterServer_RoomAdded(Room obj)
        {
            obj.SubscribeToRpc(255, RPC_255);
        }

        private void MasterServer_RoomRemoved(Room obj)
        {
            int count;
            obj.ClearSubscriptions();
            if (_server.TryGetRooms(obj.RoomId, out var rooms) && (count = rooms.Sum(x => x.MaxPlayers - x.PlayerCount)) > 0)
            {
                foreach (var item in obj.Players)
                {
                    if (--count >= 0)
                        item.ChangeRoom(obj.RoomId);
                    else item.Disconnect("Room shut down");
                }
            }
            ServerLogger.LogServer(this, $"Room {obj.RoomId.Normalize(Constants.MaxServerName)}[{obj.Guid}] removed");
        }

        private void MasterServer_PlayerAdded(Player obj)
        {
            var player = new MasterPlayer(obj, this);
            if (!m_players.TryAdd(obj.Id, player) || !m_users.TryAdd(player.User.ID, player))
            {
                ServerLogger.LogServer(this, $"Couldn't add player with id {obj.Id} user {player.User.ID}");
                obj.Disconnect("Something is terribly wrong!");
                player.Destroy();
                return;
            }
            obj.ChangeRoom(Constants.Characters);
        }

        private void MasterServer_PlayerRemoved(Player obj)
        {
            if (m_players.TryRemove(obj.Id, out var player) && m_users.TryRemove(player.User.ID, out player))
                player.Destroy();
            else ServerLogger.LogServer(this, $"Couldn't remove player with id {obj.Id}");
        }

        private INetSerializable MasterServer_ConstructNetData()
        {
            return new UserData();
        }

        private async void MasterServer_VerifyPlayer(Player arg1, NetMessage arg2)
        {
            UserData sr_user = arg1.TnUser<UserData>(); 
            var Name = arg2.ReadString();
            var Sid = arg2.ReadString();
            var id = arg2.ReadInt32();
            var time = DateTime.Now;
            var ban = await ServerDB.SelectBanAsync(id, arg1.EndPoint.Address, BanType.Ban, time);
            if (!ban.IsEmpty)
            {
                arg1.Disconnect($"You're Banned!{Environment.NewLine}" +
                    $"Reason: {ban.Reason}{Environment.NewLine}" +
                    $"Ban ends in: {ban.End - time:dd\\.hh\\:mm\\:ss}");
                return;
            }
            if (TryGetByUserId(id, out var player))
                player.Player.Disconnect("Only one session!");
            var user = await ServerDB.SelectUserAsync(id);
            if (!user.IsEmpty)
            {
                if (user.Session == Sid && user.Name == Name)
                {
                    sr_user.ID = user.Id;
                    sr_user.Name = user.Name;
                    sr_user.Access = user.Access;
                    arg1.AllowConnect();
                    return;
                }
                else
                    arg1.Disconnect("Access Denied!");
            }
            else arg1.Disconnect($"User {Name} not found!");
        }
        #endregion
    }
}