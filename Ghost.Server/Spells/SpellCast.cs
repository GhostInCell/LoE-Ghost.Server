using Ghost.Server.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ghost.Server.Core.Classes;

namespace Ghost.Server.Spells
{
    public enum SpellCastResult
    {
        OK,
        Fail,
    }
    public class SpellCast
    {
        private const int CastingFlag = 0x00000100;
        private const int ChannelingFlag = 0x00000200;

        private int m_state;

        public bool IsBusy
        {
            get
            {
                return (m_state & (CastingFlag | ChannelingFlag)) != 0;
            }
        }

        public SpellCast()
        {
            m_state = 0;
        }

        public void Update(int delta)
        {

        }

        public bool CanCast(int id, int upgrade)
        {
            return true;
        }

        public SpellCastResult Initialize(int id, int upgrade, TargetEntry target)
        {
            return SpellCastResult.OK;
        }
    }
}