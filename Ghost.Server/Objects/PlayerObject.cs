using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using PNetR;
using System.Numerics;

namespace Ghost.Server.Objects
{
    public class PlayerObject : CreatureObject
    {
        private MapPlayer m_player;

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
        }
        #region Overridden Methods
        protected override NetworkView CreateView()
        {
            return m_player.Server.Room.Instantiate("PlayerBase", m_position, m_rotation.ToDegrees(), m_player.Player);
        }
        #endregion
    }
}