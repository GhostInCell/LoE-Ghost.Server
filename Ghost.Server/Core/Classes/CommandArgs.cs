using Ghost.Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Core.Classes
{
    public class CommandArgs
    {
        private int m_index;
        private string[] m_args;

        public string Command
        {
            get
            {
                return m_args[0];
            }
        }

        public CommandArgs(string text)
        {
            m_index = 1;
            m_args = text.SplitArguments();
        }

        public bool TryGet(out bool value)
        {
            if (m_index < m_args.Length && bool.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = false;
            return false;
        }

        public bool TryGet(out sbyte value)
        {
            if (m_index < m_args.Length && sbyte.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out short value)
        {
            if (m_index < m_args.Length && short.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out int value)
        {
            if (m_index < m_args.Length && int.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out long value)
        {
            if (m_index < m_args.Length && long.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out byte value)
        {
            if (m_index < m_args.Length && byte.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out ushort value)
        {
            if (m_index < m_args.Length && ushort.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out uint value)
        {
            if (m_index < m_args.Length && uint.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out ulong value)
        {
            if (m_index < m_args.Length && ulong.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out byte[] value)
        {
            if (m_index < m_args.Length && m_args[m_index].TryParseBase64(out value))
            {
                m_index++;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGet(out string value)
        {
            if (m_index < m_args.Length)
            {
                value = m_args[m_index++].Trim(StringExt.Quote);
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGet(out float value)
        {
            if (m_index < m_args.Length && float.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out double value)
        {
            if (m_index < m_args.Length && double.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out decimal value)
        {
            if (m_index < m_args.Length && decimal.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(out DateTime value)
        {
            if (m_index < m_args.Length && DateTime.TryParse(m_args[m_index], out value))
            {
                m_index++;
                return true;
            }
            value = DateTime.MinValue;
            return false;
        }

        public bool TryPeek(out string value)
        {
            if (m_index < m_args.Length)
            {
                value = m_args[m_index].Trim(StringExt.Quote);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public CommandArgs Skip(int count)
        {
            m_index += count;
            return this;
        }

        public IEnumerable<string> GetArgs()
        {
            return m_args;
        }

        public IEnumerable<string> GetCurrentArgs()
        {
            return m_args.Skip(m_index);
        }
    }
}
