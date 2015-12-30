using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class StatsMgr : IUpdatable
    {
        protected short _level;
        protected CreatureObject _creature;
        protected Dictionary<Stats, StatHelper> _stats;
        public StatsMgr(CreatureObject creature)
        {
            _creature = creature;
            _creature.OnSpawn += StatsMgr_OnSpawn;
            _stats = new Dictionary<Stats, StatHelper>();
        }
        public float Armor
        {
            get { return _stats[Stats.Armor].Max; }
        }
        public float Speed
        {
            get { return _stats[Stats.Speed].Max; }
        }
        public short Level
        {
            get { return _level; }
        }
        public float Attack
        {
            get { return _stats[Stats.Attack].Max; }
        }
        public float Energy
        {
            get { return _stats[Stats.Energy].Current; }
        }
        public float Health
        {
            get { return _stats[Stats.Health].Current; }
        }
        public void DoHeal(CreatureObject other, float amount)
        {
            _stats[Stats.Health].UpdateCurrent(amount);
        }
        public abstract void Destroy();
        public abstract void UpdateStats();
        public abstract void Update(TimeSpan time);
        public abstract void DoDamage(CreatureObject other, float damage, bool isMagic = false);
        protected float CalculateDamage(short level, float damage, float protection)
        {
            return damage * (1f - MathHelper.Clamp(0.09f / (_level >= level ? _level : level) * protection + (0.005f * (_level - level)), 0f, 0.75f));
        }
        #region RPC Handlers
        private void RPC_053(NetMessage arg1, NetMessageInfo arg2)
        {
            _creature.View.Rpc(4, 53, arg2.Sender, _level);
        }
        #endregion
        #region Events Handlers
        private void StatsMgr_OnSpawn()
        {
            _creature.View.SubscribeToRpc(4, 53, RPC_053);
        }
        #endregion
    }
}