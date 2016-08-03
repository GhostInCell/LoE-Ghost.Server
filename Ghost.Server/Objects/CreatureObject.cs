using Ghost.Server.Objects.Managers;
using Ghost.Server.Utilities;
using System.Numerics;

namespace Ghost.Server.Objects
{
    public abstract class CreatureObject : NetworkObject
    {
        protected const int DeadFlag = 0x00000001;

        public bool IsDead
        {
            get
            {
                return (m_state & DeadFlag) != 0;
            }
        }

        public short Level
        {
            get; set;
        }

        public StatsManager Stats
        {
            get; private set;
        }

        public abstract Vector3 SpawnPosition
        {
            get;
        }

        public abstract Vector3 SpawnRotation
        {
            get;
        }

        public CreatureObject()
            : base()
        {
            Stats = AddManager<StatsManager>();
        }

        public bool InCombatRange(CreatureObject other)
        {
            return Vector3.DistanceSquared(m_position, other.m_position) <= (Constants.MaxCombatDistanceSquared + Constants.EpsilonX1);
        }
    }
}