using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime CompareMax(this DateTime first, DateTime second)
        {
            if (first >= second)
            {
                return first;
            }
            return second;
           
        }

        public static DateTime CompareMin(this DateTime first, DateTime second)
        {
            if (second >= first)
            {
                return first;
            }
            return second;

        }

        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            return Math.Abs((lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year));
        }
    }
}
