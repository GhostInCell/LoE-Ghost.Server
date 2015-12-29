using Ghost.Server.Core.Players;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class CooldownReset : TimedEvent<MapPlayer>
    {
        private int _skillID;
        public CooldownReset(MapPlayer target, int skillID, float seconds)
            : base(target, TimeSpan.FromSeconds(seconds), false)
        {
            _skillID = skillID;
        }
        public override void OnFire()
        {
            _data.Skills.RemoveCooldown(_skillID);
        }
    }
}