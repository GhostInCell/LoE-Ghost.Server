using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
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
                lock (_lock)
                {
                    return _threat.OrderByDescending(x => x.Value).Select(x => x.Key as WO_Player).ToArray();
                }
            }
        }
        public MobThreatMgr(WO_MOB parent)
            : base(parent)
        { }
        public override bool SelectTarget(out CreatureObject target)
        {
            lock (_lock)
            {
                target = null;
                if (_threat.Count > 0)
                {
                    foreach (var item in _threat.ToArray())
                        if (!item.Key.IsSpawned || item.Key.IsDead || Vector3.Distance(item.Key.Position, _creature.Position) > Constants.MaxSkillsDistance)
                        {
                            if (item.Key.IsSpawned)
                                _creature.View.SetCombat(item.Key.Owner, false);
                            _threat.Remove(item.Key);
                        }
                }
                foreach (var item in _creature.Manager.GetPlayersInRadius(_creature, Constants.MaxInteractionDistance))
                    if (!_threat.ContainsKey(item))
                    {
                        _creature.View.SetCombat(item.Owner, true);
                        _threat[item] = item.Player.Char.Level;
                    }
                return (target = _threat.OrderByDescending(x => x.Value).FirstOrDefault().Key) != null;
            }
        }
    }
}