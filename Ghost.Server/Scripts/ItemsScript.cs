using Ghost.Server.Core.Players;
using Ghost.Server.Utilities;

namespace Ghost.Server.Scripts
{
    public static class ItemsScript
    {
        public static void Use(int id, MapPlayer player)
        {
            switch (id)
            {
                case 81:
                    player.SetPet(player.Pet?.ID == 2 ? -1 : 2);
                    break;
                case 82:
                    player.SetPet(player.Pet?.ID == 1 ? -1 : 1);
                    break;
                case 93:
                    player.SetPet(player.Pet?.ID == 3 ? -1 : 3);
                    break;
                default:
                    ServerLogger.LogServer(player, $"NotImplemented: using item {id}");
                    break;
            }
        }
    }
}