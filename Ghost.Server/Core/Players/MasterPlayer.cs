using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ghost.Server.Core.Players
{
    public class MasterPlayer : IPlayer
    {
        private static readonly int SpamDelay;
        static MasterPlayer()
        {
            SpamDelay = Configs.Get<int>(Configs.Game_SpamDelay);
        }
        private DB_Ban m_mute;
        private string m_name;
        private Player _player;
        private UserData _user;
        private DateTime _time;
        private UserSave _save;
        private Character _char;
        private IPlayer _rPlayer;
        private string _lastWhisper;
        private DateTime m_mute_chek;
        private FriendStatus _status;
        private MasterServer _server;
        public bool OnMap
        {
            get
            {
                return _rPlayer is MapPlayer;
            }
        }
        public UserSave Save
        {
            get
            {
                return _save;
            }
        }
        public Player Player
        {
            get
            {
                return _player;
            }
        }
        public UserData User
        {
            get
            {
                return _user;
            }
        }
        public string Status
        {
            get
            {
                return _rPlayer?.Status ?? $"Player[{_player.Id}|{_user.ID}:{_user.Name}]: at dedicated server {_player.Room.RoomId.Normalize(Constants.MaxServerName)}[{_server.Guid.ToString().Normalize(8)}]";
            }
        }
        public Character Char
        {
            get
            {
                return _char;
            }
        }
        public IServer Server
        {
            get
            {
                return _server;
            }
        }
        public WO_Player Object
        {
            get
            {
                return (_rPlayer as MapPlayer)?.Object;
            }
        }
        public IPlayer RoomPlayer
        {
            get
            {
                return _rPlayer;
            }
            set
            {
                _rPlayer = value;
            }
        }
        PNetR.Player IPlayer.Player
        {
            get
            {
                return _rPlayer?.Player;
            }
        }
        public MasterPlayer(Player player, MasterServer server)
        {
            _server = server;
            _player = player;
            _status = new FriendStatus();
            _user = player.TnUser<UserData>();
            _player.SubscribeRpcsOnObject(this);
            //if (!ServerDB.SelectUserSave(_user.ID, out _save) || _save == null)
            //{
            //    _save = new UserSave();
            //    if (!ServerDB.UpdateUserSave(_user.ID, _save))
            //        ServerLogger.LogError($"Couldn't save user[{_user.ID}] data");
            //}
            _player.NetUserDataChanged += Player_NetUserDataChanged;
            _player.FinishedSwitchingRooms += Player_FinishedSwitchingRooms;
        }
        public void Destroy()
        {
            //MasterPlayer target;
            _player.ClearSubscriptions();
            //ServerDB.UpdateUserSave(_user.ID, _save);
            _player.NetUserDataChanged -= Player_NetUserDataChanged;
            _player.FinishedSwitchingRooms -= Player_FinishedSwitchingRooms;
            //foreach (var item in _save.Friends)
            //{
            //    if (item.Value.Item1 == 1 && _server.TryGetByUserId(item.Key, out target))
            //        target.Player.UpdateFriend(_status.Fill(this, OnlineStatus.Offline));
            //}
            _char = null;
            _save = null;
            _user = null;
            _status = null;
            _player = null;
            _server = null;
            _rPlayer = null;
            _lastWhisper = null;
        }

        private void UpdateStatus()
        {
            MasterPlayer target = null; Tuple<byte, string, short, DateTime> state;
            if (_status.ID != 0)
            {
                switch (_status.Status)
                {
                    case OnlineStatus.Online:
                    case OnlineStatus.Offline:
                    case OnlineStatus.Blocker:
                    case OnlineStatus.Blockee:
                    case OnlineStatus.Incoming:
                        ServerLogger.LogInfo($"Player[{_player.Id}] received update friend status for user[{_status.ID}] status: {_status.Status}");
                        break;
                    case OnlineStatus.Remove:
                        if (_server.TryGetByUserId(_status.ID, out target))
                        {
                            _save.Friends.Remove(_status.ID);
                            target._save.Friends.Remove(_user.ID);
                            _player.UpdateFriend(_status.Fill(target, OnlineStatus.Remove));
                            target._player.UpdateFriend(_status.Fill(this, OnlineStatus.Remove));
                        }
                        else
                            ServerLogger.LogInfo($"Player[{_player.Id}] received update friend status for offline user[{_status.ID}] status: {_status.Status}");
                        break;
                    case OnlineStatus.Outgoing:
                        if (_server.TryGetByUserId(_status.ID, out target))
                        {
                            if (_save.Friends.TryGetValue(target._user.ID, out state))
                            {
                                _save.Friends[target._user.ID] =
                                   new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Online, target._char.Pony.Name, (short)target._char.Pony.CutieMark0, DateTime.Now);
                                target._save.Friends[_user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Online, _char.Pony.Name, (short)_char.Pony.CutieMark0, DateTime.Now);
                                _player.UpdateFriend(_status.Fill(target, OnlineStatus.Online));
                                target._player.UpdateFriend(_status.Fill(this, OnlineStatus.Online));
                            }
                            else
                            {
                                _save.Friends[target._user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Outgoing, target._char.Pony.Name, (short)target._char.Pony.CutieMark0, DateTime.Now);
                                target._save.Friends[_user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Incoming, _char.Pony.Name, (short)_char.Pony.CutieMark0, DateTime.Now);
                                target._player.UpdateFriend(_status.Fill(this, OnlineStatus.Incoming));
                                _player.UpdateFriend(_status.Fill(target, OnlineStatus.Outgoing));
                            }
                        }
                        else
                            ServerLogger.LogInfo($"Player[{_player.Id}] received update friend status for offline user[{_status.ID}] status: {_status.Status}");
                        break;
                }
                //ServerDB.UpdateUserSave(_user.ID, _save);
                //if (target != null)
                //    ServerDB.UpdateUserSave(target._user.ID, target._save);
            }
            else if (_status.PlayerID != 0)
            {
                switch (_status.Status)
                {
                    case OnlineStatus.Online:
                    case OnlineStatus.Remove:
                    case OnlineStatus.Offline:
                    case OnlineStatus.Blocker:
                    case OnlineStatus.Blockee:
                    case OnlineStatus.Incoming:
                        ServerLogger.LogInfo($"Player[{_player.Id}] received update friend status for player[{_status.PlayerID}] status: {_status.Status}");
                        break;
                    case OnlineStatus.Outgoing:
                        if (_server.TryGetById(_status.PlayerID, out target))
                        {
                            if (_save.Friends.TryGetValue(target._user.ID, out state))
                            {
                                _save.Friends[target._user.ID] =
                                   new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Online, target._char.Pony.Name, (short)target._char.Pony.CutieMark0, DateTime.Now);
                                target._save.Friends[_user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Online, _char.Pony.Name, (short)_char.Pony.CutieMark0, DateTime.Now);
                                _player.UpdateFriend(_status.Fill(target, OnlineStatus.Online));
                                target._player.UpdateFriend(_status.Fill(this, OnlineStatus.Online));
                            }
                            else
                            {
                                _save.Friends[target._user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Outgoing, target._char.Pony.Name, (short)target._char.Pony.CutieMark0, DateTime.Now);
                                target._save.Friends[_user.ID] =
                                    new Tuple<byte, string, short, DateTime>((byte)OnlineStatus.Incoming, _char.Pony.Name, (short)_char.Pony.CutieMark0, DateTime.Now);
                                target._player.UpdateFriend(_status.Fill(this, OnlineStatus.Incoming));
                                _player.UpdateFriend(_status.Fill(target, OnlineStatus.Outgoing));
                            }
                        }
                        break;
                }
            }
        }

        private bool DoCommand(CommandArgs args)
        {
            switch (args.Command)
            {
                case "ban":
                    if (_user.Access <= AccessLevel.Moderator) break;
                    {
                        if (args.TryGet(out string request) && args.TryGet(out int time))
                        {
                            if (args.TryGet(out string reason))
                                BanPlayer(request, false, time, BanType.Ban, reason);
                            else BanPlayer(request, false, time, BanType.Ban);
                        }
                        else _player.SystemMsg($":ban \"Player or User Name\" \"time in minutes\" \"reason\" { Environment.NewLine}");
                        return true;
                    }
                case "banip":
                    if (_user.Access <= AccessLevel.Moderator) break;
                    {
                        if (args.TryGet(out string request) && args.TryGet(out int time))
                        {
                            if (args.TryGet(out string reason))
                                BanPlayer(request, true, time, BanType.Ban, reason);
                            else BanPlayer(request, true, time, BanType.Ban);
                        }
                        else _player.SystemMsg($":ban \"Player or User Name\" \"time in minutes\" \"reason\" { Environment.NewLine}");
                        return true;
                    }
            }
            return false;
        }

        private async void BanPlayer(string request, bool includeIp, int time, BanType type, string reason = "Unspecified")
        {
            var players = _server.FindPlayers(request.ToLowerInvariant());
            var count = players.Count();
            if (count > 1)
            {
                _player.SystemMsg($"Found more then one player: {Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, players.Select(x => $"User '{x.User.Name}', Pony '{x.Char.Pony.Name}'"))}");
            }
            else if(count == 0)
                _player.SystemMsg($"Search result for {request} returns nothing!");
            else
            {
                var player = players.First();
                if (time != 0)
                {
                    if (await ServerDB.CreateBanAsync(player.User.ID, includeIp ? player.Player.EndPoint.Address : null, type, _user.ID, time, reason))
                    {
                        _player.SystemMsg($"{type} applied to {player.User.Name}({player.Char.Pony.Name}) for {time} minutes");
                        if (type == BanType.Ban)
                            player.Player.Disconnect($"Congratulation!{Environment.NewLine}You're Banned!");
                    }
                    else _player.SystemMsg($"Error while appling {type} to {player.User.Name}({player.Char.Pony.Name})");
                }
                else
                {
                    if (await ServerDB.DeleteBanAsync(player.User.ID, null, type))
                        _player.SystemMsg($"{type} removed from {player.User.Name}({player.Char.Pony.Name})!");
                    else _player.SystemMsg($"Error while removing {type} from {player.User.Name}({player.Char.Pony.Name})");
                }
            }
        }
        #region RPC Handlers
        [Rpc(15, false)]
        private async void RPC_015(NetMessage arg1, PlayerMessageInfo arg2)
        {
            var message = default(ChatMsg);
            message.OnDeserialize(arg1);
            if (message.Text.Length > 1 && message.Text[0] == ':')
            {
                if (DoCommand(new CommandArgs(message.Text.Substring(1))))
                    return;
            }
            if (message.Time >= m_mute_chek)
            {
                m_mute = await ServerDB.SelectBanAsync(_user.ID, _player.EndPoint.Address, BanType.Mute, message.Time);
                m_mute_chek = message.Time.AddSeconds(Constants.MuteCheckDelay);
            }
            if (m_mute.End > message.Time)
            {
                _player.MuteMsg(m_mute);
                return;
            }
            if (message.Icon != 0 && _user.Access < AccessLevel.Moderator) message.Icon = 0;
            if (message.Text.Sum(x => char.IsUpper(x) ? 1 : 0) > message.Text.Length / 4 + 4 || message.Time < _time)
                _player.SystemMsg(Constants.ChatWarning);
            else
            {
                message.CharID = _user.Char;
                message.PlayerID = _player.Id;
                message.Name = m_name ?? _user.Name;
                _time = message.Time.AddMilliseconds(SpamDelay);
                switch (message.Type)
                {
                    case ChatType.Global:
                        _player.Server.AllPlayersRpc(15, message);
                        ServerLogger.LogChat(_user.Name, message.Name, message.Type, message.Text);
                        break;
                    case ChatType.Herd:
                    //break;
                    case ChatType.Party:
                        _player.SystemMsg($"Chat type {message.Type} not yet implemented!");
                        break;
                    case ChatType.Whisper:
                        if (_lastWhisper != null)
                        {
                            var player = _server.FindPlayers(_lastWhisper).FirstOrDefault();
                            if (player != null)
                                _player.Whisper(player.Player, message);
                            else
                                _lastWhisper = null;
                        }
                        break;
                    default:
                        _player.SystemMsg($"Chat type {message.Type} not allowed!");
                        break;
                }
            }
        }

        [Rpc(16, false)]
        private void RPC_016(NetMessage arg1, PlayerMessageInfo arg2)
        {
            var target = arg1.ReadString();
            var text = arg1.ReadString();
            ChatIcon icon = (ChatIcon)arg1.ReadByte();
            if (target == _user.Name || target == m_name)
                return;
            if (icon != ChatIcon.None && _user.Access < AccessLevel.Moderator)
                icon = ChatIcon.None;
            _lastWhisper = target.ToLowerInvariant();
            var players = _server.FindPlayers(_lastWhisper);
            var count = players.Count();
            if (count > 1)
                _player.SystemMsg($"Found more then one player, try to be more specific");
            else if (count == 0)
                _player.SystemMsg($"Search result for \"{target}\" returns nothing!");
            else
            {
                var message = default(ChatMsg);
                message.Icon = icon;
                message.Text = text;
                message.Time = DateTime.Now;
                message.CharID = _user.Char;
                message.PlayerID = _player.Id;
                message.Type = ChatType.Whisper;
                message.Name = m_name ?? _user.Name;
                //_player.PlayerRpc(15, ChatType.Whisper, _message.Name ?? _user.Name, message, time, _user.Char, icon, (int)_player.Id);
                players.First().Player.PlayerRpc(15, message);
            }
        }
        [Rpc(20, false)]
        private void RPC_020(NetMessage arg1, PlayerMessageInfo arg2)
        {
            switch (arg1.ReadByte())
            {
                //case 21:
                //    _status.OnDeserialize(arg1);
                //    UpdateStatus();
                //    break;
                case 51:
                    if (_user.Access >= AccessLevel.Moderator)
                    {
                        var name = arg1.ReadString().ToLowerInvariant();
                        _player.PlayerSubRpc(20, 51, _server.FindPlayers(arg1.ReadString().ToLowerInvariant())
                            .Select(x => new FriendStatus()
                            {
                                ID = x?.Char.Id ?? 0,
                                PlayerID = x.Player.Id,
                                CharacterName = x.Char.Pony.Name,
                                CutieMarkID = (short)x.Char.Pony.CutieMark0,
                                LastOnline = DateTime.Now,
                                MapName = _player.Room.RoomId,
                                MapID = _player.Room.Guid.ToString(),
                                Race = x.Char.Pony.Race,
                                UserName = x.User?.Name,
                                Status = OnlineStatus.Online
                            }));
                    }
                    else _player.SystemMsg("Bad pony!");
                    break;
            }
        }
        [Rpc(49, false)]
        private void RPC_049(NetMessage arg1, PlayerMessageInfo arg2)
        {
            if (_user.Access >= AccessLevel.Moderator)
            {
                var subRpc = arg1.ReadByte();
                switch (subRpc)
                {
                    case 200:
                        BanPlayer(arg1.ReadString(), false, arg1.ReadInt32(), BanType.Ban, null);
                        break;
                    default:
                        ServerLogger.LogWarn($"Unhandled: Rpc 49; SubRpc: {subRpc}");
                        break;
                }
            }
            else _player.SystemMsg("Bad pony!");
        }
        [Rpc(150, false)]
        private void RPC_150(NetMessage arg1, PlayerMessageInfo arg2)
        {
            switch (arg1.ReadByte())
            {
                case 10:
                    Guid guid = arg1.ReadGuid(); Room room;
                    if (_server.Server.TryGetRoom(guid, out room))
                        _player.ChangeRoom(room);
                    else _player.SystemMsg($"Couldn't find room {guid.ToString().Normalize(8)}");
                    break;
                case 11:
                    Room[] rooms;
                    if (_server.Server.TryGetRooms(_player.Room.RoomId, out rooms))
                        _player.PlayerSubRpc(150, 11, new SER_RoomInfo(rooms));
                    break;
                default:
                    break;
            }
        }
        [Rpc(201, false)]
        private void RPC_201(NetMessage arg1, PlayerMessageInfo arg2)
        {
            if (_user.Access >= AccessLevel.Moderator)
            {
                var message = arg1.ReadString();
                var duration = arg1.ReadSingle();
                _player.AnnounceAll(message, duration);
            }
            else _player.SystemMsg("Bad pony!");
        }
        [Rpc(204, false)]
        private void RPC_204(NetMessage arg1, PlayerMessageInfo arg2)
        {
            if (_user.Access >= AccessLevel.Moderator)
                BanPlayer(arg1.ReadString(), false, arg1.ReadInt32(), BanType.Mute, null);
            else _player.SystemMsg("Bad pony!");
        }
        #endregion
        #region Events Handlers
        private async void Player_NetUserDataChanged(Player obj)
        {
            if (_user.Char != 0 && (_user.Char != (_char?.Id ?? -1)))
            {
                m_name = null;
                _char = await ServerDB.SelectCharacterAsync(_user.Char);
                if (_char == null)
                    ServerLogger.LogServer(_server, $"{obj.Id} couldn't load character {_user.Char}");
                else
                    m_name = _char.Pony.Name;
            }
        }
        private void Player_FinishedSwitchingRooms(Room obj)
        {
            _rPlayer = null;
            if (ServersMgr.Contains(obj.Guid))
                _rPlayer = ServersMgr.GetPlayer(obj.Guid, _player.Id);
            //if (!(_rPlayer is MapPlayer)) return;
            //MasterPlayer target; DB_User user;
            //foreach (var item in _save.Friends)
            //{
            //    if (_server.TryGetByUserId(item.Key, out target))
            //    {
            //        if (item.Value.Item1 == 1)
            //        {
            //            _player.UpdateFriend(_status.Fill(target, OnlineStatus.Online));
            //            _player.UpdateFriend(_status.Fill(this, OnlineStatus.Online));
            //        }
            //        else if (item.Value.Item1 == 25)
            //            _player.UpdateFriend(_status.Fill(target, OnlineStatus.Incoming));
            //    }
            //    else
            //    {
            //        if (ServerDB.SelectUser(item.Key, out user))
            //        {
            //            if (item.Value.Item1 == 1)
            //                _player.UpdateFriend(_status.Fill(user, OnlineStatus.Offline));
            //            else if (item.Value.Item1 == 25)
            //                _player.UpdateFriend(_status.Fill(user, OnlineStatus.Incoming));
            //        }
            //    }
            //}
        }
        #endregion
    }
}