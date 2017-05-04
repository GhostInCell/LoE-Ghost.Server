using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Servers;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface INPCArray
    {
        WO_NPC this[int index] { get; set; }
    }
}
