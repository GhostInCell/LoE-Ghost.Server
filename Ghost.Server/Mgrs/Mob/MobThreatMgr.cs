using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs.Mob
{
    public class MobThreatMgr : ThreatMgr
    {
        public WO_Player[] ToAward
        {
            get
            {
                return m_threat.OrderByDescending(x => x.Value).Select(x => x.Key as WO_Player).ToArray();
            }
        }
        public MobThreatMgr(WO_MOB parent)
            : base(parent)
        { }
        public override bool SelectTarget(out CreatureObject target)
        {
            target = null;
            if (m_threat.Count > 0)
            {
                foreach (var item in m_threat)
                {
                    if (!item.Key.IsSpawned || item.Key.IsDead || Vector3.DistanceSquared(item.Key.Position, m_creature.Position) > Constants.MaxSpellsDistanceSquared)
                    {
                        if (item.Key.IsSpawned)
                            m_creature.View.SetCombat(item.Key.Owner, false);
                        m_threat.TryRemove(item.Key, out _);
                    }
                }
            }
            foreach (var item in m_creature.Manager.GetPlayersInRadius(m_creature, Constants.MaxInteractionDistance))
            {
                if (!m_threat.ContainsKey(item))
                {
                    m_creature.View.SetCombat(item.Owner, true);
                    m_threat[item] = item.Player.Char.Level;
                }
            }
            return (target = m_threat.OrderByDescending(x => x.Value).FirstOrDefault().Key) != null;
        }
    }
}