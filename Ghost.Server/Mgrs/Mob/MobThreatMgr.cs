using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs.Mob
{
    public class MobThreatMgr
    {
        private readonly object _lock = new object();
        private WO_MOB _mob;
        private Dictionary<WO_Player, int> _threat;
        public WO_Player[] ToAward
        {
            get
            {
                lock (_lock)
                {
                    return _threat.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
                }
            }
        }
        public MobThreatMgr(WO_MOB mob)
        {
            _mob = mob;
            _threat = new Dictionary<WO_Player, int>();
        }
        public void Clear()
        {
            lock (_lock)
                _threat.Clear();
        }
        public void Destroy()
        {
            _mob = null;
            lock (_lock) 
                _threat.Clear();
            _threat = null;
        }
        public void Remove(WO_Player player)
        {
            lock (_lock)
                _threat.Remove(player);
        }
        public bool SelectTarget(out WO_Player player)
        {
            lock (_lock)
            {
                player = null;
                if (_threat.Count > 0)
                {
                    foreach (var item in _threat.ToArray())
                        if (!item.Key.IsSpawned || Vector3.Distance(item.Key.Position, _mob.Position) > Constants.MaxSkillsDistance)
                            _threat.Remove(item.Key);
                }
                foreach (var item in _mob.Manager.GetPlayersInRadius(_mob, Constants.MaxInteractionDistance))
                    if (!_threat.ContainsKey(item))
                        _threat[item] = item.Player.Char.Level;
                return (player = _threat.OrderByDescending(x => x.Value).FirstOrDefault().Key) != null;
            }
        }
        public void AddThreat(WO_Player player, int value)
        {
            lock (_lock)
            {
                if (_threat.ContainsKey(player))
                    _threat[player] += value;
                else
                    _threat[player] = value;
            }
        }
    }
}