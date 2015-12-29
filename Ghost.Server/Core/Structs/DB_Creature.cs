using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_Creature
    {
        public int ID;
        public int LootID;
        public int SpellID;
        public float Speed;
        public int Resource;
        public float Base_Armor;
        public float Base_Dodge;
        public float Base_Power;
        public ushort KillCredit;
        public float Attack_Rate;
        public float Base_Health;
        public float Base_Energy;
        public float Base_HP_Reg;
        public float Base_EP_Reg;
        public float Base_Resists;
        public float Base_Dmg_Min;
        public float Base_Dmg_Max;
        public CreatureFlags Flags;
        public DB_Creature(int id, int loot, byte flags, int spell, float speed, int resource, ushort killCredit, float aRate, float bResist, float bArmor, float bDodge, float bPower,
            float bHealth, float bEnergy, float bHPReg, float bEPReg, float bDMGMin, float bDMGMax)
        {
            ID = id;
            LootID = loot;
            Speed = speed;
            SpellID = spell;
            Resource = resource;
            Base_Armor = bArmor;
            Base_Dodge = bDodge;
            Base_Power = bPower;
            Attack_Rate = aRate;
            Base_HP_Reg = bHPReg;
            Base_EP_Reg = bEPReg;
            Base_Health = bHealth;
            Base_Energy = bEnergy;
            Base_Dmg_Min = bDMGMin;
            Base_Dmg_Max = bDMGMax;
            Base_Resists = bResist;
            KillCredit = killCredit;
            Flags = (CreatureFlags)flags;
        }
    }
}