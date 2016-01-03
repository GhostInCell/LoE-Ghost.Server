using Ghost.Server.Core.Movment;
using Ghost.Server.Utilities.Abstracts;
using System;

namespace Ghost.Server.Core.Events
{
    public class MovementWait : TimedEvent<ScriptedMovement>
    {
        public MovementWait(ScriptedMovement data, TimeSpan time)
            : base(data, time, false)
        { }
        public override void OnFire()
        {
            _data.ResetWait();
        }
    }
}