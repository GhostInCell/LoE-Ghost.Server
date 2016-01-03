using Ghost.Server.Utilities;
using System.Numerics;

namespace Ghost.Server.Core.Classes
{
    public class MovementEntry
    {
        public int Data01;
        public int Data02;
        public Vector3 Position;
        public Vector3 Rotation;
        public MovementType Type;
        public int CommandData01;
        public int CommandData02;
        public MovementCommand Command;
        public MovementEntry(byte type, int data01, int data02, byte cmd, int cmdData01, int cmdData02, Vector3 position, Vector3 rotation)
        {
            Data01 = data01;
            Data02 = data02;
            Position = position;
            Rotation = rotation;
            Type = (MovementType)type;
            CommandData01 = cmdData01;
            CommandData02 = cmdData02;
            Command = (MovementCommand)cmd;
        }
    }
}