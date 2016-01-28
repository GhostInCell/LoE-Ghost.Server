using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class TimedModifer : TimedEvent<StatsMgr>
    {
        public readonly Stats Stat;
        public readonly float Value;
        public readonly bool IsMultiplier;
        public TimedModifer(StatsMgr stats, Stats stat, float value, bool isMul, float time)
            : base(stats, TimeSpan.FromSeconds(time), false)
        {
            Stat = stat;
            Value = value;
            IsMultiplier = isMul;
        }
        public override void OnFire()
        {
            _data.RemoveModifier(this);
        }
    }
}