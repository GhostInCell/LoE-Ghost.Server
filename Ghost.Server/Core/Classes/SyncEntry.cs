using Ghost.Server.Utilities;
using PNet;
using System.Numerics;

namespace Ghost.Server.Core.Classes
{
    public class SyncEntry : INetSerializable
    {
        public double Time;
        public Vector3 Position;
        public Vector3 Rotation;
        public bool FullRotation;
        public int AllocSize
        {
            get
            {
                return (FullRotation ? 3 : 1) + 16;
            }
        }
        public override string ToString()
        {
            return $"time {Time:0.000000} <{Position.X:0.00}, {Position.Y:0.00}, {Position.Z:0.00}>";
        }
        public void OnSerialize(NetMessage message)
        {
            message.WriteFixedTime(Time, false);
            message.WritePosition(Position);
            message.WriteRotation(Rotation, FullRotation);
        }
        public void OnDeserialize(NetMessage message)
        {
            Time = message.ReadFixedTime(false);
            Position = message.ReadVector3();
            Rotation = message.ReadRotation(ref FullRotation);
        }
    }
}