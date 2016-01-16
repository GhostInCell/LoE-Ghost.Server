using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using System;

namespace Ghost.Server.Mgrs.Mob
{
    public class MobStatsMgr : StatsMgr
    {
        private const int interval = 200;
        private int _update;
        private WO_MOB _mob;
        private float _dmg_min;
        private float _dmg_max;
        private int _meleeSkill;
        private float _attackRate;
        public override int MeleeSkill
        {
            get
            {
                return _meleeSkill;
            }
        }
        public override float AttackRate
        {
            get
            {
                return _attackRate;
            }
        }
        public override float MeleeDamage
        {
            get
            {
                return _stats[Stats.Attack].Max + Constants.RND.Next((int)_dmg_min, (int)_dmg_max);
            }
        }
        public MobStatsMgr(WO_MOB parent)
            : base(parent)
        {
            _mob = parent;
            _meleeSkill = _mob.Creature.SpellID;
            _attackRate = _mob.Creature.Attack_Rate;
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
        public override void Update(TimeSpan time)
        {
            if ((_update -= time.Milliseconds) > 0) return;
            _update = interval;
            var hp = _stats[Stats.Health];
            var ep = _stats[Stats.Energy];
            if (hp.Max != hp.Current)
            {
                hp.IncreaseCurrent(_stats[Stats.HealthRegen].Max * (interval / 1000f));
                _view.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, hp.Current);
            }
            if (ep.Max != ep.Current)
                ep.IncreaseCurrent(_stats[Stats.EnergyRegen].Max * (interval / 1000f));
        }
        #region Events Handlers
        private void MobStatsMgr_OnDestroy()
        {
            _mob = null;
            _view = null;
            _stats = null;
            _creature = null;
        }
        #endregion
    }
}