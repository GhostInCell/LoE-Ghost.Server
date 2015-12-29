using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNet;

namespace PNetR
{
    public class NetworkConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int MaximumPlayers;
        /// <summary>
        /// 
        /// </summary>
        public readonly int ListenPort;
        /// <summary>
        /// The address the Players can connect to. this is used by the dispatcher, and should be publicly accessible
        /// </summary>
        public readonly string ListenAddress;
        /// <summary>
        /// 
        /// </summary>
        public readonly int TickRate;
        /// <summary>
        /// this should be unique per game, and should be the same on the client and server
        /// </summary>
        public readonly string AppIdentifier;

        /// <summary>
        /// The name of the room that this represents.
        /// </summary>
        public readonly string RoomIdentifier;

        public readonly RoomAuthType RoomAuthType;
        /// <summary>
        /// data used for the specified authtype
        /// </summary>
        public readonly string AuthData;

        /// <summary>
        /// Extra data that is associated with a room on connecting to the dispatcher, not entirely associated with authorization itself
        /// </summary>
        public readonly string UserDefinedAuthData;

        public readonly string DispatcherAddress;
        public readonly int DispatcherPort;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maximumPlayers"></param>
        /// <param name="listenPort"></param>
        /// <param name="tickRate"></param>
        /// <param name="roomIdentifier"></param>
        /// <param name="appIdentifier"></param>
        /// <param name="dispatcherAddress"></param>
        /// <param name="dispatcherPort"></param>
        /// <param name="listenAddress"></param>
        /// <param name="roomAuthType"></param>
        /// <param name="authData"></param>
        /// <param name="userDefinedAuthData"></param>
        public NetworkConfiguration(
            int maximumPlayers = 32, int listenPort = 14000, int tickRate = 66, 
            string roomIdentifier = "room", string appIdentifier = "PNet", 
            string dispatcherAddress = "localhost", int dispatcherPort = 14001, string listenAddress = null,
            RoomAuthType roomAuthType = RoomAuthType.AllowedHost,
            string authData = "",
            string userDefinedAuthData = "")
        {
            MaximumPlayers = maximumPlayers;
            ListenPort = listenPort;
            TickRate = tickRate;
            AppIdentifier = appIdentifier;
            DispatcherAddress = dispatcherAddress;
            DispatcherPort = dispatcherPort;
            ListenAddress = listenAddress;
            RoomIdentifier = roomIdentifier;
            RoomAuthType = roomAuthType;
            AuthData = authData;
            UserDefinedAuthData = userDefinedAuthData;
        }
    }
}
