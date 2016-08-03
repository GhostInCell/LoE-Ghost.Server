using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ghost.Server.Objects
{
    public enum HighId : ushort
    {
        Null = 0x0000,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 8, Size = 8)]
    public struct ObjectGuid : IEquatable<ObjectGuid>, IComparable<ObjectGuid>
    {
        private const uint LOW_MASK = 0xFFFFFF;

        [FieldOffset(0)]
        private ulong m_full;
        [FieldOffset(0)]
        private uint m_low;
        [FieldOffset(4)]
        private uint m_high;

        [FieldOffset(3)]
        private uint m_entry;
        [FieldOffset(6)]
        private HighId m_highId;

        public uint Low
        {
            get
            {
                return m_low & LOW_MASK;
            }
            private set
            {
                m_low &= ~LOW_MASK;
                m_low |= value & LOW_MASK;
            }
        }

        public uint Entry
        {
            get
            {
                return m_entry & LOW_MASK;
            }
            private set
            {
                m_entry &= ~LOW_MASK;
                m_entry |= value & LOW_MASK;
            }
        }

        public HighId High
        {
            get
            {
                return m_highId;
            }
            private set
            {
                m_highId = value;
            }
        }

        public ObjectGuid(ulong full)
        {
            this = default(ObjectGuid);
            m_full = full;
        }

        public override string ToString()
        {
            return $"ObjectGuid[{((ushort)m_highId):X4}:{m_entry:X6}:{m_low:X6}]";
        }

        public override int GetHashCode()
        {
            return m_full.GetHashCode();
        }

        public bool Equals(ObjectGuid other)
        {
            return m_full == other.m_full;
        }

        public int CompareTo(ObjectGuid other)
        {
            return m_full.CompareTo(other.m_full);
        }

        public override bool Equals(object obj)
        {
            return (obj is ObjectGuid) ? Equals((ObjectGuid)obj) : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(ObjectGuid value)
        {
            return value.m_highId != HighId.Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ObjectGuid(long value)
        {
            return new ObjectGuid((ulong)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ObjectGuid(ulong value)
        {
            return new ObjectGuid(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator long(ObjectGuid value)
        {
            return (long)value.m_full;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(ObjectGuid value)
        {
            return value.m_full;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ObjectGuid left, ObjectGuid right)
        {
            return left.m_full == right.m_full;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ObjectGuid left, ObjectGuid right)
        {
            return left.m_full != right.m_full;
        }
    }
}