using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Ghost.Server.Utilities
{
    public static class StringExt
    {
        public static readonly char[] Quote = new[] { '"' };
        public static readonly IFormatProvider DefaultFormat;

        static StringExt()
        {
            var provider = new CultureInfo(CultureInfo.InvariantCulture.LCID);
            provider.NumberFormat.NumberDecimalSeparator = ".";
            DefaultFormat = provider;
        }

        private static readonly Regex s_rgex_match_01 = new Regex(@"\s", RegexOptions.Compiled);
        private static readonly Regex s_rgex_match_02 = new Regex(@"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.Compiled);
        private static readonly Regex s_rgex_split_01 = new Regex(" (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)", RegexOptions.Compiled);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasWhiteSpace(this string value)
        {
            return s_rgex_match_01.IsMatch(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] SplitArguments(this string value)
        {
            return s_rgex_split_01.Split(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseBase64(this string value, out byte[] data)
        {
            var str = value.Trim();
            if ((str.Length % 4 == 0) && s_rgex_match_02.IsMatch(value))
            {
                data = Convert.FromBase64String(str);
                return true;
            }
            data = null;
            return false;
        }
    }
}
