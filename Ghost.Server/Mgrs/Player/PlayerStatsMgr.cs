using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using System;
using System.Linq;
using static PNet.NetConverter;

namespace Ghost.Server.Mgrs.Player
{
    public class PlayerStatsMgr : StatsMgr
    {
        protected static readonly float s_exp_multipler;
        static PlayerStatsMgr()
        {
            s_exp_multipler = Configs.Get<float>(Configs.Game_ExpMultipler);
        }
        private const int runMod = 18;
        private const int interval = 200;
        private int _update;
        private string _status;
        private MapPlayer _mPlayer;
        public string Status
        {
            get
            {
                return _status;
            }
        }
        public override int MeleeSkill
        {
            get
            {
                return 0;
            }
        }
        public override float AttackRate
        {
            get
            {
                return 0f;
            }
        }
        public override float MeleeDamage
        {
            get
            {
                return 0f;
            }
        }
        public PlayerStatsMgr(WO_Player parent)
            : base(parent)
        {
            _mPlayer = parent.Player;
            if (_stats.Count == 0) CreateBase();
            parent.OnDestroy += PlayerStatsMgr_OnDestroy;
            UpdateStats();
        }
        public override void UpdateStats()
        {
            foreach (var stat in _stats)
                stat.Value.CleanItems();
            foreach (var item in _mPlayer.Char.Data.Wears)
            {
                var entry = DataMgr.SelectItem(item.Value.Id);
                if ((entry.Flags & ItemFlags.Stats) > 0)
                    foreach (var stat in entry.Stats)
                        if (_stats.ContainsKey(stat.Item1))
                            _stats[stat.Item1].AddItemModifer(stat.Item2);
                        else
                            ServerLogger.LogWarn($"Player stat {stat.Item1} not found");
            }
            if (_view != null) SendStats();
        }
        public override void Update(TimeSpan time)
        {
            if (_creature.IsDead || (_update -= time.Milliseconds) > 0) return;
            _update = interval;
            var hp = _stats[Stats.Health];
            var ep = _stats[Stats.Energy];
            var net = default(StatNetData);
            if (_creature.Movement.IsRunning && _mPlayer.User.Access < AccessLevel.TeamMember)
            {
                ep.DecreaseCurrent(runMod * (interval / 1000f));
                if (ep.Current == 0) _creature.Movement.Lock();
            }
            if (hp.Max != hp.Current)
            {
                hp.IncreaseCurrent(_stats[Stats.HealthRegen].Max * (interval / 1000f));
                _view.Rpc(4, 50, RpcMode.AllUnordered, net.Fill(Stats.Health, hp.Current));
            }
            if (ep.Max != ep.Current)
            {
                ep.IncreaseCurrent(_stats[Stats.EnergyRegen].Max * (interval / 1000f));
                _view.Rpc(4, 50, RpcMode.AllUnordered, net.Fill(Stats.Energy, ep.Current));
            }
            _status = $"HP {hp.Current}/{hp.Max}; EP {ep.Current}/{ep.Max}";
        }
        public void AddExpAll(uint exp, uint bonusExp = 0)
        {
            var talents = _mPlayer.Data.Talents.Keys.ToArray();
            for (int i = 0; i < talents.Length; i++)
            {
                if ((talents[i] & TalentMarkId.All) != 0)
                    AddExp(talents[i], exp, bonusExp);
            }

            talents = null;
        }
        public void AddExp(TalentMarkId talant, uint exp, uint bonusExp = 0)
        {
            var talantState = _mPlayer.Data.Talents[talant];
            if (talantState.Level >= CharsMgr.MaxLevel)
                return;
            exp = (uint)(exp * s_exp_multipler);
            bonusExp = (uint)(bonusExp * s_exp_multipler);
            if (CalculateTalentLevel(talantState, exp + bonusExp))
            {
                UpdateBase();
                _mPlayer.Player.Rpc(4, _mPlayer.Data.SerTalents);
                _mPlayer.Player.Rpc(3, new TalentNetData((uint)talant, talantState.Exp, (uint)talantState.Level));
                _view.Rpc<Int16Serializer>(4, 53, RpcMode.AllUnordered, _mPlayer.Char.Level);
            }
            else
                _mPlayer.Player.Rpc(2, new TalentNetData((uint)talant, exp, bonusExp));
        }
        private short GetLevel()
        {
            short ret = _mPlayer.Data.Talents.Values.Max(x => x.Level);
            return ret > CharsMgr.MaxLevel ? CharsMgr.MaxLevel : ret;
        }
        private void CreateBase()
        {
            _level = (short)(_mPlayer.Char.Level == 0 ? 1 : _mPlayer.Char.Level);
            _stats[Stats.Dodge] = new StatValue(0);
            _stats[Stats.Armor] = new StatValue(0);
            _stats[Stats.MagicResist] = new StatValue(0);
            _stats[Stats.Health] = new StatValue(250 + _level * 100);
            _stats[Stats.Attack] = new StatValue(1 + (_level - 1) * 17);
            switch (_mPlayer.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.Speed] = new StatValue(350);
                    _stats[Stats.Energy] = new StatValue(125);
                    _stats[Stats.EnergyRegen] = new StatValue(10);
                    _stats[Stats.HealthRegen] = new StatValue(20 + _level * 10);
                    break;
                case 2:
                    _stats[Stats.Speed] = new StatValue(307);
                    _stats[Stats.Energy] = new StatValue(100);
                    _stats[Stats.EnergyRegen] = new StatValue(11);
                    _stats[Stats.HealthRegen] = new StatValue(28 + (_level - 1) * 9.5f);
                    break;
                case 3:
                    _stats[Stats.Speed] = new StatValue(310);
                    _stats[Stats.Energy] = new StatValue(100);
                    _stats[Stats.EnergyRegen] = new StatValue(11);
                    _stats[Stats.HealthRegen] = new StatValue(28 + (_level - 1) * 9.5f);
                    break;
            }
        }
        private void UpdateBase()
        {
            _mPlayer.Char.Level = _level = GetLevel();
            _stats[Stats.Health].SetBase(250 + _level * 100);
            _stats[Stats.Attack].SetBase(1 + (_level - 1) * 17);
            switch (_mPlayer.Char.Pony.Race)
            {
                case 1:
                    _stats[Stats.HealthRegen].SetBase(20 + _level * 10);
                    break;
                case 2:
                case 3:
                    _stats[Stats.HealthRegen].SetBase(28 + (_level - 1) * 9.5f);
                    break;
            }
            SendStats();
        }
        private bool CalculateTalentLevel(TalentData talent, uint exp)
        {
            if (talent.Level >= CharsMgr.MaxLevel)
                return false;
            var cExp = talent.Exp + exp;
            var level = (short)(talent.Level <= 0 ? 1 : talent.Level);
            while (level < CharsMgr.MaxLevel)
            {
                var nExp = CharsMgr.GetExpForLevel(level);
                if (cExp < nExp) break;
                cExp -= nExp;
                level++;
            }
            talent.Exp = cExp;
            if (level != talent.Level)
            {
                talent.Points += (short)((level - talent.Level) * CharsMgr.TalentPointsPerLevel);
                talent.Level = level;
                return true;
            }
            return false;
        }
        #region Events Handlers
        private void PlayerStatsMgr_OnDestroy()
        {
            _mPlayer.Char.Level = GetLevel();
            _status = null;
            _mPlayer = null;
        }
        #endregion
    }
}