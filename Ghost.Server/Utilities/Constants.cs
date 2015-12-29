using System;

namespace Ghost.Server.Utilities
{
    public static class Constants
    {
        public static readonly Random RND = new Random();

        public const byte TypeIDPet = 0x10;
        public const byte TypeIDNPC = 0x40;
        public const byte TypeIDMOB = 0x20;
        public const byte TypeIDLoot = 0x80;
        public const byte TypeIDSpawn = 0x80;
        public const byte TypeIDSwitch = 0x40;
        public const byte TypeIDPlayer = 0x80;
        public const byte TypeIDPickup = 0x40;
        public const byte TypeIDVoidZone = 0x80;

        public const uint ReleaseGuide = 0x1000000;
        public const uint ServerObject = 0x80000000;
        public const uint PlayerObject = 0x40000000;
        public const uint DynamicObject = 0x8000000;
        public const uint CreatureObject = 0x20000000;
        public const uint InteractObject = 0x10000000;

        public const uint IDRObject = IRObject | DynamicObject;
        public const uint DRObject = ReleaseGuide | DynamicObject;
        public const uint IRObject = InteractObject | ReleaseGuide;

        public const uint PlayerVarPet = 0xF0000000;

        public const byte Killed = 1;
        public const int MaxServerName = 10;
        public const byte MaxWornItems = 32;
        public const string Master = "Master";
        public const float LootDespawnTime = 32f;
        public const float PlayerRespawnTime = 8f;
        
        public const float MaxSkillsDistance = 32f;
        public const float AnnounceDuration = 1.5f;

        public const float MeleeCombatDistance = 1.5f;

        public const string Characters = "Characters";
        public const float MaxInteractionDistance = 8f;
        public const string Configs = "loe_server.cfg";
        public const string NoErrors = "no errors or warnings";
        public const string NoScripts = "scripts directory not found or empty";
        public const string ChatWarning = "CAPSLOCK and spamming not allowed!";
        public static readonly string DeadMsg = $"Rest In Peace...\r\nRespawn in {PlayerRespawnTime} seconds";
    }
}