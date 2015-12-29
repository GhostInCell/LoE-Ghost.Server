using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Servers;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNetR;
using System.Collections.Generic;
namespace Ghost.Server.Core.Players
{
    public class CharPlayer : IPlayer
    {
        private Player _player;
        private UserData _user;
        private CharServer _server;
        private List<Character> _data;
        public string Status
        {
            get
            {
                return $"Player[{_player.Id}|{_user.ID}:{_user.Name}]: at server {_server.Name}[{_server.ID}] loaded {_data?.Count ?? 0} characters";
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
        IServer IPlayer.Server
        {
            get
            {
                return _server;
            }
        }
        public CharServer Server
        {
            get
            {
                return _server;
            }
        }
        public List<Character> Data
        {
            get { return _data; }
        }
        public CharPlayer(Player player, CharServer server)
        {
            _server = server;
            _player = player;
            _user = player.TnUser<UserData>();
            _player.NetUserDataChanged += Player_NetUserDataChanged;
        }
        public void Destroy()
        {
            _player.NetUserDataChanged -= Player_NetUserDataChanged;
            _data.RemoveAll(x => x.ID == _user.Char);
            CharsMgr.RemoveCharacters(_data);
            _data.Clear();
            _data = null;
            _user = null;
            _player = null;
            _server = null;
        }
        public bool DeleteCharacter(int index)
        {
            if (index >= _data.Count || index < 0) return false;
            if (!CharsMgr.DeleteCharacter(_data[index].ID))
                return false;
            _data.RemoveAt(index);
            return true;
        }
        public bool UpdateCharacter(int index, PonyData pony)
        {
            if (_data == null || index >= _data.Count) return false;
            Character character;
            if (index == -1)
            {
                if (_data.Count >= CharsMgr.MaxChars) return false;
                if (!CharsMgr.CreateCharacter(_user.ID, pony, out character))
                    return false;
                _data.Add(character);
            }
            else
            {

                character = _data[index];
                character.Pony = pony;
                if (!ServerDB.UpdatePony(character))
                    return false;
            }
            return true;
        }
        #region Events Handlers
        private void Player_NetUserDataChanged(Player obj)
        {
            if (_data != null) return;
            if (CharsMgr.SelectAllUserCharacters(_user.ID, out _data))
                this.SendPonies();
            else
                _player.Error($"Error while retrieving ponies from data base");
        }
        #endregion
    }
}