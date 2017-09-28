using PNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PNetR
{
    public partial class Room
    {
        private readonly Dictionary<Guid, WaitingToken> _waitingTokens = new Dictionary<Guid, WaitingToken>();
        public int PlayerCount { get; private set; }

        internal void PlayerWillConnect(NetMessage msg)
        {
            if (!msg.ReadGuid(out var guid))
            {
                Debug.LogError("Got a message for expecting a player, but no token was sent");
                return;
            }
            var id = msg.ReadUInt16();

            if (_waitingTokens.TryGetValue(guid, out var wait))
            {
                wait.Id = id;
            }
            else
            {
                _waitingTokens[guid] = new WaitingToken { Id = id };
            }
        }

        internal void VerifyPlayerConnecting(Player player, Guid token)
        {
            if (_waitingTokens.TryGetValue(token, out var wait))
            {
                wait.Player = player;
            }
            else
            {
                _waitingTokens[token] = new WaitingToken { Player = player };
            }
        }

        private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(10);

        internal void VerifyWaitingPlayers()
        {
            if (_waitingTokens.Count == 0) return;

            foreach (var token in _waitingTokens.ToArray())
            {
                if (DateTime.UtcNow - token.Value.StartTime > WaitTime)
                {
                    if (token.Value.Player != null) 
                        token.Value.Player.Disconnect(DtoPMsgs.TokenTimeout);
                    
                    _waitingTokens.Remove(token.Key);
                    continue;
                }
                
                if (token.Value.Player == null || token.Value.Id == 0) continue;

                _waitingTokens.Remove(token.Key);
                token.Value.Player.Token = token.Key;
                token.Value.Player.Id = token.Value.Id;
#if DEBUG
                Debug.Log($"Verified waiting player {token.Value.Player}");
#endif
                token.Value.Player.AllowConnect();
            }
        }

        internal void AllowConnect(Player player)
        {
            _roomServer.AllowConnect(player);
        }

        internal void SendToPlayer(Player player, NetMessage msg, ReliabilityMode mode)
        {
            _roomServer.SendToPlayer(player, msg, mode);
        }

        internal void SendToPlayers(NetMessage msg, ReliabilityMode mode)
        {
            _roomServer.SendToPlayers(msg, mode);
        }

        internal void SendToConnections(List<object> connections, NetMessage msg, ReliabilityMode reliable)
        {
            _roomServer.SendToConnections(connections, msg, reliable);
        }

        internal void SendExcept(NetMessage msg, Player except, ReliabilityMode mode)
        {
            _roomServer.SendExcept(msg, except, mode);
        }
        
        internal void SendToPlayers(List<Player> players, NetMessage msg, ReliabilityMode mode)
        {
            _roomServer.SendToPlayers(players, msg, mode);
        }

        internal void SendSceneView(NetMessage msg, ReliabilityMode mode)
        {
            _roomServer.SendSceneView(msg, mode);
        }

        internal void Disconnect(Player player, string reason)
        {
            _roomServer.Disconnect(player, reason);
        }
        
        class WaitingToken
        {
            public readonly DateTime StartTime;
            public ushort Id;
            public Player Player;

            public WaitingToken()
            {
                StartTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// send the player all the existing network views
        /// </summary>
        /// <param name="player"></param>
        internal void SendViewInstantiates(Player player)
        {
            CleanupInvalidNetworkViewOwners();

            try
            {
                PlayerAdded?.Invoke(player);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void RequestAllRoomViews(Player player)
        {
            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null) continue;
                if (!view.OnPlayerEnteredRoom(player)) continue;
                if(view.Owner == player) continue;

                var pos = view.GetPosition();
                var rot = view.GetRotation();

                var msg = ConstructInstMessage(view, pos, rot);
                SendToPlayer(player, msg, ReliabilityMode.Ordered);
            }
        }

        public void RequestUpdatedRoomViews(Player player, bool shouldDestroy = false)
        {
            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null) continue;
                if(view.Owner.Id > 0) // Need a better way to handle this
                    continue;

                if (shouldDestroy)
                {
                    var destroymsg = GetDestroyMessage(view, RandPRpcs.Destroy, 0);
                    SendToPlayer(player, destroymsg, ReliabilityMode.Ordered);
                }
                if (!view.OnPlayerEnteredRoom(player)) continue;

                var pos = view.GetPosition();
                var rot = view.GetRotation();

                var msg = ConstructInstMessage(view, pos, rot);
                SendToPlayer(player, msg, ReliabilityMode.Ordered);
            }
        }

        /// <summary>
        /// send a static rpc to all players in this room
        /// </summary>
        /// <param name="rpcId"></param>
        /// <param name="args"></param>
        public void PlayerRpc(byte rpcId, params object[] args)
        {
            var size = 0;
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new NullReferenceException("Cannot serialize null value");

                size += Serializer.SizeOf(arg);
            }

            var msg = Player.StartMessage(this, rpcId, ReliabilityMode.Ordered, size);
            foreach (var arg in args)
            {
                Serializer.Serialize(arg, msg);
            }
            SendToPlayers(msg, ReliabilityMode.Ordered);
        }

        internal Player ConstructNewPlayer()
        {
            return ConstructNetData != null ? new Player(this, ConstructNetData()) : new Player(this);
        }

        internal void AddPlayer(Player player)
        {
            Player oldPlayer;
            _players.TryGetValue(player.Id, out oldPlayer);
            if (oldPlayer != null)
            {
                Debug.LogWarning($"Contention over id {player.Id} : {oldPlayer} is still connected, but should probably not be. Disconnecting");
                oldPlayer.Disconnect("player id contention");
            }

            _players[player.Id] = player;
            PlayerCount++;
            Debug.Log($"Player {player.Id} joined at {player.Connection}");
            SendViewInstantiates(player);

            var pconnected = ServerGetMessage(4);
            pconnected.Write(RpcUtils.GetHeader(ReliabilityMode.Ordered, BroadcastMode.Server, MsgType.Internal));
            pconnected.Write(DandRRpcs.PlayerConnected);
            pconnected.Write(player.Id);
            Server.SendMessage(pconnected, ReliabilityMode.Ordered);
        }

        internal void RemovePlayer(Player player)
        {
            if (player.Id == 0)
            {
                Debug.LogWarning("Player disconnected with id 0. They probably didn't finish connecting");
            }
            else
            {
                Player oplayer;
                _players.TryGetValue(player.Id, out oplayer);
                if (oplayer != player)
                {
                    Debug.Log($"Finished removing player {player} over contention with id {player.Id}");
                }
                else
                {
                    _players.Remove(player.Id);
                    PlayerCount--;
                }

                try
                {
                    PlayerRemoved.Raise(player);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
