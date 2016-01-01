using Ghost.Server.Core.Classes;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using PNet;
using PNetR;
using System.Linq;

namespace Ghost.Server.Scripts.Servers
{
    public class CharServerScript
    {
        private static readonly string[] startMap;
        static CharServerScript()
        {
            startMap = new string[4];
            startMap[1] = Configs.Get<string>(Configs.Game_EarthPonyStart);
            startMap[2] = Configs.Get<string>(Configs.Game_UnicornStart);
            startMap[3] = Configs.Get<string>(Configs.Game_PegasusStart);
        }
        private CharServer _server;
        public CharServerScript(CharServer server)
        {
            _server = server;
            server.Room.SubscribeRpcsOnObject(this);
        }
        [Rpc(1)]
        private void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id]; var pony = new PonyData();
            var index = arg1.ReadInt32(); pony.OnDeserialize(arg1); DB_Map map;
            if (player.UpdateCharacter(index, pony))
            {
                var character = index == -1 ? player.Data.Last() : player.Data[index];
                player.User.Char = character.ID;
                if (character.Map == 0 || !DataMgr.Select(character.Map, out map))
                    arg2.Sender.ChangeRoom(startMap[pony.Race]);
                else
                    arg2.Sender.ChangeRoom(map.Name);
                arg2.Sender.SynchNetData();
            }
            else
                arg2.Sender.Error("Error while saving pony");
        }
        [Rpc(2)]
        private void RPC_002(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            var index = arg1.ReadInt32();
            if (player.DeleteCharacter(index))
                player.SendPonies();
            else
                arg2.Sender.Error("Error while deleting pony");
        }
    }
}