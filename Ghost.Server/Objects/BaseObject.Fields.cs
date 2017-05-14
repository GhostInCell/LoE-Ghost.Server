using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ghost.Server.Objects
{
    public partial class BaseObject
    {
        private Dictionary<int, object> m_fields;

        public void SetField<TKey, TValue>(TKey key, TValue value)
            where TKey : struct, IConvertible
        {
            m_fields[key.ToInt32(CultureInfo.InvariantCulture)] = value;
        }

        public bool TryGetField<TKey, TValue>(TKey key, out TValue value)
            where TKey : struct, IConvertible
        {
            if (m_fields.TryGetValue(key.ToInt32(CultureInfo.InvariantCulture), out var objValue) && objValue is TValue)
            {
                value = (TValue)objValue;
                return true;
            }
            value = default(TValue);
            return false;
        }
    }
}