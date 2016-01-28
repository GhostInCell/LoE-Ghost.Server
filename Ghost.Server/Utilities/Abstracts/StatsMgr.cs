using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Utilities.Interfaces;
using PNet;
using PNetR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Utilities.Abstracts
{
    public abstract class StatsMgr : ObjectComponent, IUpdatable
    {
        private readonly object _modifersLock = new object();
        protected short _level;
        protected Player _player;
        protected NetworkView _view;
        protected CreatureObject _creature;
        protected Dictionary<Stats, StatHelper> _stats;
        private SER_Stats stats_ser;
        private List<TimedModifer> _modifers;
        private Dictionary<uint, List<Tuple<Stats, float, bool>>> _auras;
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
            _modifers = new List<TimedModifer>();
            _stats = new Dictionary<Stats, StatHelper>();
            _auras = new Dictionary<uint, List<Tuple<Stats, float, bool>>>();
            stats_ser = new SER_Stats(_stats);
            parent.OnSpawn += StatsMgr_OnSpawn;
            parent.OnDespawn += StatsMgr_OnDespawn;
            parent.OnDestroy += StatsMgr_OnDestroy;
        }
        public abstract void UpdateStats();
        public abstract void Update(TimeSpan time);
        public void SendStats()
        {
            _view.Rpc(4, 52, RpcMode.AllUnordered, stats_ser);
        }
        public void RemoveAuraEffects(uint guid)
        {
            List<Tuple<Stats, float, bool>> aura;
            lock (_modifersLock)
            {
                if (_auras.TryGetValue(guid, out aura))
                {
                    ServerLogger.LogInfo($"Aura[{guid:X8}] removed from {_creature.Guid:X8}");
                    foreach (var item in aura)
                        if (item.Item3)
                            _stats[item.Item1].RemoveMultiplier(item.Item2);
                        else
                            _stats[item.Item1].RemoveModifer(item.Item2);
                    aura.Clear();
                    _auras.Remove(guid);
                }
            }
        }
        public void RemoveModifier(TimedModifer modifer)
        {
            StatHelper hStat; Stats stat = modifer.Stat;
            if (_stats.TryGetValue(stat, out hStat))
            {
                lock (_modifersLock)
                {
                    if (modifer.IsMultiplier)
                        hStat.RemoveMultiplier(modifer.Value);
                    else
                        hStat.RemoveModifer(modifer.Value);
                    _view.Rpc(4, 51, RpcMode.AllUnordered, (byte)stat, hStat.Max);
                    _view.Rpc(4, 50, RpcMode.AllUnordered, (byte)stat, hStat.Current);
                    _modifers.Remove(modifer);
                }
            }
        }
        public void DoHeal(CreatureObject other, float amount)
        {
            _stats[Stats.Health].IncreaseCurrent(amount);
            OnHealReceived?.Invoke(other, amount);
        }
        public void AddModifier(Stats stat, float value, float time, bool isMul)
        {
            StatHelper mStat;
            if (_stats.TryGetValue(stat, out mStat))
            {
                lock (_modifersLock)
                {
                    if (isMul)
                        mStat.AddMultiplier(value);
                    else
                        mStat.AddModifer(value);
                    _view.Rpc(4, 51, RpcMode.AllUnordered, (byte)stat, mStat.Max);
                    _view.Rpc(4, 50, RpcMode.AllUnordered, (byte)stat, mStat.Current);
                    _modifers.Add(new TimedModifer(this, stat, value, isMul, time));
                }
            }
        }
        public void AddAuraEffect(uint guid, Stats stat, float value, bool isMul)
        {
            ServerLogger.LogInfo($"Creature[{_creature.Guid}] Aura[{guid:X8}] added stat {(isMul ? "multipler" : "modifer")} {value} for {stat} ");
            StatHelper mStat; List<Tuple<Stats, float, bool>> aura;
            if (_stats.TryGetValue(stat, out mStat))
            {
                lock (_modifersLock)
                {
                    if (!_auras.TryGetValue(guid, out aura))
                        _auras[guid] = aura = new List<Tuple<Stats, float, bool>>();
                    aura.Add(new Tuple<Stats, float, bool>(stat, value, isMul));
                    if (isMul)
                        mStat.AddMultiplier(value);
                    else
                        mStat.AddModifer(value);
                    _view.Rpc(4, 51, RpcMode.AllUnordered, (byte)stat, mStat.Max);
                    _view.Rpc(4, 50, RpcMode.AllUnordered, (byte)stat, mStat.Current);
                }
            }
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
        private void StatsMgr_OnDespawn()
        {
            foreach (var item in _modifers.ToArray())
            {
                item.Destroy();
                RemoveModifier(item);
            }
            foreach (var item in _auras.Keys.ToArray())
                RemoveAuraEffects(item);
        }
        private void StatsMgr_OnDestroy()
        {
            foreach (var item in _modifers.ToArray())
            {
                item.Destroy();
                RemoveModifier(item);
            }
            foreach (var item in _auras.Keys.ToArray())
                RemoveAuraEffects(item);
            _view = null;
            _stats = null;
            _player = null;
            _creature = null;
            _modifers = null;
        }
        #endregion
    }
}