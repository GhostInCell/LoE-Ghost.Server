using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PNet;

namespace PNetR
{
    public partial class Room
    {
        private readonly Dictionary<Guid, WaitingToken> _waitingTokens = new Dictionary<Guid, WaitingToken>();

        internal void PlayerWillConnect(NetMessage msg)
        {
            Guid guid;
            if (!msg.ReadGuid(out guid))
            {
                Debug.LogError("Got a message for expecting a player, but no token was sent");
                return;
            }
            var id = msg.ReadUInt16();
            
            WaitingToken wait;
            if (_waitingTokens.TryGetValue(guid, out wait))
            {
                wait.Id = id;
            }
            else
            {
                _waitingTokens[guid] = new WaitingToken {Id = id};
            }
        }

        private void VerifyPlayerConnecting(Player player, Guid token)
        {
            WaitingToken wait;
            if (_waitingTokens.TryGetValue(token, out wait))
            {
                wait.Player = player;
            }
            else
            {
                _waitingTokens[token] = new WaitingToken { Player = player };
            }
        }

        private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(10);
        void VerifyWaitingPlayers()
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

        internal void SendToPlayers(NetMessage msg, ReliabilityMode mode)
        {
            ImplSendToPlayers(msg, mode);
        }
        partial void ImplSendToPlayers(NetMessage msg, ReliabilityMode mode);

        internal void SendExcept(NetMessage msg, Player except, ReliabilityMode mode)
        {
            ImplSendExcept(msg, except, mode);
        }
        partial void ImplSendExcept(NetMessage msg, Player except, ReliabilityMode mode);
        partial void ImplSendToPlayer(Player player, NetMessage msg, ReliabilityMode mode);

        internal void SendToPlayers(Player[] players, NetMessage msg, ReliabilityMode mode)
        {
            ImplSendToPlayers(players, msg, mode);
        }
        partial void ImplSendToPlayers(Player[] players, NetMessage msg, ReliabilityMode mode);

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

            foreach (var view in NetworkManager.AllViews)
            {
                if (view == null) continue;
                if (!view.OnPlayerEnteredRoom(player)) continue;

                var pos = view.GetPosition();
                var rot = view.GetRotation();

                var msg = ConstructInstMessage(view, pos, rot);
                ImplSendToPlayer(player, msg, ReliabilityMode.Ordered);
            }

            try
            {
                PlayerAdded?.Invoke(player);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
            ImplSendToPlayers(msg, ReliabilityMode.Ordered);
        }
    }
}
