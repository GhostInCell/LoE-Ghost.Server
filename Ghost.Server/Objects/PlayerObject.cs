using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Players;
using Ghost.Server.Mgrs;
using Ghost.Server.Objects.Managers;
using Ghost.Server.Utilities;
using PNetR;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Ghost.Server.Objects
{
    public class SaveManager : BaseManager<PlayerObject>
    {
        public static int SaveTime = Configs.Get<int>(Configs.Game_SaveChar);

        public SaveManager()
            : base()
        {
            Interval = TimeSpan.FromSeconds(SaveTime);
        }

        protected override void OnUpdate(TimeSpan time)
        {
            base.OnUpdate(time);
            m_owner.SaveCharacter();
        }
    }

    public class PlayerObject : CreatureObject
    {
        private UserData m_user;
        private Character m_char;
        private MapPlayer m_player;
        private SaveManager m_save;

        public UserData User
        {
            get
            {
                return m_user;
            }
        }

        public override Vector3 SpawnPosition
        {
            get
            {
                return m_player.Data.Position;
            }
        }

        public override Vector3 SpawnRotation
        {
            get
            {
                return m_player.Data.Rotation.ToRadians();
            }
        }

        public PlayerObject(MapPlayer player)
            : base()
        {
            m_player = player;
            m_user = player.User;
            m_char = player.Char;
            m_save = AddManager<SaveManager>();
        }

        public Task<bool> SaveCharacter()
        {
            m_char.Data.Position = m_position;
            m_char.Data.Rotation = m_rotation;
            return CharsMgr.SaveCharacterAsync(m_char);
        }

        public Task<bool> PrepareForMapSwitch()
        {
            m_save.Enabled = false;
            return SaveCharacter();
        }
        #region Overridden Methods
        protected async override void OnDispose()
        {
            base.OnDispose();
            if (m_save.Enabled)
                await SaveCharacter();
        }
        protected override NetworkView CreateView()
        {
            return m_player.Server.Room.Instantiate("PlayerBase", m_position, m_rotation.ToDegrees(), m_player.Player);
        }
        #endregion
    }
}