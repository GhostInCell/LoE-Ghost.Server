namespace PNet
{
    public enum NetworkStatus
    {
        /// <summary>
        /// Not running, socket is not bound
        /// </summary>
        NotRunning = 0,

        /// <summary>
        /// In the process of starting
        /// </summary>
        Starting = 1,

        /// <summary>
        /// Bound to the socket and listening for packets
        /// </summary>
        Running = 2,

        /// <summary>
        /// Shutdown has been requested and will be executed shortly
        /// </summary>
        ShutdownRequested = 3,
    }
}