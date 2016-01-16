namespace Ghost.Server.Core.Classes
{
    public class StatHelper
    {
        private float stat_curr;
        private float stat_item;
        private float stat_base;

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
            stat_base = val;
            stat_curr = val;
            max_chace = stat_base;
        }
        public void Clean()
        {
            stat_item = 0;
            Recalculate();
        }
        public void UpdateItem(float val)
        {
            stat_item += val;
            Recalculate();
        }
        public void IncreaseCurrent(float val)
        {
            float @new = stat_curr + val;
            if (@new < 0) @new = 0;
            else if (@new > max_chace) @new = max_chace;
            stat_curr = @new;
        }
        public void DecreaseCurrent(float val)
        {
            float @new = stat_curr - val;
            if (@new < 0) @new = 0;
            else if (@new > max_chace) @new = max_chace;
            stat_curr = @new;
        }
        public void SetBase(float val)
        {
            stat_base = val;
            Recalculate();
        }
        public void Recalculate()
        {
            float coff = stat_curr / max_chace;
            max_chace = stat_item + stat_base;
            if (stat_curr > 0) stat_curr = (int)(max_chace * coff);
        }
        public override string ToString()
        {
            return $"{stat_curr}/{max_chace}({stat_base}:{stat_item})";
        }
    }
}