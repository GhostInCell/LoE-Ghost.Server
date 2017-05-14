using Ghost.Server.Core;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Ghost.Server
{
    public class ServerInstance
    {       
        static ServerInstance()
        {
            RuntimeTypeModel.Default.Add(typeof(Vector3), false).Add("X", "Y", "Z");
        }

        private MasterServer _master;
        private List<MapServer> _maps;
        private CharServer _characters;
        private Dictionary<string, Action<string[]>> _cmdList;
        public bool IsRunning { get; private set; }
        public ServerInstance()
        {
            _maps = new List<MapServer>();
            _cmdList = new Dictionary<string, Action<string[]>>();
        }
        public void Stop()
        {
            foreach (var item in _maps)
                item.Stop();
            _maps.Clear();
            _characters?.Stop();
            _master?.Stop();
        }

        public async Task<bool> Startup()
        {
            if (await LoadAsync())
            {
                if (Configs.Get<bool>(Configs.Server_Master))
                    (_master = new MasterServer()).Start();
                if (Configs.Get<bool>(Configs.Server_Characters))
                    (_characters = new CharServer()).Start();
                if (Configs.Get<bool>(Configs.Server_AllMaps))
                {
                    foreach (var item in DataMgr.SelectAllMaps())
                    {
                        var mapServer = new MapServer(item);
                        _maps.Add(mapServer = new MapServer(item));
                        mapServer.Start();
                    }
                }
                else
                {
                    var list = Configs.Get<string>(Configs.Server_MapList).Split(';');
                    foreach (var item in list)
                    {
                        if (int.TryParse(item, out var id) && DataMgr.Select(id, out DB_Map map))
                        {
                            var mapServer = new MapServer(map);
                            _maps.Add(mapServer);
                            mapServer.Start();
                        }
                    }
                }
                IsRunning = true;
                InitializeCMD();
                return true;
            }
            return false;
        }
        public void DoCommand(string command)
        {
            string[] cmdArgs = null;
            int index = command.IndexOf(' ');
            if (index > 0)
            {
                cmdArgs = command.Substring(index + 1).Split(' ').Select(x => x.Trim()).ToArray();
                command = command.Remove(index);
            }
            if (_cmdList.TryGetValue(command, out var cmdAction))
                cmdAction(cmdArgs);
            cmdArgs = null;
        }

        private async Task<bool> LoadAsync()
        {
            if (await ServerDB.PingAsync())
                ServerLogger.LogInfo($"Connected to database");
            else
            {
                ServerLogger.LogError($"Couldn't connect to database: {Environment.NewLine}{ServerDB.ConnectionString}");
                return false;
            }
            await DataMgr.LoadAllAsync();
            ServerLogger.LogInfo($"Data loaded: {DataMgr.Info}");
            return true;
        }

        private void InitializeCMD()
        {
            #region Map
            _cmdList["map"] = async (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    switch (args[0])
                    {
                        case "reload":
                            if (args.Length >= 3)
                            {
                                switch (args[1])
                                {
                                    case "objects":
                                        int id;
                                        if (int.TryParse(args[2], out id))
                                        {
                                            var maps = _maps.Where(x => x.Map.Id == id).ToArray();
                                            if (maps.Length > 0)
                                            {
                                                await DataMgr.LoadAllAsync();
                                                foreach (var item in maps)
                                                    item.Objects.Reload();
                                            }
                                            else Console.WriteLine($"Error: can't find maps with id {id}");
                                        }
                                        else Console.WriteLine("Using: map reload objects mapID");
                                        break;
                                    default:
                                        Console.WriteLine("Using: map reload objects mapID");
                                        break;
                                }
                            }
                            else Console.WriteLine("Using: map reload objects mapID");
                            break;
                    }
                }
                else Console.WriteLine("Using: map reload args");
            };
            #endregion
            #region User
            _cmdList["user"] = async (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    switch (args[0])
                    {
                        case "ban":
                            if (_master == null)
                            {
                                Console.WriteLine("Only Master instance can ban users!");
                                return;
                            }
                            {

                                int id = -1;
                                int time = -1;
                                if (args.Length < 4 || !int.TryParse(args[1], out id) || !int.TryParse(args[2], out time))
                                    Console.WriteLine("Using: user ban id time reason");
                                else
                                {
                                    var user = await ServerDB.SelectUserAsync(id);
                                    if (user.IsEmpty)
                                        Console.WriteLine($"Error: can't find user whith {id}");
                                    else if (!await ServerDB.CreateBanAsync(id, null, BanType.Ban, -1, time, args[3]))
                                        Console.WriteLine($"Error: can't create ban entry");
                                    else
                                    {
                                        if (_master.TryGetByUserId(id, out var player))
                                            player.Player.Disconnect($"Congratulation!{Environment.NewLine}You're Banned!");
                                        Console.WriteLine($"User {user.Name} banned");
                                    }
                                }
                                break;
                            }
                        case "unban":
                            {
                                int id = -1;
                                if (args.Length < 2 || !int.TryParse(args[1], out id))
                                    Console.WriteLine("Using: user unban id");
                                else
                                {
                                    var user = await ServerDB.SelectUserAsync(id);
                                    if (user.IsEmpty)
                                        Console.WriteLine($"Error: can't find user whith {id}");
                                    else if (!await ServerDB.DeleteBanAsync(id, null, BanType.Ban))
                                        Console.WriteLine($"Error: can't delete ban entry");
                                    else Console.WriteLine($"User {user.Name} unbanned");
                                }
                                break;
                            }
                        case "mute":
                            {
                                int id = -1;
                                int time = -1;
                                if (args.Length < 4 || !int.TryParse(args[1], out id) || !int.TryParse(args[2], out time))
                                    Console.WriteLine("Using: user mute id time reason");
                                else
                                {
                                    var user = await ServerDB.SelectUserAsync(id);
                                    if (user.IsEmpty)
                                        Console.WriteLine($"Error: can't find user whith {id}");
                                    else if (!await ServerDB.CreateBanAsync(id, null, BanType.Mute, -1, time, args[3]))
                                        Console.WriteLine($"Error: can't create mute entry");
                                    else Console.WriteLine($"User {user.Name} muted");
                                }
                                break;
                            }
                        case "unmute":
                            {
                                int id = -1;
                                if (args.Length < 2 || !int.TryParse(args[1], out id))
                                    Console.WriteLine("Using: user unmute id");
                                else
                                {
                                    var user = await ServerDB.SelectUserAsync(id);
                                    if (user.IsEmpty)
                                        Console.WriteLine($"Error: can't find user whith {id}");
                                    else if (!await ServerDB.DeleteBanAsync(id, null, BanType.Mute))
                                        Console.WriteLine($"Error: can't delete mute entry");
                                    else Console.WriteLine($"User {user.Name} unmuted");
                                }
                                break;
                            }
                        case "create":
                            byte access = 1;
                            if (args.Length < 3 || (args.Length > 3 && !byte.TryParse(args[3], out access)))
                                Console.WriteLine("Using: user create login password <access>");
                            else if (!await ServerDB.CreateUserAsync(args[1], args[2], access))
                                Console.WriteLine($"Error: can't create user {args[1]}:{args[2]}");
                            else
                                Console.WriteLine($"Created user {args[1]}");
                            break;
                        case "info":
                            {
                                int id = -1;
                                if (args.Length < 2 || !int.TryParse(args[1], out id))
                                    Console.WriteLine("Using: user info id");
                                else
                                {
                                    var user = await ServerDB.SelectUserAsync(id);
                                    if (user.IsEmpty)
                                        Console.WriteLine($"Error: can't find user whith {id}");
                                    else
                                        Console.WriteLine($"User {id}, {user.Name} access {user.Access}, {(user.Session == null ? "offline" : (_master?.IsOnline(id) ?? false ? "online" : "offline/undefined"))}");
                                }
                                break;
                            }
                        case "list":
                            {
                                if (_master != null)
                                {
                                    Console.WriteLine("Online[Global]:");
                                    foreach (var item in _master.GetPlayers())
                                    {
                                        var user = item.User;
                                        var @char = item.Char;
                                        if (user != null)
                                            Console.WriteLine($"User {user.ID}, \"{user.Name}\", access {user.Access}{(@char != null ? $", char level {@char.Level}, \"{@char.Pony.Name}\", map {@char.Map}" : string.Empty)}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Online[Local]:");
                                    foreach (var item in _characters.GetPlayers())
                                    {
                                        var user = item.User;
                                        if (user != null)
                                            Console.WriteLine($"User {user.ID}, {user.Name} access {user.Access}, characters screen");
                                    }
                                    foreach (var map in _maps)
                                    {
                                        foreach (var item in map.GetPlayers())
                                        {
                                            var user = item.User;
                                            var @char = item.Char;
                                            if (user != null)
                                                Console.WriteLine($"User {user.ID}, \"{user.Name}\", access {user.Access}{(@char != null ? $", char level {@char.Level}, \"{@char.Pony.Name}\", map {@char.Map}" : string.Empty)}");
                                        }
                                    }
                                }
                                break;
                            }
                        default:
                            Console.WriteLine("Using: user ban|unban|mute|unmute|info|create|list args");
                            break;
                    }
                }
                else Console.WriteLine("Using: user ban|unban|mute|unmute|info|create|list args");
            };
            #endregion
            #region Data
            _cmdList["data"] = async (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    if (_master != null)
                    {
                        switch (args[0])
                        {
                            case "create":
                                if (args.Length >= 2)
                                {
                                    switch (args[1])
                                    {
                                        case "npc":
                                            if (args.Length == 8 && ushort.TryParse(args[2], out var level) && byte.TryParse(args[3], out var flags) && 
                                            ushort.TryParse(args[4], out var dialog) && byte.TryParse(args[5], out var index) && ushort.TryParse(args[6], out var movement))
                                            {
                                                try
                                                {
                                                    var id = await ServerDB.CreateNpcAsync(level, flags, dialog, index, movement, args[7].ToPonyData());
                                                    if (id > 0)
                                                        Console.WriteLine($"new NPC id {id} created!");
                                                    else Console.WriteLine($"Error: can't create new npc");
                                                }
                                                catch
                                                {
                                                    Console.WriteLine($"Error: bad ponycode");
                                                }
                                            }
                                            else
                                                Console.WriteLine("Using: data create npc level flags dialogID dialogIndex movementID ponycode");
                                            break;
                                        default:
                                            Console.WriteLine("Using: data create npc args");
                                            break;
                                    }
                                }
                                else Console.WriteLine("Using: data create npc args");
                                break;
                        }
                    }
                    else Console.WriteLine("Error: can't find master server in this instance");
                }
                else Console.WriteLine("Using: data create args");
            };
            #endregion
            #region Payer
            _cmdList["player"] = (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    if (_master != null)
                    {
                        ushort id;
                        switch (args[0])
                        {
                            case "items":
                                if (args.Length >= 2)
                                {
                                    switch (args[1])
                                    {
                                        case "clear":
                                            if (args.Length >= 3 && ushort.TryParse(args[2], out id))
                                            {
                                                var pClass = _master[id];
                                                if (pClass != null && pClass.OnMap)
                                                    pClass.Object.Player.Items.RemoveAllItems();
                                                else Console.WriteLine($"Error: can't find player {id}");
                                            }
                                            else
                                                Console.WriteLine("Using: player items clear playerID");
                                            break;
                                        case "print":
                                            if (args.Length >= 3 && ushort.TryParse(args[2], out id))
                                            {
                                                var pClass = _master[id];
                                                if (pClass != null && pClass.OnMap)
                                                    foreach (var item in pClass.Char.Data.Items)
                                                        Console.WriteLine($"{item.Key:X4}[{item.Value.Item.Id:X8}:{item.Value.Amount:X8}]");
                                                else Console.WriteLine($"Error: can't find player {id}");
                                            }
                                            else
                                                Console.WriteLine("Using: player items print playerID");
                                            break;
                                        default:
                                            Console.WriteLine("Using: player items clear|print args");
                                            break;
                                    }
                                }
                                else Console.WriteLine("Using: player items clear|print args");
                                break;
                            case "dialogs":
                                if (args.Length >= 2)
                                {
                                    switch (args[1])
                                    {
                                        case "clear":
                                            if (args.Length >= 3 && ushort.TryParse(args[2], out id))
                                            {
                                                var pClass = _master[id];
                                                if (pClass != null && pClass.OnMap)
                                                    pClass.Char.Data.Dialogs.Clear();
                                                else Console.WriteLine($"Error: can't find player {id}");
                                            }
                                            else
                                                Console.WriteLine("Using: player dialogs clear playerID");
                                            break;
                                        case "print":
                                            if (args.Length >= 3 && ushort.TryParse(args[2], out id))
                                            {
                                                var pClass = _master[id];
                                                if (pClass != null && pClass.OnMap)
                                                    foreach (var item in pClass.Char.Data.Dialogs)
                                                        Console.WriteLine($"{item.Key:X8}:{item.Value:X4}");
                                                else Console.WriteLine($"Error: can't find player {id}");
                                            }
                                            else
                                                Console.WriteLine("Using: player dialogs print playerID");
                                            break;
                                        default:
                                            Console.WriteLine("Using: player dialogs clear|print args");
                                            break;
                                    }
                                }
                                else Console.WriteLine("Using: player dialogs clear|print args");
                                break;
                            case "kill":
                                if (args.Length >= 1 && ushort.TryParse(args[1], out id))
                                {
                                    var pClass = _master[id];
                                    if (pClass != null && pClass.OnMap)
                                        (pClass.RoomPlayer as MapPlayer).Object.Despawn();
                                    else Console.WriteLine($"Error: can't find player {id}");
                                }
                                else Console.WriteLine("Using: player kill playerID");
                                break;
                            default:
                                Console.WriteLine("Using: player kill|dialogs|items args");
                                break;
                        }
                    }
                    else Console.WriteLine("Error: can't find master server in this instance");
                }
                else Console.WriteLine("Using: player kill|dialogs|items args");
            };
            #endregion
            #region Status
            _cmdList["status"] = (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    switch (args[0])
                    {
                        case "servers":
                            if (_master != null) Console.WriteLine(_master.Status);
                            if (_characters != null) Console.WriteLine(_characters.Status);
                            foreach (var item in _maps) Console.WriteLine(item.Status);
                            break;
                        case "player":
                            ushort player;
                            if (args.Length >= 2 && ushort.TryParse(args[1], out player))
                            {
                                if (_master != null)
                                {
                                    var pClass = _master[player];
                                    if (pClass != null && pClass.RoomPlayer != null)
                                        Console.WriteLine(pClass.RoomPlayer.Status);
                                    else Console.WriteLine($"Error: can't determine status for {player}");
                                }
                                else Console.WriteLine("Error: can't find master server in this instance");
                            }
                            else Console.WriteLine("Using: status player id");
                            break;
                        default:
                            Console.WriteLine("Using: status servers|player args");
                            break;
                    }
                }
                else Console.WriteLine("Using: status servers|player args");
            };
            #endregion
            #region Object
            _cmdList["object"] = async (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    if (_master != null)
                    {
                        switch (args[0])
                        {
                            case "create":
                                if (args.Length >= 3 && args[1] == "at")
                                {
                                    switch (args[2])
                                    {
                                        case "player":
                                            if (args.Length >= 9 && ushort.TryParse(args[3], out var id) && ushort.TryParse(args[4], out var guid) && int.TryParse(args[5], out var objectID) &&
                                            byte.TryParse(args[6], out var type) && byte.TryParse(args[7], out var flags) && float.TryParse(args[8], out var time))
                                            {
                                                var pClass = _master[id];
                                                if (pClass != null && pClass.OnMap)
                                                {
                                                    if (args.Length < 10 || !int.TryParse(args[09], out var data01)) data01 = -1;
                                                    if (args.Length < 11 || !int.TryParse(args[10], out var data02)) data02 = -1;
                                                    if (args.Length < 12 || !int.TryParse(args[11], out var data03)) data03 = -1;
                                                    if (await ServerDB.CreateObjectAtAsync(pClass.Object, pClass.User.Map, guid, objectID, type, flags, time, data01, data02, data03))
                                                        Console.WriteLine($"Object [{guid}:{objectID}] created at map {pClass.User.Map} pos {pClass.Object.Position}");
                                                    else Console.WriteLine($"Error: can't create object [{guid}:{objectID}]");
                                                }
                                                else Console.WriteLine($"Error: can't determine position for {id}");
                                            }
                                            else
                                                Console.WriteLine("Using: object create at player playerID guid objectID type flags time dataArgs");
                                            break;
                                        default:
                                            Console.WriteLine("Using: object create at player args");
                                            break;
                                    }
                                }
                                else Console.WriteLine("Using: object create at player args");
                                break;
                        }
                    }
                    else Console.WriteLine("Error: can't find master server in this instance");
                }
                else Console.WriteLine("Using: object create args");
            };
            #endregion
            #region Restart
            _cmdList["restart"] = async (string[] args) =>
            {
                if (args?.Length >= 1 && args[0] != "?")
                {
                    switch (args[0])
                    {
                        case "all":
                            _master?.Stop();
                            _characters?.Stop();
                            foreach (var item in _maps) item.Stop();
                            await DataMgr.LoadAllAsync();
                            _master?.Start();
                            _characters?.Start();
                            foreach (var item in _maps) item.Start();
                            break;
                        case "maps":
                            foreach (var item in _maps) item.Restart();
                            break;
                    }
                }
                else Console.WriteLine("Using: restart all|maps");
            };
            #endregion
            #region Small CMD
            _cmdList["exit"] = (string[] args) => { IsRunning = false; };
            _cmdList["clear"] = (string[] args) => { Console.Clear(); };
            _cmdList["help"] = (string[] args) =>
            {
                Console.WriteLine("Available commands:");
                foreach (var item in _cmdList.Keys) Console.WriteLine(item);
            };
            #endregion
        }
    }
}