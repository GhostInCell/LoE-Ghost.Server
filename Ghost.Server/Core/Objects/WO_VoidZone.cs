using Ghost.Server.Core.Events;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Mgrs.Map;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNetR;
using System;
using System.Linq;
using System.Numerics;

namespace Ghost.Server.Core.Objects
{
    public class WO_VoidZone : WorldObject
    {
        private float _ticks;
        private TimeSpan _time;
        private TimeSpan _period;
        private WO_Player _onwer;
        private Vector3 _position;
        private VoidZoneTick _tick;
        private readonly DB_Spell _spell;
        private readonly DB_SpellEffect _main;
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
            _ticks = main.AttackModifer;
            _time = TimeSpan.FromSeconds(main.LevelModifer);
            _period = TimeSpan.FromSeconds(_time.TotalSeconds / _ticks);
            Spawn();
        }
        public void Tick()
        {
            if (_onwer.IsSpawned)
            {
                if (--_ticks >= 0)
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
                    return;
                }
            }
            Destroy();
        }
        #region Events Handlers
        private void WO_VoidZone_OnSpawn()
        {
            _tick = new VoidZoneTick(this, _period);
        }
        private void WO_VoidZone_OnDestroy()
        {
            _tick?.Destroy();
            _tick = null;
            _onwer = null;
        }
        #endregion
    }
}