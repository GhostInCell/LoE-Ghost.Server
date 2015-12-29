using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Structs
{
    public struct DB_SpellEffect
    {
        public float Data01;
        public float Data02;
        public float Data03;
        public float BaseConst;
        public float LevelModifer;
        public SpellTarget Target;
        public float AttackModifer;
        public SpellEffectType Type;
        public DB_SpellEffect(byte type, byte target, float data01, float data02, float data03, float bConst, float mLevel, float mAttack)
        {
            Type = (SpellEffectType)type;
            Target = (SpellTarget)target;
            Data01 = data01;
            Data02 = data02;
            Data03 = data03;
            BaseConst = bConst;
            LevelModifer = mLevel;
            AttackModifer = mAttack;
        }
    }
}