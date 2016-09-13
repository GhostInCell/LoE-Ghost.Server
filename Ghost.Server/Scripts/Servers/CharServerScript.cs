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
        private static readonly string[] StartMaps;

        static CharServerScript()
        {
            StartMaps = new string[4];
            StartMaps[1] = Configs.Get<string>(Configs.Game_EarthPonyStart);
            StartMaps[2] = Configs.Get<string>(Configs.Game_UnicornStart);
            StartMaps[3] = Configs.Get<string>(Configs.Game_PegasusStart);
        }

        private CharServer m_server;

        public CharServerScript(CharServer server)
        {
            m_server = server;
            server.Room.SubscribeRpcsOnObject(this);
        }

        [Rpc(1)]
        private void RPC_001(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = m_server[arg2.Sender.Id]; var pony = new PonyData();
            var index = arg1.ReadInt32(); pony.OnDeserialize(arg1); DB_Map map;
            if (player.UpdateCharacter(index, pony))
            {
                var character = index == -1 ? player.Data.Last() : player.Data[index];
                player.User.Char = character.ID;
                if (character.Map == 0 || !DataMgr.Select(character.Map, out map))
                    arg2.Sender.ChangeRoom(StartMaps[pony.Race]);
                else
                    arg2.Sender.ChangeRoom(map.Name);
                arg2.Sender.SynchNetData();
            }
            else
                arg2.Sender.Error("Error while saving pony, most likely character isn't unique.");
        }

        [Rpc(2)]
        private void RPC_002(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = m_server[arg2.Sender.Id];
            var index = arg1.ReadInt32();
            if (player.DeleteCharacter(index))
                player.SendPonies();
            else
                arg2.Sender.Error("Error while deleting pony");
        }
    }
}