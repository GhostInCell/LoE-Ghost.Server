using Ghost.Server.Core.Objects;

namespace Ghost.Server.Utilities.Interfaces.Script
{
    public interface IScriptedAI : IUpdatable
    {
        void SetOwner(WO_MOB creature);
    }
}