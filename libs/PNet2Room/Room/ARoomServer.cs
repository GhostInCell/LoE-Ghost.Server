using PNet;
using System;
using System.Collections.Generic;

namespace PNetR
{
    public abstract class ARoomServer
    {
        protected internal Room Room;

        protected internal abstract void Setup();
        protected internal abstract void Start();
        protected internal abstract void ReadQueue();
        protected internal abstract void Shutdown(string reason);

        protected internal abstract NetMessage GetMessage(int size);

        protected void AddingPlayer(Player player)
        {
            Room.AddPlayer(player);
        }

        protected void RemovePlayer(Player player)
        {
            Room.RemovePlayer(player);
        }

        protected Player ConstructNewPlayer(object connection)
        {
            var player = Room.ConstructNewPlayer();
            player.Connection = connection;
            return player;
        }

        protected void VerifyPlayerConnecting(Player player, Guid token)
        {
            Room.VerifyPlayerConnecting(player, token);
        }

        protected void VerifyWaitingPlayers()
        {
            Room.VerifyWaitingPlayers();
        }

        protected internal abstract void SendToPlayers(NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendToPlayers(List<Player> players, NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode);
        protected internal abstract void SendExcept(NetMessage msg, Player except, ReliabilityMode mode);
        protected internal abstract void SendSceneView(NetMessage msg, ReliabilityMode mode);
        
        protected internal abstract void AllowConnect(Player player);
        protected internal abstract void Disconnect(Player player, string reason);
        protected internal abstract void SendToConnections(List<object> connections, NetMessage msg, ReliabilityMode reliable);

        protected void ConsumeData(Player player, NetMessage msg)
        {
            player.ConsumeData(msg);
        }
    }
}
