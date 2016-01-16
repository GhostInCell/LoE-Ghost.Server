using Ghost.Server.Core.Classes;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class StatsMgr : ObjectComponent, IUpdatable
    {
        protected short _level;
        protected Player _player;
        protected NetworkView _view;
        protected CreatureObject _creature;
        protected Dictionary<Stats, StatHelper> _stats;
        private SER_Stats stats_ser;
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
        public abstract int MeleeSkill
        {
            get;
        }
        public abstract float AttackRate
        {
            get;
        }
        public abstract float MeleeDamage
        {
            get;
        }
        public StatHelper this[Stats stat]
        {
            get
            {
                StatHelper ret; _stats.TryGetValue(stat, out ret); return ret;
            }
        }
        public event Action<CreatureObject, float> OnHealReceived;
        public event Action<CreatureObject, float> OnDamageReceived;
        public StatsMgr(CreatureObject parent) 
            : base(parent)
        {
            _creature = parent;
            _stats = new Dictionary<Stats, StatHelper>();
            stats_ser = new SER_Stats(_stats);
            parent.OnSpawn += StatsMgr_OnSpawn;
            parent.OnDestroy += StatsMgr_OnDestroy;
        }
        public abstract void UpdateStats();
        public abstract void Update(TimeSpan time);
        public void SendStats()
        {
            _view.Rpc(4, 52, RpcMode.AllUnordered, stats_ser);
        }
        public void DoHeal(CreatureObject other, float amount)
        {
            _stats[Stats.Health].IncreaseCurrent(amount);
            OnHealReceived?.Invoke(other, amount);
        }
        public void DoDamage(CreatureObject other, float damage, bool isMagic = false)
        {
            if (_creature.IsDead) return;
            StatHelper hStat = _stats[Stats.Health];
            StatHelper pStat = isMagic ? _stats[Stats.MagicResist] : _stats[Stats.Armor];
            hStat.DecreaseCurrent(damage = CalculateDamage(other.Stats.Level, damage, pStat.Max));
            OnDamageReceived?.Invoke(other, damage);
            _view.Rpc(4, 50, RpcMode.AllOrdered, (byte)Stats.Health, hStat.Current);
            if (hStat.Current == 0f) _creature.Kill(other);
        }
        protected float CalculateDamage(short level, float damage, float protection)
        {
            return damage * (1f - MathHelper.Clamp(0.08f / (_level >= level ? _level : level) * protection + (0.005f * (_level - level)), 0f, 0.75f));
        }
        #region RPC Handlers
        private void RPC_051(NetMessage arg1, NetMessageInfo arg2)
        {
            Stats stat = (Stats)arg1.ReadByte(); StatHelper rStat;
            if (_stats.TryGetValue(stat, out rStat))
            {
                _view.Rpc(4, 51, arg2.Sender, (byte)stat, rStat.Max);
                _view.Rpc(4, 50, arg2.Sender, (byte)stat, rStat.Current);
                return;
            }
            _view.Rpc(4, 51, arg2.Sender, (byte)stat, 0f);
            _view.Rpc(4, 50, arg2.Sender, (byte)stat, 0f);
        }
        private void RPC_053(NetMessage arg1, NetMessageInfo arg2)
        {
            _view.Rpc(4, 53, arg2.Sender, _level);
        }
        private void RPC_056(NetMessage arg1, NetMessageInfo arg2)
        {
            _view.Rpc(4, 52, arg2.Sender, stats_ser);
        }
        private void RPC_058(NetMessage arg1, NetMessageInfo arg2)
        {
            _creature.Despawn();
        }
        #endregion
        #region Events Handlers
        private void StatsMgr_OnSpawn()
        {
            _view = _creature.View;
            _player = _view.Owner;
            _view.SubscribeToRpc(4, 51, RPC_051);
            _view.SubscribeToRpc(4, 53, RPC_053);
            _view.SubscribeToRpc(4, 56, RPC_056);
            _view.SubscribeToRpc(4, 58, RPC_058);
        }
        private void StatsMgr_OnDestroy()
        {
            _view = null;
            _stats = null;
            _player = null;
            _creature = null;
        }
        #endregion
    }
}