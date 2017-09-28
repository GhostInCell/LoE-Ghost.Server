using System.Diagnostics.Contracts;

namespace PNet
{
    public struct NetworkViewId : INetSerializable
    {
        private ushort _id;

        public ushort Id
        {
            get { return _id; }
        }

        public NetworkViewId(ushort id)
        {
            _id = id;
        }

// ReSharper disable once PureAttributeOnVoidMethod
        [Pure]
        public void OnSerialize(NetMessage message)
        {
            message.Write(_id);
        }

        public void OnDeserialize(NetMessage message)
        {
            _id = message.ReadUInt16();
        }

        public int AllocSize { get { return 2; }}
        
        public static readonly NetworkViewId Zero = new NetworkViewId(0);

        public static bool operator ==(NetworkViewId a, NetworkViewId b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(NetworkViewId a, NetworkViewId b)
        {
            return !(a == b);
        }

        public bool Equals(NetworkViewId other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NetworkViewId && Equals((NetworkViewId)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}