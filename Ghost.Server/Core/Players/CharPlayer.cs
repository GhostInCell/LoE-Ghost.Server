using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Servers;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces;
using PNetR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Ghost.Server.Core.Players
{
    public class CharPlayer : IPlayer
    {
        private Player _player;

        private UserData _user;

        private CharServer _server;

        private List<Character> m_data;

        public string Status
        {
            get
            {
                return $"Player[{_player.Id}|{_user.ID}:{_user.Name}]: at server {_server.Name}[{_server.ID}] loaded {m_data?.Count ?? 0} characters";
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
            get { return m_data; }
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
            m_data.Clear();
            m_data = null;
            _user = null;
            _player = null;
            _server = null;
        }

        public void Disconnect(string message)
        {
            _server.Room.Server.Rpc(255, _player.Id, message);
        }

        public async Task<bool> DeleteCharacter(int index)
        {
            if (index >= m_data.Count || index < 0) return false;
            if (!await ServerDB.DeleteCharacterAsync(m_data[index].Id))
                return false;
            m_data.RemoveAt(index);
            return true;
        }

        public async Task<bool> UpdateCharacter(int index, PonyData pony)
        {
            if (m_data == null || index >= m_data.Count) return false;
            CharsMgr.ValidatePonyData(pony);
            if (index == -1)
            {
                if (m_data.Count >= CharsMgr.MaxChars) return false;
                var character = await ServerDB.CreateCharacterAsync(_user.ID, pony);
                if (character == null)
                    return false;
                m_data.Add(character);
            }
            else
            {

                var character = m_data[index];
                character.Pony = pony;
                if (!await ServerDB.UpdatePonyAsync(character))
                    return false;
            }
            return true;
        }

        #region Events Handlers
        private async void Player_NetUserDataChanged(Player obj)
        {
            if (m_data != null) return;
            m_data = await ServerDB.SelectAllUserCharactersAsync(_user.ID);
            if (m_data != null)
            {
                foreach (var item in m_data)
                    CharsMgr.ValidatePonyData(item.Pony);
                this.SendPonies();
            }
            else
                _player.Error($"Error while retrieving ponies from data base");
        }
        #endregion
    }
}