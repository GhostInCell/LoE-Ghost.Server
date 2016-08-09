using Ghost.Server.Core.Servers;
using Ghost.Server.Utilities;
using PNet;
using PNetR;
using System;
using System.Linq;

namespace Ghost.Server.Scripts.Servers
{
    public class MapServerScript
    {
        private static readonly int SpamDelay;
        static MapServerScript()
        {
            SpamDelay = Configs.Get<int>(Configs.Game_SpamDelay);
        }
        private MapServer _server;
        public MapServerScript(MapServer server)
        {
            _server = server;
            server.Room.SubscribeRpcsOnObject(this);
        }
        [Rpc(11, false)]//Dialog Next
        private void RPC_011(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            if (player.Dialog != null)
                player.Dialog.Dialog.OnDialogNext(player);
        }
        [Rpc(12, false)]//Dialog Choice
        private void RPC_012(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            if (player.Dialog != null)
                player.Dialog.Dialog.OnDialogChoice(player, arg1.ReadInt32());
        }
        [Rpc(13, false)]//Dialog End
        private void RPC_013(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            if (player.Dialog != null)
            {
                player.Dialog.Dialog.OnDialogEnd(player);
                player.Dialog = null;
            }
        }
        [Rpc(15, false)]//Local Chat 
        private void RPC_015(NetMessage arg1, NetMessageInfo arg2)
        {
            var time = DateTime.Now;
            var icon = ChatIcon.None;
            ChatType type = (ChatType)arg1.ReadByte();
            string message = arg1.ReadString();
            var player = _server[arg2.Sender.Id];
            if (player.User.Access == AccessLevel.Admin)
                icon = ChatIcon.Admin;
            if (player.User.Access == AccessLevel.Moderator)
                icon = ChatIcon.Mod;
            if (message == Constants.StuckCommand)
                _server.Objects.Teleport(player.Object, player.User.Spawn);
            else if (message.Sum(x => char.IsUpper(x) ? 1 : 0) > message.Length / 4 + 4 || time < player.LastMsg)
                player.Player.SystemMsg(Constants.ChatWarning);
            else
            {
                player.LastMsg = time.AddMilliseconds(SpamDelay);
                ServerLogger.LogLocalChat(player, message);
                _server.Room.PlayerRpc(15, ChatType.Local, player.Char.Pony.Name, player.User.Name, message, player.User.Char, (int)arg2.Sender.Id, icon, DateTime.Now);
            }
        }
        [Rpc(20, false)]
        private void RPC_020(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            switch (arg1.ReadByte())
            {
                default:
                    arg1.Position -= 8;
                    ServerLogger.LogServer(_server, $"Unhandled sub 20 rpc {arg1.PeekByte()}");
                    break;
            }
        }
        [Rpc(49, false)]
        private void RPC_049(NetMessage arg1, NetMessageInfo arg2)
        {
            var player = _server[arg2.Sender.Id];
            switch (arg1.ReadByte())
            {
                case 206://tele
                    if (player.User.Access < AccessLevel.TeamMember)
                        player.SystemMsg($"You haven't permission to teleport");
                    else
                    {
                        string tPlayer = arg1.ReadString();
                        string tLevel = arg1.ReadString();
                        if (player.Char.Pony.Name == tPlayer)
                            player.Player.ChangeRoom(tLevel);
                        else
                            player.SystemMsg($"teleport player not implemented");
                    }
                    break;
                case 230://add xp
                    if (player.User.Access < AccessLevel.TeamMember)
                        player.SystemMsg($"You haven't permission to adding xp");
                    else
                        player.Stats.AddExpAll(arg1.ReadUInt32());
                    break;
                default:
                    arg1.Position -= 8;
                    ServerLogger.LogServer(_server, $"Unhandled sub 49 rpc {arg1.PeekByte()}");
                    break;
            }
        }
    }
}