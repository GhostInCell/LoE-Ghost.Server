using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Classes
{
    public class StatValue
    {
        private float stat_muls;
        private float stat_mods;
        private float stat_item;

        private float stat_base;
        private float stat_curr;

        private float min_cache;
        private float max_cache;

        public float Max
        {
            get { return max_cache; }
        }

        public float Min
        {
            get { return min_cache; }
        }

        public float Current
        {
            get { return stat_curr; }
        }

        public StatValue(float value)
        {
            min_cache = 0f;
            stat_muls = 1f;
            stat_base = value;
            stat_curr = value;
            max_cache = value;
        }

        public void CleanAll()
        {
            stat_item = 0;
            stat_mods = 0;
            stat_muls = 1f;
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

        public void SetBase(float val)
        {
            stat_base = val;
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

        public void AddItemModifer(float val)
        {
            stat_item += val;
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

        public void RemoveItemModifer(float val)
        {
            stat_item -= val;
            Recalculate();
        }

        public void IncreaseCurrent(float val)
        {
            stat_curr = MathHelper.Clamp(stat_curr + val, min_cache, max_cache);
        }

        public void DecreaseCurrent(float val)
        {
            stat_curr = MathHelper.Clamp(stat_curr - val, min_cache, max_cache);
        }

        public override string ToString()
        {
            return $"{stat_curr}/{max_cache}[({stat_base}+{stat_item}+{stat_mods})*{stat_muls}]";
        }

        private void Recalculate()
        {
            float coff = stat_curr / max_cache;
            max_cache = (stat_base + stat_item + stat_mods) * stat_muls;
            if (max_cache < min_cache) max_cache = min_cache;
            if (stat_curr > 0) stat_curr = (int)(max_cache * coff);
        }
    }
}