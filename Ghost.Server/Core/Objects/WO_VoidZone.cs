using Ghost.Server.Core.Structs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_VoidZone : WorldObject
    {
        private int _ticks;
        private int _period;
        private int _update;
        private bool _hasAura;
        private WO_Player _onwer;
        private Vector3 _position;
        private readonly DB_Spell _spell;
        private readonly DB_SpellEffect _main;
        private List<CreatureObject> _targets;
        private List<CreatureObject> _addedTargets;
        private List<CreatureObject> _removedTargets;
        public override byte TypeID
        {
            get
            {
                return Constants.TypeIDVoidZone;
            }
        }
        public override ushort SGuid
        {
            get
            {
                return (ushort)(_guid & 0xFFFF);
            }
        }
        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override Vector3 Rotation
        {
            get
            {
                return Vector3.Zero;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public WO_VoidZone(WO_Player onwer, DB_Spell spell, DB_SpellEffect main)
            : base(onwer.Manager.GetNewGuid() | Constants.DRObject, onwer.Manager)
        {
            _main = main;
            _spell = spell;
            _onwer = onwer;
            _position = onwer.Position;
            _ticks = (int)main.AttackModifer;
            OnUpdate += WO_VoidZone_OnUpdate;
            OnDestroy += WO_VoidZone_OnDestroy;
            _targets = new List<CreatureObject>();
            _addedTargets = new List<CreatureObject>();
            _removedTargets = new List<CreatureObject>();
            _update = _period = (int)(main.LevelModifer * 1000) / _ticks;
            _hasAura = spell.Effects.Values.Any(x => x.Type == SpellEffectType.AuraModifier);   
            Spawn();
        }
        private void UpdateAreaEffects()
        {
            float effect; bool magick;
            foreach (var item in _spell.Effects.Values)
            {
                switch (item.Type)
                {
                    case SpellEffectType.AreaPeriodicHeal:
                        effect = item.BaseConst + (item.LevelModifer * _onwer.Stats.Level) + (item.AttackModifer * _onwer.Stats.Attack);
                        if ((item.Target & SpellTarget.Player) != 0)
                            foreach (var entry in _manager.GetPlayersInRadius(_position, _main.BaseConst).ToArray())
                                entry.Stats?.DoHeal(_onwer, effect);
                        break;
                    case SpellEffectType.AreaPeriodicDamage:
                    case SpellEffectType.AreaPeriodicMagickDamage:
                        magick = item.Type == SpellEffectType.AreaPeriodicMagickDamage;
                        effect = item.BaseConst + (item.LevelModifer * _onwer.Stats.Level) + (item.AttackModifer * _onwer.Stats.Attack);
                        if ((item.Target & SpellTarget.Creature) != 0)
                            foreach (var entry in _manager.GetMobsInRadius(_position, _main.BaseConst).ToArray())
                                entry.Stats?.DoDamage(_onwer, effect, magick);
                        break;
                }
            }
        }
        private void UpdateAuraEffects()
        {
            _addedTargets.Clear();
            _removedTargets.Clear();
            foreach (var item in _targets)
                if ((item.IsDead || !item.IsSpawned) || Vector3.Distance(_position, item.Position) > _main.BaseConst)
                {
                    item.Stats.RemoveAuraEffects(_guid);
                    _removedTargets.Add(item);
                }
            _manager.GetCreaturesInRadius(this, _main.BaseConst, _addedTargets);
            foreach (var item in _removedTargets)
                _targets.Remove(item);
            foreach (var target in _targets)
                _addedTargets.Remove(target);
            _targets.AddRange(_addedTargets);
            foreach (var item in _spell.Effects.Values)
            {
                switch (item.Type)
                {
                    case SpellEffectType.AuraModifier:
                        if ((item.Target & SpellTarget.Creature) != 0)
                            foreach (var creature in _addedTargets.Where(x => x is WO_MOB))
                                creature.Stats.AddAuraEffect(_guid, (Stats)item.Data01, item.BaseConst, item.Data02 == 1);
                        if ((item.Target & SpellTarget.Player) != 0)
                            foreach (var creature in _addedTargets.Where(x => x.IsPlayer))
                                creature.Stats.AddAuraEffect(_guid, (Stats)item.Data01, item.BaseConst, item.Data02 == 1);
                        break;
                }
            }
        }
        #region Events Handlers
        private void WO_VoidZone_OnDestroy()
        {
            if (_hasAura)
            {
                foreach (var item in _targets)
                    if (item.IsSpawned)
                        item.Stats.RemoveAuraEffects(_guid);
            }
            _onwer = null;
            _targets.Clear();
            _addedTargets.Clear();
            _removedTargets.Clear();
            _targets = null;
            _addedTargets = null;
            _removedTargets = null;
        }
        private void WO_VoidZone_OnUpdate(TimeSpan time)
        {
            if (_onwer.IsDead || !_onwer.IsSpawned || _ticks <= 0)
                Destroy();
            else
            {
                if (_hasAura)
                    UpdateAuraEffects();
                if ((_update -= time.Milliseconds) <= 0)
                {
                    _update = _period;
                    UpdateAreaEffects();
                    _ticks--;
                }
            }
        }
        #endregion
    }
}