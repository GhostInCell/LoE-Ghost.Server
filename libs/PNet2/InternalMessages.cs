using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNet
{
    public static class DtoRMsgs
    {
        public const string NotAllowed = "badhost";
        public const string NoRoomId = "noroomid";
        public const string UnknownRoom = "badroom";
    }

    public static class DtoPMsgs
    {
        public const string UnknownPlayer = "badplayer";
        public const string BadToken = "badtoken";
        public const string TokenTimeout = "tokentimeout";
        public const string NoRoom = "noroom";
    }

    public static class PtoDMsgs
    {
        public const string RoomSwitch = "roomswitch";
    }

    internal static class DandRRpcs
    {
        public const byte ExpectPlayer = 1;
        public const byte RoomSwitch = 2;
        public const byte SyncNetUser = 3;
        public const byte PlayerConnected = 4;
        public const byte RoomAdd = 5;
        public const byte RoomRemove = 6;
        public const byte ExpectLeavingPlayer = 7;
        public const byte DisconnectPlayer = 8;

        public const byte Ping = 75;
        public const byte Pong = 76;

        public const byte DisconnectMessage = 100;
    }

    internal static class DandPRpcs
    {
        public const byte RoomSwitch = 1;

        public const byte Ping = 75;
        public const byte Pong = 76;

        public const byte DisconnectMessage = 100;
    }

    internal static class RandPRpcs
    {
        public const byte Instantiate = 1;
        public const byte FinishedRoomSwitch = 2;
        public const byte Destroy = 3;
        public const byte SceneObjectRpc = 4;
        public const byte Hide = 6;

        public const byte Ping = 75;
        public const byte Pong = 76;

        public const byte DisconnectMessage = 100;
    }
}
