using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OneSTools.TechLog
{
    public static class StringBuilderExtensions
    {
        public static int IndexOf(this StringBuilder stringBuilder, char value)
        {
            return IndexOf(stringBuilder, value, 0);
        }

        public static int IndexOf(this StringBuilder stringBuilder, char value, int startIndex)
        {
            for (int i = startIndex; i < stringBuilder.Length; i++)
                if (stringBuilder[i] == value)
                    return i;

            return -1;
        }

        public static int IndexOfAny(this StringBuilder stringBuilder, char[] values)
        {
            return IndexOfAny(stringBuilder, values, 0);
        }

        public static int IndexOfAny(this StringBuilder stringBuilder, char[] values, int startIndex)
        {
            for (int i = startIndex; i < stringBuilder.Length; i++)
                if (values.Contains(stringBuilder[i]))
                    return i;

            return -1;
        }
    }
}
