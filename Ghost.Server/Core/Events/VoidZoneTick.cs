using Ghost.Server.Core.Objects;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class VoidZoneTick : TimedEvent<WO_VoidZone>
    {
        public VoidZoneTick(WO_VoidZone data, TimeSpan time) 
            : base(data, time, true)
        {

        }
        public override void OnFire()
        {
            _data.Tick();
        }
    }
}