using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Classes
{
    public class StatHelper
    {
        private float stat_muls;
        private float stat_mods;
        private float stat_item;

        private float stat_base;
        private float stat_curr;

        private float min_chace;
        private float max_chace;
        public float Max
        {
            get { return max_chace; }
        }
        public float Min
        {
            get { return min_chace; }
        }
        public float Current
        {
            get { return stat_curr; }
        }
        public StatHelper(float val)
        {
            min_chace = 0f;
            stat_muls = 1f;
            stat_base = val;
            stat_curr = val;
            max_chace = stat_base;
        }
        public void CleanAll()
        {
            stat_item = 0;
            stat_mods = 0;
            Recalculate();
        }
        public void CleanItems()
        {
            stat_item = 0;
            Recalculate();
        }
        public void CleanModifers()
        {
            stat_mods = 0;
            Recalculate();
        }
        public void CleanMultipliers()
        {
            stat_muls = 1f;
            Recalculate();
        }
        public void AddModifer(float val)
        {
            stat_mods += val;
            Recalculate();
        }
        public void AddMultiplier(float val)
        {
            stat_muls += val;
            Recalculate();
        }
        public void RemoveModifer(float val)
        {
            stat_mods -= val;
            Recalculate();
        }
        public void RemoveMultiplier(float val)
        {
            stat_muls -= val;
            Recalculate();
        }
        public void AddItemModifer(float val)
        {
            stat_item += val;
            Recalculate();
        }
        public void IncreaseCurrent(float val)
        {
            stat_curr = MathHelper.Clamp(stat_curr + val, min_chace, max_chace);
        }
        public void DecreaseCurrent(float val)
        {
            stat_curr = MathHelper.Clamp(stat_curr - val, min_chace, max_chace);
        }
        public void SetBase(float val)
        {
            stat_base = val;
            Recalculate();
        }
        public void Recalculate()
        {
            float coff = stat_curr / max_chace;
            max_chace = (stat_base + stat_item + stat_mods) * stat_muls;
            if (max_chace < min_chace) max_chace = min_chace;
            if (stat_curr > 0) stat_curr = (int)(max_chace * coff);
        }
        public override string ToString()
        {
            return $"{stat_curr}/{max_chace}({stat_base}:{stat_item}:{stat_mods}:{stat_muls})";
        }
    }
}