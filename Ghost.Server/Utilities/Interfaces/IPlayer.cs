using Ghost.Server.Core.Classes;
using PNetR;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface IPlayer
    {
        Player Player { get; }
        string Status { get; }
        UserData User { get; }
        IServer Server { get; }
    }
}