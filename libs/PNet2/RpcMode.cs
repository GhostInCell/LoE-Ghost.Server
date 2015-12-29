using System;

namespace PNet
{
    [Flags]
    public enum RpcMode : byte
    {
        AllUnreliable = BroadcastMode.All | ReliabilityMode.Unreliable,
        AllUnordered = BroadcastMode.All | ReliabilityMode.Unordered,
        AllOrdered = BroadcastMode.All | ReliabilityMode.Ordered,
        AllBuffered = BroadcastMode.All | ReliabilityMode.Buffered,

        OthersUnreliable = BroadcastMode.Others | ReliabilityMode.Unreliable,
        OthersUnordered = BroadcastMode.Others | ReliabilityMode.Unordered,
        OthersOrdered = BroadcastMode.Others | ReliabilityMode.Ordered,
        OthersBuffered = BroadcastMode.Others | ReliabilityMode.Buffered,

        OwnerUnreliable = BroadcastMode.Owner | ReliabilityMode.Unreliable,
        OwnerUnordered = BroadcastMode.Owner | ReliabilityMode.Unordered,
        OwnerOrdered = BroadcastMode.Owner | ReliabilityMode.Ordered,
        OwnerBuffered = BroadcastMode.Owner | ReliabilityMode.Buffered,

        ServerUnreliable = BroadcastMode.Server | ReliabilityMode.Unreliable,
        ServerUnordered = BroadcastMode.Server | ReliabilityMode.Unordered,
        ServerOrdered = BroadcastMode.Server | ReliabilityMode.Ordered,
        ServerBuffered = BroadcastMode.Server | ReliabilityMode.Buffered,
    }

    //top two bits
    public enum ReliabilityMode : byte
    {
        Unreliable = 0,
        Unordered = 1 << 6,
        Ordered = 2 << 6,
        Buffered = 3 << 6,
    }

    //top-mid bits
    public enum BroadcastMode : byte
    {
        All = 0,
        Others = 1 << 4,
        Owner = 2 << 4,
        //player and server are identical, because from player -> server it's just server, and from server -> player it's just one player.
        Server = 3 << 4,
    }

    //low-mid bits
    public enum MsgType : byte
    {
        Stream = 0,
        Internal = 1 << 2,
        Netview = 2 << 2,
        Static = 3 << 2,
    }

    //low bits
    public enum SubMsgType : byte
    {
        Out = 0,
        Error = 1,
        Reply = 2,
    }

    internal static class RpcUtils
    {
        public const byte ReliabilityMask = 3 << 6;
        public const byte BroadcastMask = 3 << 4;
        public const byte MsgTypeMask = 3 << 2;
        public const byte SubTypeMask = 3;

        public static SubMsgType RcpType(this byte value)
        {
            return (SubMsgType) (value & SubTypeMask);
        }

        public static MsgType MsgType(this byte value)
        {
            return (MsgType)(value & MsgTypeMask);
        }

        public static BroadcastMode BroadcastMode(this byte value)
        {
            return (BroadcastMode) (value & BroadcastMask);
        }

        public static ReliabilityMode ReliabilityMode(this byte value)
        {
            return (ReliabilityMode) (value & ReliabilityMask);
        }

        public static ReliabilityMode ReliabilityMode(this RpcMode value)
        {
            return ((byte)value).ReliabilityMode();
        }

        public static BroadcastMode BroadcastMode(this RpcMode value)
        {
            return ((byte) value).BroadcastMode();
        }

        public static RpcMode RpcMode(this byte value)
        {
            return (RpcMode)((value & ReliabilityMask) | (value & BroadcastMask));
        }

        public static RpcMode RpcMode(ReliabilityMode reliable, BroadcastMode broadcast)
        {
            //this is necessary because csharp refuses to give back the correct enum value.
            return (RpcMode) ((byte) reliable | (byte) broadcast);
        }

        public static byte GetHeader(ReliabilityMode rel, BroadcastMode broad, MsgType msg, SubMsgType sub = SubMsgType.Out)
        {
            //OR can be used because the values should not encroach in the other bit areas.
            byte ret = (byte)rel;
            ret |= (byte) broad;
            ret |= (byte) msg;
            ret |= (byte) sub;
            return ret;
            
            //todo: which is more efficient?
            
            //return (byte)((byte)rel | (byte)broad | (byte)msg | (byte)sub);
        }

        public static byte GetHeader(RpcMode mode, MsgType msg)
        {
            byte ret = (byte) mode;
            ret |= (byte) msg;
            return ret;
        }

        public static void ReadHeader(byte value, out ReliabilityMode rel, out BroadcastMode broad, out MsgType msg, out SubMsgType sub)
        {
            rel = (ReliabilityMode) (value & ReliabilityMask);
            broad = (BroadcastMode) (value & BroadcastMask);
            msg = (MsgType) (value & MsgTypeMask);
            sub = (SubMsgType) (value & SubTypeMask);
        }
    }
}