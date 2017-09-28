using Ghost.Server.Terrain.Primitives;
using System;
using System.Numerics;

namespace Ghost.Server.Utilities
{
    public static class Constants
    {
        public static readonly Random RND = new Random();

        public const int LevelExpMultipler = 100;

        public const byte TypeIDPet = 0x10;
        public const byte TypeIDMOB = 0x20;
        public const byte TypeIDNPC = 0x40;
        public const byte TypeIDLoot = 0x80;

        public const byte TypeIDSpawn = 0x80;
        public const byte TypeIDSwitch = 0x40;
        public const byte TypeIDSpawnPool = 0x20;

        public const byte TypeIDPlayer = 0x80;
        public const byte TypeIDPickup = 0x40;

        public const byte TypeIDVoidZone = 0x80;

        public const uint ReleaseGuide = 0x1000000;
        public const uint ClonedObject = 0x4000000;
        public const uint ServerObject = 0x80000000;
        public const uint PlayerObject = 0x40000000;
        public const uint DynamicObject = 0x8000000;
        public const uint CreatureObject = 0x20000000;
        public const uint InteractObject = 0x10000000;

        public const uint IDRObject = IRObject | DynamicObject;
        public const uint CRObject = ClonedObject | ReleaseGuide;
        public const uint DRObject = DynamicObject | ReleaseGuide;
        public const uint IRObject = InteractObject | ReleaseGuide;

        public const uint PlayerVarPet = 0xF0000000;

        public const int ArrayCapacity = 5;

        public const int MuteCheckDelay = 10;

        public const int RoomVersion = 20170922;

        public const byte Killed = 1;
        public const byte Fainted = 3;
        public const byte Destroyed = 0;
        public const int LootResource = 3;
        public const int MaxWornItems = 32;
        public const int MaxServerName = 10;
        public const string Master = "Master";
        public const float LootDespawnTime = 32f;
        public const float PlayerRespawnTime = 8f;

        public const float AnnounceDuration = 1.5f;

        public static readonly BoundingBox DefaultRoomBounds = new BoundingBox(new Vector3(-3000f, -2000f, -3000f), new Vector3(5000f, 3000f, 5000f));

        public static readonly BoundingBox DefaultRotationBounds = new BoundingBox(new Vector3(-6.28318548f), new Vector3(6.28318548f));

        public const string StuckCommand = "unstuck me";
        public const string Characters = "Characters";
        public const string Configs = "loe_server.cfg";       
        public const string NoErrors = "no errors or warnings";
        public const string NoScripts = "scripts directory not found or empty";
        public const string ChatWarning = "CAPSLOCK and spamming not allowed!";
        public static readonly string DeadMsg = $"Respawn in {PlayerRespawnTime} seconds";

        public const float EpsilonX1 = 0.1f;
        public const float EpsilonX2 = 0.01f;
        public const float EpsilonX3 = 0.001f;


        public const float MaxCombatDistance = 64f;
        public const float MaxSpellsDistance = 32f;
        public const float MaxVisibleDistance = 128f;
        public const float MaxInteractionDistance = 8f;
        public const float MaxMeleeCombatDistance = 2.5f;

        public const float MaxCombatDistanceSquared = MaxCombatDistance * MaxCombatDistance;
        public const float MaxSpellsDistanceSquared = MaxSpellsDistance * MaxSpellsDistance;
        public const float MaxVisibleDistanceSquared = MaxVisibleDistance * MaxVisibleDistance;
        public const float MaxMeleeCombatDistanceSquared = MaxMeleeCombatDistance * MaxMeleeCombatDistance;
        public const float MaxInteractionDistanceSquared = MaxInteractionDistance * MaxInteractionDistance;
    }
}