using PNet;
using System;

namespace PNetR
{
    public abstract class ADispatchClient
    {
        protected internal Room Room;

        protected void Connected(Guid roomId, object connection)
        {
            Room.RoomId = roomId;
            Debug.Log($"Connected to dispatcher. Id is {roomId}");
            Room.Server = new Server(Room) { Connection = connection };
        }

        protected void UpdateConnectionStatus(ConnectionStatus status)
        {
            Room.UpdateDispatchConnectionStatus(status);
        }

        protected void Disconnected()
        {
            Room.Server = null;
            Room.RoomId = Guid.Empty;
            Debug.Log("Disconnected from dispatcher");
        }

        protected internal abstract void Setup();
        protected internal abstract void Connect();
        protected internal abstract void ReadQueue();
        protected internal abstract void Disconnect(string reason);
        protected internal abstract NetMessage GetMessage(int size);
        protected internal abstract void SendMessage(NetMessage msg, ReliabilityMode mode);

        protected void ConsumeData(NetMessage msg)
        {
            if (Room.Server != null)
                Room.Server.ConsumeData(msg);
            else
                Debug.LogWarning("Received server data when not connected");
        }
    }
}
