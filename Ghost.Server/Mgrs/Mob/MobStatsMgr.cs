using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Mgrs.Mob
{
    public class MobStatsMgr : StatsMgr
    {
        private const int interval = 200;
        private int _update;
        private WO_MOB _mob;
        private float _dmg_min;
        private float _dmg_max;
        public int RandomDamage
        {
            get { return (int)_stats[Stats.Attack].Max + Constants.RND.Next((int)_dmg_min, (int)_dmg_max); }
        }
        public MobStatsMgr(WO_MOB mob)
            : base(mob)
        {
            _mob = mob;
        }
        public override void UpdateStats()
        {
            _stats.Clear();
            _level = (short)Constants.RND.Next(_mob.Data.Data01, _mob.Data.Data02);
            _dmg_min = _level * _mob.Creature.Base_Dmg_Min;
            _dmg_max = _level * _mob.Creature.Base_Dmg_Max;
            _stats[Stats.Speed] = new StatHelper(_mob.Creature.Speed);
            _stats[Stats.Armor] = new StatHelper(_level * _mob.Creature.Base_Armor);
            _stats[Stats.Dodge] = new StatHelper(_level * _mob.Creature.Base_Dodge);
            _stats[Stats.Attack] = new StatHelper(_level * _mob.Creature.Base_Power);
            _stats[Stats.Energy] = new StatHelper(_level * _mob.Creature.Base_Energy);
            _stats[Stats.Health] = new StatHelper(_level * _mob.Creature.Base_Health);
            _stats[Stats.EnergyRegen] = new StatHelper(_level * _mob.Creature.Base_EP_Reg);
            _stats[Stats.HealthRegen] = new StatHelper(_level * _mob.Creature.Base_HP_Reg);
            _stats[Stats.MagicResist] = new StatHelper(_level * _mob.Creature.Base_Resists);
        }
        public override void Destroy()
        {
            _stats.Clear();
            _mob = null;
            _stats = null;
            _creature = null;
        }
        public override void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) > 0) return;
            _update = interval;
            var hp = _stats[Stats.Health];
            var ep = _stats[Stats.Energy];
            if (hp.Max != hp.Current)
            {
                hp.UpdateCurrent(_stats[Stats.HealthRegen].Max * (interval / 1000f));
                _mob.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Health, hp.Max);
                _mob.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, hp.Current);
            }
            if (ep.Max != ep.Current)
                ep.UpdateCurrent(_stats[Stats.EnergyRegen].Max * (interval / 1000f));
        }
        public override void DoDamage(CreatureObject other, float damage, bool isMagic = false)
        {
            int lvlDif = other.Stats.Level - _level;
            StatHelper hStat = _stats[Stats.Health];
            StatHelper pStat = isMagic ? _stats[Stats.MagicResist] : _stats[Stats.Armor];
            hStat.UpdateCurrent(-damage * (1f - MathHelper.Clamp((0.095f / (_level + lvlDif)) * pStat.Max, 0f, 0.75f)));
            if (other.IsPlayer) _mob.ThreatMgr.AddThreat(other as WO_Player, (int)damage);
            _mob.View.Rpc(4, 51, RpcMode.AllOrdered, (byte)Stats.Health, hStat.Max);
            _mob.View.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, hStat.Current);
            if (hStat.Current == 0f) _mob.Kill();
        }
    }
}