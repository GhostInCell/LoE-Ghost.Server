using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Servers;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface IDialog
    {
        ushort ID { get; }
        WO_NPC[] NPC { get; }
        MapServer Server { get; }
        void OnDialogEnd(MapPlayer player);
        void OnDialogNext(MapPlayer player);
        void OnDialogStart(MapPlayer player);
        void OnDialogChoice(MapPlayer player, int choice);
    }
}