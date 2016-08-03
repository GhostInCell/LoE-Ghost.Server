using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ghost.Server.Core.Servers
{
    public class MasterServer : IServer
    {
        private static readonly object _lock = new object();
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
        private NetworkConfiguration _cfg;
        private Dictionary<int, MasterPlayer> _users;
        private Dictionary<ushort, MasterPlayer> _players;

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
                    $"Rooms: {_server?.RoomCount}/{_server?.Configuration.MaximumRooms}; Players: {_players.Count}/{_server?.Configuration.MaximumPlayers}";
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
            get { MasterPlayer entry; _players.TryGetValue(index, out entry); return entry; }
        }
        public MasterServer()
        {
            _users = new Dictionary<int, MasterPlayer>();
            _players = new Dictionary<ushort, MasterPlayer>();
            ServersMgr.Add(this);
#if DEBUG
            if (!(Debug.Logger is DefaultConsoleLogger))
                Debug.Logger = new DefaultConsoleLogger();
#endif
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
            _players.Clear();
            _server.Shutdown();
            _cfg = null;
            _server = null;
            Thread.Sleep(50);
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
            if (_running) Stop(); ReloadCFG();
            Start();
        }
        public bool IsOnline(int id)
        {
            lock (_lock) return _users.ContainsKey(id);
        }
        public bool TryGetById(ushort id, out MasterPlayer player)
        {
            lock (_lock) return _players.TryGetValue(id, out player);
        }
        public bool TryGetByUserId(int id, out MasterPlayer player)
        {
            lock (_lock)return _users.TryGetValue(id, out player);
        }
        public bool TryGetByName(string name, out MasterPlayer player)
        {
            lock (_lock) player = _players.Values.FirstOrDefault(x => x.User.Name == name);
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
        #region RPC Handlers
        private void RPC_255(NetMessage message)
        {
            MasterPlayer player;
            var id = message.ReadUInt16();
            var reason = message.ReadString();
            if (TryGetById(id, out player))
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
            Room[] rooms; int count;
            obj.ClearSubscriptions();
            if (_server.TryGetRooms(obj.RoomId, out rooms) && (count = rooms.Sum(x => x.MaxPlayers - x.PlayerCount)) > 0)
                foreach (var item in obj.Players)
                {
                    if (--count >= 0)
                        item.ChangeRoom(obj.RoomId);
                    else item.Disconnect("Room shut down");
                }
            ServerLogger.LogServer(this, $"Room {obj.RoomId.Normalize(Constants.MaxServerName)}[{obj.Guid}] removed");
        }
        private void MasterServer_PlayerAdded(Player obj)
        {
            var player = new MasterPlayer(obj, this);
            lock (_lock)
            {
                _players[obj.Id] = player;
                _users[player.User.ID] = player;
            }
            obj.ChangeRoom(Constants.Characters);
        }
        private void MasterServer_PlayerRemoved(Player obj)
        {
            MasterPlayer player;
            lock (_lock)
            {
                if (_players.TryGetValue(obj.Id, out player))
                {
                    _players.Remove(obj.Id);
                    _users.Remove(player.User.ID);
                    player.Destroy();
                }
            }
        }
        private INetSerializable MasterServer_ConstructNetData()
        {
            return new UserData();
        }
        private void MasterServer_VerifyPlayer(Player arg1, NetMessage arg2)
        {
            UserData sr_user = arg1.TnUser<UserData>(); DB_User user;
            var Name = arg2.ReadString();
            var SID = arg2.ReadString();
            var id = arg2.ReadInt32();
            MasterPlayer player;
            if (_users.ContainsKey(id) && TryGetByUserId(id, out player))
                player.Player.Disconnect("Only one session!");
            if (ServerDB.SelectUser(id, out user))
            {
                if (user.SID == SID && user.Name == Name)
                {
                    sr_user.ID = user.ID;
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