namespace PNetS
{
    public class NetworkConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int MaximumRooms;

        /// <summary>
        /// 
        /// </summary>
        public readonly int MaximumPlayers;
        
        /// <summary>
        /// Port to listen for player data on
        /// </summary>
        public readonly int PlayerListenPort;

        /// <summary>
        /// Port to listen for room data on
        /// </summary>
        public readonly int RoomListenPort;
        /// <summary>
        /// this should be unique per game, and should be the same on the client and server
        /// </summary>
        public readonly string AppIdentifier;

        /// <summary>
        /// semicolon-delimited list of valid hosts that the server will allow to be registered as rooms.
        /// </summary>
        public readonly string RoomHosts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maximumRooms"></param>
        /// <param name="playerListenPort"></param>
        /// <param name="roomListenPort"></param>
        /// <param name="appIdentifier"></param>
        /// <param name="roomHosts"></param>
        /// <param name="maximumPlayers"></param>
        public NetworkConfiguration(int maximumRooms = 256, 
            int playerListenPort = 14000, int roomListenPort = 14001, 
            string appIdentifier = "PNet", string roomHosts = "localhost;", 
            int maximumPlayers = 50000)
        {
            MaximumRooms = maximumRooms;
            PlayerListenPort = playerListenPort;
            RoomListenPort = roomListenPort;
            AppIdentifier = appIdentifier;
            RoomHosts = roomHosts;
            MaximumPlayers = maximumPlayers;
        }
    }
}