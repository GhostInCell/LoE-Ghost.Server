using PNet;
using System;

namespace Ghost.Server.Core.Classes
{
    public class MoreInstantiates : INetSerializable
    {
        public string JsonData { get; private set; }
        public string[] Components { get; private set; }

        public MoreInstantiates(string[] components, string jsonData)
        {
            components = components ?? Array.Empty<string>();
            if (components.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(components), "maximum length is 255");
            Components = components;

            JsonData = jsonData ?? string.Empty;

            foreach (var c in Components)
                AllocSize += c.Length * 2;
            AllocSize += JsonData.Length * 2;
        }

        public int AllocSize { get; private set; }

        public void OnDeserialize(NetMessage msg)
        {
            var cc = msg.ReadByte();
            Components = new string[cc];
            for (var i = 0; i < cc; i++)
                Components[i] = msg.ReadString();
            JsonData = msg.ReadString();
        }

        public void OnSerialize(NetMessage msg)
        {
            msg.Write((byte)Components.Length);
            foreach (var c in Components)
                msg.Write(c);
            msg.Write(JsonData);
        }
    }
}
