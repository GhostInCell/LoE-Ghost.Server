using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Servers;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Player;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNetR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Core.Players
{
    public class MapPlayer : IPlayer
    {
        private WO_Pet _pet;
        private WO_NPC _shop;
        private WO_NPC _dialog;
        private Player _player;
        private UserData _user;
        private Character _char;
        private TradeMgr _trade;
        private ItemsMgr _items;
        private SkillsMgr _skills;
        private MapServer _server;
        private WO_Player _object;
        private AutoSaveChar _save;
        private Dictionary<int, WO_NPC> _clones;
        public WO_Pet Pet
        {
            get { return _pet; }
        }
        public WO_NPC Shop
        {
            get { return _shop; }
            set { _shop = value; }
        }
        public WO_NPC Dialog
        {
            get { return _dialog; }
            set { _dialog = value; }
        }
        public string Status
        {
            get
            {
                return $"Player[{_player.Id}|{_user.ID}:{_user.Name}]: map {_server.Map.Name} id {_server.ID}{Environment.NewLine}Character[{_char?.ID}|{_char?.Level}|{_char.Pony?.Name}]: at pos <{_object?.Position.X:0.00}, {_object?.Position.Y:0.00}, {_object?.Position.Z:0.00}> rot {_object?.Rotation.Y:0.00} stats {(_object?.Stats as PlayerStatsMgr)?.Status}";
            }
        }
        public Player Player
        {
            get
            {
                return _player;
            }
        }
        public CharData Data
        {
            get { return _char.Data; }
        }
        public UserData User
        {
            get
            {
                return _user;
            }
        }
        IServer IPlayer.Server
        {
            get
            {
                return _server;
            }
        }
        public TradeMgr Trade
        {
            get
            {
                return _trade;
            }
        }
        public Character Char
        {
            get { return _char; }
        }
        public ItemsMgr Items
        {
            get
            {
                return _items;
            }
        }
        public MapServer Server
        {
            get
            {
                return _server;
            }
        }
        public NetworkView View
        {
            get { return _object.View; }
        }
        public WO_Player Object
        {
            get
            {
                return _object;
            }
        }
        public DateTime LastMsg
        {
            get; set;
        }
        public Dictionary<int, WO_NPC> Clones
        {
            get { return _clones; }
        }
        public short LastDialogChoice
        {
            get; set;
        }
        public PlayerStatsMgr Stats
        {
            get
            {
                return _object?.Stats as PlayerStatsMgr;
            }
        }
        public SkillsMgr Skills
        {
            get
            {
                return _skills;
            }
        }
        public MapPlayer(Player player, MapServer server)
        {
            _server = server;
            _player = player;
            _user = player.TnUser<UserData>();
            _clones = new Dictionary<int, WO_NPC>();
            _player.NetUserDataChanged += Player_NetUserDataChanged;
        }
        public void Destroy()
        {
            _player.NetUserDataChanged -= Player_NetUserDataChanged;
            _save.Destroy();
            _object.Destroy();
            _pet?.Destroy();
            CharsMgr.SaveCharacter(_char);
            if (_dialog != null)
                _dialog.Dialog.OnDialogEnd(this);
            if (_shop != null)
                _shop.Movement.Unlock();
            foreach (var item in _clones.Values.ToArray())
                item.Destroy();
            _pet = null;
            _save = null;
            _shop = null;
            _user = null;
            _char = null;
            _trade = null;
            _items = null;
            _clones = null;
            _dialog = null;
            _server = null;
            _player = null;
            _object = null;
            _skills = null;
        }
        public void DialogEnd()
        {
            _dialog.Dialog.OnDialogEnd(this);
            _player.Rpc(13);
            _dialog = null;
        }
        public void DialogBegin()
        {
            _player.Rpc(11);
            _dialog.Dialog.OnDialogStart(this);
        }
        public void SetPet(int id = -1)
        {
            if (id > 0)
            {
                if (_pet != null) _pet.Destroy();
                _pet = new WO_Pet(id, _object);
                _char.Data.Variables[Constants.PlayerVarPet] = id;
            }
            else if (_pet != null)
            {
                _char.Data.Variables.Remove(Constants.PlayerVarPet);
                _pet.Destroy();
                _pet = null;
            }
            else
            {
                id = _char.Data.GetVariable(Constants.PlayerVarPet);
                if (id > 0) _pet = new WO_Pet(id, _object);
            }
        }
        public void Disconnect(string message)
        {
            _server.Room.Server.Rpc(255, _player.Id, message);
        }
        public void DialogSetOptions(string[] options)
        {
            _player.Rpc(12, (object)options);
        }
        public void DialogSetMessage(WO_NPC talker, int message)
        {
            var entry = DataMgr.SelectMessage(message);
            _player.Rpc(17, entry.Item2.GetMessage(this), talker.NPC.Pony.Name, talker.SGuid, entry.Item1);             
        }
        public void DialogSetMessage(string message, ushort emotion)
        {
            _player.Rpc(17, message, Dialog.NPC.Pony.Name, Dialog.SGuid, emotion);
        }
        public void DialogSetMessage(string name, string message, ushort emotion)
        {
            _player.Rpc(17, message, name, Dialog.SGuid, emotion);
        }
        public void DialogSetMessage(WO_NPC talker, string message, ushort emotion)
        {
            _player.Rpc(17, message, talker.NPC.Pony.Name, talker.SGuid, emotion);
        }
        #region Events Handlers
        private void Player_NetUserDataChanged(Player obj)
        {
            if (_char != null) return;
            if (!CharsMgr.SelectCharacter(_user.Char, out _char))
                _player.Error($"Error while retrieving pony");
            else
            {
                _player.SetBounds();
                _object = new WO_Player(this);
                _save = new AutoSaveChar(this);
                _items = _object.GetComponent<ItemsMgr>();
                _trade = _object.GetComponent<TradeMgr>();
                _skills = _object.GetComponent<SkillsMgr>();
                SetPet();
                _user.Map = _server.Map.ID;
                _char.Map = _user.Map;
                _player.SynchNetData();
                CharsMgr.SaveCharacter(_char);
            }
        }
        #endregion
    }
}