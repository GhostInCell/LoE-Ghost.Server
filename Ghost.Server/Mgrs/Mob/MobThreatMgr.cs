using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Mgrs.Mob
{
    public class MobThreatMgr
    {
        private WO_MOB _mob;
        private bool _inCombat;
        private WO_Player _attacker;
        private Dictionary<WO_Player, int> _threat;
        public WO_Player Attacker
        {
            get { return _attacker; }
        }
        public MobThreatMgr(WO_MOB mob)
        {
            _mob = mob;
            _threat = new Dictionary<WO_Player, int>();
        }
        public void Clear()
        {
            _threat.Clear();
        }
        public void Destroy()
        {
            _mob = null;
            _threat.Clear();
            _threat = null;
        }
        public void Remove(WO_Player player)
        {
            lock (_threat)
            {
                if (_attacker == player)
                {
                    _attacker = null;
                    if (_threat.Count > 0)
                        _attacker = _threat.OrderBy(x => x.Value).Last().Key;
                }
                _threat.Remove(player);
            }
        }
        public bool SelectTarget(out WO_Player player)
        {
            player = null;
            lock (_threat)
            {
                if (_threat.Count > 0)
                {
                    foreach (var item in _threat.ToArray())
                        if (!item.Key.IsSpawned || Vector3.Distance(item.Key.Position, _mob.Position) > Constants.MaxSkillsDistance)
                            _threat.Remove(item.Key);
                }
                foreach (var item in _mob.Manager.GetPlayersInRadius(_mob, Constants.MaxInteractionDistance))
                    if (!_threat.ContainsKey(item))
                        _threat[item] = item.Player.Char.Level;
            }
            if (_inCombat = _threat.Count > 0)
                player = _threat.OrderBy(x => x.Value).Last().Key;
            return _inCombat;
        }
        public void AddThreat(WO_Player player, int value)
        {
            lock (_threat)
            {
                if (_threat.Count == 0 || _attacker == null)
                    _attacker = player;
                if (_threat.ContainsKey(player))
                    _threat[player] += value;
                else
                    _threat[player] = value;
            }

        }
    }
}