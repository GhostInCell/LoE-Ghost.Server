using Ghost.Server.Core.Players;

namespace Ghost.Server.Utilities.Interfaces.Script
{
    public interface IScriptedItem
    {
        void OnUse(MapPlayer player);
    }
}