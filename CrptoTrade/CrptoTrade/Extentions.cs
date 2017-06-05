using System;
using System.Globalization;

namespace CrptoTrade
{
    public static class Extentions
    {
        public static bool TryTo(this string input, out decimal value, NumberStyles style = NumberStyles.Any,
            IFormatProvider formatProvider = null)
        {
            return decimal.TryParse(input, style, formatProvider, out value);
        }

        public static decimal ToDecimal(this string input, NumberStyles style = NumberStyles.Any,
            IFormatProvider formatProvider = null)
        {
            if (!input.TryTo(out decimal value, style, formatProvider))
            {
                throw new Exception($"{input} not a decimal");
            }
            return value;
        }
    }
}