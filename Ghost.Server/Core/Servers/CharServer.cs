using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs;
using Ghost.Server.Scripts.Servers;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ghost.Server.Core.Servers
{
    public class CharServer : IRoom
    {
        private static int _staticPort;
        private static readonly string _name;
        static CharServer()
        {
            _staticPort = Configs.Get<int>(Configs.Char_Port);
            _staticPort--;
            _name = Constants.Characters.Normalize(Constants.MaxServerName);
        }
        private readonly int _port;
        private string _id;
        private Room _room;
        private bool _running;
        private Thread _rLoop;
        private int _maxPlayers;
        private CharServerScript _script;
        private NetworkConfiguration _cfg;
        private Dictionary<ushort, CharPlayer> _players;
        public string ID
        {
            get
            {
                return _id;
            }
        }
        public Guid Guid
        {
            get { return _room?.RoomId ?? Guid.Empty; }
        }
        public Room Room
        {
            get { return _room; }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public string Status
        {
            get
            {
                return $"{_name}[{_id}]: Status {_room.ServerStatus}; Port: {_port}; Players: {_players.Count}/{_maxPlayers};";
            }
        }
        public bool IsRunning
        {
            get { return _running && _room.ServerStatus == ConnectionStatus.Connected; }
        }
        public CharPlayer this[ushort index]
        {
            get { CharPlayer entry; _players.TryGetValue(index, out entry); return entry; }
        }
        public CharServer()
        {
            _port = Interlocked.Increment(ref _staticPort);
            _id = "########";
            _players = new Dictionary<ushort, CharPlayer>();
#if DEBUG
            if (!(Debug.Logger is DefaultConsoleLogger))
                Debug.Logger = new DefaultConsoleLogger();
#endif
        }
        public void Stop()
        {
            if (_room == null || !_running) return;
            ServersMgr.Remove(this);
            _running = false;
            _room.Shutdown();
            _rLoop.Join();
            Thread.Sleep(50);
            _room.PlayerAdded -= Room_PlayerAdded;
            _room.PlayerRemoved -= Room_PlayerRemoved;
            _room.ConstructNetData -= Room_ConstructNetData;
            _room.ServerStatusChanged -= Room_ServerStatusChanged;
            _players.Clear();
            _cfg = null;
            _room = null;
            _rLoop = null;
            _script = null;
        }
        public void Start()
        {
            if (_room != null || _running) return;
            CreateRoom();
            ServerLogger.LogServer(this, $" Started");
        }
        public void Restart()
        {
            if (_running) Stop();
            CreateRoom();
            ServerLogger.LogServer(this, $" Restarted");
        }
        #region Server Loop
        private void ServerLoop()
        {
            int delay = Configs.Get<int>(Configs.Map_Reconnect); if (delay <= 0) delay = 2000;
            int tick = _cfg.TickRate; if (tick <= 0) tick = 1;
            int reconnect = delay;
            while (_running)
            {
                if (_room.ServerStatus == ConnectionStatus.Disconnected)
                {
                    reconnect += tick;
                    if (reconnect >= delay) { _room.StartConnection(); reconnect = 0; }
                }
                _room.ReadQueue();
                Thread.Sleep(tick);
            }
        }
        #endregion
        #region Events Handlers
        private void Room_ServerStatusChanged()
        {
            if (_room.ServerStatus == ConnectionStatus.Connected)
            {
                ServersMgr.Add(this);
                _id = _room.RoomId.ToString().Normalize(8);
            }
            else ServersMgr.Remove(this);
            ServerLogger.LogServer(this, $" Status {_room.ServerStatus}");
        }
        private void Room_PlayerAdded(Player obj)
        {
            var player = new CharPlayer(obj, this);
            _players[obj.Id] = player;
            ServerLogger.LogServer(this, $" Player {obj.Id} added");
        }
        private void Room_PlayerRemoved(Player obj)
        {
            _players[obj.Id].Destroy();
            _players.Remove(obj.Id);
            ServerLogger.LogServer(this, $" Player {obj.Id} removed");
        }
        private INetSerializable Room_ConstructNetData()
        {
            return new UserData();
        }
        #endregion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateRoom()
        {
            _maxPlayers = Configs.Get<int>(Configs.Char_MaxPlayers);
            _cfg = new NetworkConfiguration(_maxPlayers, _port, Configs.Get<int>(Configs.Char_Tick), Constants.Characters, dispatcherAddress: Configs.Get<string>(Configs.Server_Host),
                userDefinedAuthData: $"{ServersMgr.Dedicated}@{GetType().Name}{(ServersMgr.Dedicated ? string.Empty : $"@{ServersMgr.Master.Guid}")}",
                dispatcherPort: Configs.Get<int>(Configs.Server_Maps_Port), listenAddress: Configs.Get<string>(Configs.Char_Host));
            _room = new Room(_cfg);
            _script = new CharServerScript(this);
            _room.PlayerAdded += Room_PlayerAdded;
            _room.PlayerRemoved += Room_PlayerRemoved;
            _room.ConstructNetData += Room_ConstructNetData;
            _room.ServerStatusChanged += Room_ServerStatusChanged;
            _rLoop = new Thread(ServerLoop);
            _rLoop.IsBackground = true;
            _running = true;
            _rLoop.Start();
        }
    }
}