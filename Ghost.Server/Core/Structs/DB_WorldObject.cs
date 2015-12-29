using System;
using System.Numerics;

namespace Ghost.Server.Core.Structs
{
    public struct DB_WorldObject
    {
        public int Map;
        public byte Type;
        public byte Flags;
        public int Data01;
        public int Data02;
        public int Data03;
        public ushort Guid;
        public int ObjectID;
        public TimeSpan Time;
        public Vector3 Position;
        public Vector3 Rotation;
        public DB_WorldObject(int map, ushort guid, int objectID, byte type, byte flags, Vector3 position, Vector3 rotation,
            float time = -1f, int data01 = -1, int data02 = -1, int data03 = -1)
        {
            Map = map;
            Guid = guid;
            Type = type;
            Flags = flags;
            Data01 = data01;
            Data02 = data02;
            Data03 = data03;
            ObjectID = objectID;
            Position = position;
            Rotation = rotation;
            Time = TimeSpan.FromSeconds(time);
        }
    }
}