using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Events;
using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Utilities.Abstracts;
using PNet;
using PNetR;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Mgrs.Player
{
    [NetComponent(5)]
    public class SkillsMgr: ObjectComponent
    {
        private MapPlayer _mPlayer;
        private WO_Player _wPlayer;
        private TargetEntry _entry;
        private Dictionary<int, int> _skills;
        private Dictionary<int, CooldownReset> _cooldown;
        public SkillsMgr(WO_Player parent)
            :base(parent)
        {
            _wPlayer = parent;
            _mPlayer = _wPlayer.Player;
            _entry = new TargetEntry();
            _skills = _mPlayer.Data.Skills;
            _cooldown = new Dictionary<int, CooldownReset>();
            parent.OnSpawn += SkillsMgr_OnSpawn;
            parent.OnDestroy += SkillsMgr_OnDestroy;
        }
        public void RemoveCooldown(int skillID)
        {
            _cooldown.Remove(skillID);
        }
        public void AddCooldown(int skillID, float seconds)
        {
            _cooldown.Add(skillID, new CooldownReset(_mPlayer, skillID, seconds));
        }
        #region RPC Handlers
        [Rpc(61, false)]//Perform Skill
        private void RPC_061(NetMessage arg1, NetMessageInfo arg2)
        {
            _entry.OnDeserialize(arg1);
            if (!_skills.ContainsKey(_entry.SkillID) || _cooldown.ContainsKey(_entry.SkillID))
                return;
            if (SpellsMgr.CanCast(_mPlayer, _entry))
                SpellsMgr.PerformSkill(_mPlayer, _entry);
        }
        [Rpc(62, false)]//Cancel Skill
        private void RPC_062(NetMessage arg1, NetMessageInfo arg2)
        {

        }
        #endregion
        #region Events Handlers
        private void SkillsMgr_OnSpawn()
        {
            _wPlayer.View.SubscribeMarkedRpcsOnComponent(this);
        }
        private void SkillsMgr_OnDestroy()
        {
            var toDestroy = _cooldown.ToArray();
            for (int i = 0; i < toDestroy.Length; i++)
                toDestroy[i].Value.Destroy();
            _cooldown.Clear();
            _skills = null;
            _mPlayer = null;
            _wPlayer = null;
            _cooldown = null;
        }
        #endregion
    }
}